using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;
using System.Threading.RateLimiting;
using TechChallenge.Calculations.Api.Clients;
using TechChallenge.Calculations.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http.Extensions;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "CO₂ Calculator API", Version = "v1" });
        });

        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient();

        // Configure Measurements API (handles 30% failures)
        builder.Services.AddHttpClient<IMeasurementsApiClient, MeasurementsApiClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Apis:Measurements"]!);
        })
        .AddResilienceHandler("measurements-pipeline", builder =>
        {
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 10,
                DelayGenerator = args =>
                {
                    var delay = TimeSpan.FromMilliseconds(500 * Math.Pow(2, args.AttemptNumber - 1));
                    return new ValueTask<TimeSpan?>(delay);
                },
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result?.StatusCode is HttpStatusCode.RequestTimeout or >= HttpStatusCode.InternalServerError ||
                    args.Outcome.Exception is not null)
            });
        });

        // Configure Emissions API (handles 50% delays)
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        }).AddHttpClient<IEmissionsApiClient, EmissionsApiClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Apis:Emissions"]!);
        })
        .AddResilienceHandler("emissions-pipeline", builder =>
        {
            builder
                .AddTimeout(TimeSpan.FromSeconds(10))
                .AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    DelayGenerator = args =>
                    {
                        var delay = TimeSpan.FromMilliseconds(500 * Math.Pow(2, args.AttemptNumber - 1));
                        return new ValueTask<TimeSpan?>(delay);
                    },
                    ShouldHandle = args => ValueTask.FromResult(
                        args.Outcome.Result?.StatusCode is HttpStatusCode.RequestTimeout or >= HttpStatusCode.InternalServerError ||
                        args.Outcome.Exception is TimeoutException)
                });
        });

        // Register services
        builder.Services.AddScoped<IEmissionsCalculator, EmissionsCalculator>();

        var app = builder.Build();

        // Middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        const int MaxAllowedTimeRange = 1_209_600; // 2 weeks in seconds
        app.MapGet("/emissions/{userId}",
            async (
                [FromRoute] string userId,
                [FromQuery] long from,
                [FromQuery] long to,
                [FromServices] IEmissionsCalculator calculator,
                ILogger<Program> logger) =>
            {
                try
                {
                    if (from >= to)
                    {
                        return Results.BadRequest("Invalid time range");
                    }

                    if (to - from > MaxAllowedTimeRange)
                    {
                        return Results.BadRequest("Time range too large (max 2 weeks)");
                    }

                    var result = await calculator.CalculateTotalEmissionsAsync(userId, from, to);
                    return Results.Ok(new { TotalCO2Kg = result });
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Calculation failed for user {UserId}", userId);
                    return Results.Problem("Service unavailable", statusCode: 503);
                }
            })
        .WithName("CalculateEmissions")
        .Produces<double>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status503ServiceUnavailable);

        app.Run();
    }
}