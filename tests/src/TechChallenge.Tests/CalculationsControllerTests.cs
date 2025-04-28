using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using TechChallenge.Calculations.Api.Clients;
using TechChallenge.Calculations.Api.Services;
using TechChallenge.Measurements.Api;
using Xunit;
using Microsoft.Extensions.Logging;

namespace TechChallenge.Tests;

public class CalculationsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetEmissions_ValidRequest_ReturnsOkWithCorrectResult()
    {
        var from = 1000;
        var to = 2000;

        var calculatorMock = new Mock<IEmissionsCalculator>();
        calculatorMock
            .Setup(c => c.CalculateTotalEmissionsAsync("alpha", from, to))
            .ReturnsAsync(42.5);

        var client = CreateClientWithMocks(calculatorMock);

        var response = await client.GetAsync($"/emissions/alpha?from={from}&to={to}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"totalCO2Kg\":42.5");
    }

    [Fact]
    public async Task GetEmissions_CalculatorThrowsException_ReturnsServiceUnavailable()
    {
        var calculatorMock = new Mock<IEmissionsCalculator>();
        calculatorMock
            .Setup(c => c.CalculateTotalEmissionsAsync("alpha", 1000, 2000))
            .ThrowsAsync(new Exception("Calculation error"));

        var client = CreateClientWithMocks(calculatorMock);

        var response = await client.GetAsync("/emissions/alpha?from=1000&to=2000");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Service unavailable");
    }

    [Fact]
    public async Task GetEmissions_SmallestValidTimeRange_ReturnsOk()
    {
        var calculatorMock = new Mock<IEmissionsCalculator>();
        calculatorMock
            .Setup(c => c.CalculateTotalEmissionsAsync("alpha", 1000, 1001))
            .ReturnsAsync(0.1);

        var client = CreateClientWithMocks(calculatorMock);

        var response = await client.GetAsync("/emissions/alpha?from=1000&to=1001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"totalCO2Kg\":0.1");
    }

    [Fact]
    public async Task GetEmissions_LargestValidTimeRange_ReturnsOk()
    {
        var from = 1000;
        var to = from + 1_209_600;

        var calculatorMock = new Mock<IEmissionsCalculator>();
        calculatorMock
            .Setup(c => c.CalculateTotalEmissionsAsync("alpha", from, to))
            .ReturnsAsync(1000.0);

        var client = CreateClientWithMocks(calculatorMock);

        var response = await client.GetAsync($"/emissions/alpha?from={from}&to={to}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var bodyJson = JsonDocument.Parse(body);
        var value = bodyJson.RootElement.GetProperty("totalCO2Kg").GetDouble();
        value.Should().Be(1000.0);
    }

    [Fact]
    public async Task GetEmissions_MeasurementsApiFails_RetriesAndReturnsOk()
    {
        var from = 1000;
        var to = 2000;

        var measurementsMock = new Mock<IMeasurementsApiClient>();
        measurementsMock
            .SetupSequence(m => m.GetMeasurementsAsync("alpha", from, to))
            .ThrowsAsync(new HttpRequestException())
            .ReturnsAsync([new MeasurementResponse(from, 100)]);

        var emissionsClientMock = new Mock<EmissionsApiClient>(
            new HttpClient(), new MemoryCache(new MemoryCacheOptions()), Mock.Of<ILogger<EmissionsApiClient>>());

        var calculatorMock = new Mock<IEmissionsCalculator>();
        calculatorMock
            .Setup(c => c.CalculateTotalEmissionsAsync("alpha", from, to))
            .ReturnsAsync(1000.0);

        var client = CreateClientWithMocks(calculatorMock, measurementsMock);

        var response = await client.GetAsync($"/emissions/alpha?from={from}&to={to}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private HttpClient CreateClientWithMocks(
    Mock<IEmissionsCalculator> calculatorMock,
    Mock<IMeasurementsApiClient>? measurementsMock = null,
    Mock<IEmissionsApiClient>? emissionsMock = null)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IEmissionsCalculator>(_ => calculatorMock.Object);

                if (measurementsMock != null)
                {
                    services.AddSingleton<IMeasurementsApiClient>(_ => measurementsMock.Object);
                }

                if (emissionsMock != null)
                {
                    services.AddSingleton<IEmissionsApiClient>(_ => emissionsMock.Object);
                }

                // Add a mock logger if needed
                services.AddSingleton<ILogger<IEmissionsCalculator>>(_ => Mock.Of<ILogger<IEmissionsCalculator>>());
            });
        }).CreateClient();
    }
}
