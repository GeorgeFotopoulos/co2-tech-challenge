# NOTES.md

This document outlines my approach to solving the CO₂ Tech Challenge, including the architecture and specific implementation choices I made. For each challenge, I describe the issue, the code I wrote, how it works, and how it addresses the problem.

---

## 1. I wanted to **handle unstable Measurements API calls (30% chance of failure)**

I wrote a retry policy using **Polly** with exponential backoff for the Measurements API client:

```csharp
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
```

That’s how I ensured resilience against intermittent failures from the Measurements API.

## 2. I wanted to **prevent the Emissions API 15s delay from blocking**

I implemented a combination of a timeout and retry logic for the Emissions API:

```csharp
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
```

That’s how I prevented long waits while still allowing retries for slow Emissions API responses.

## 3. I wanted to **calculate emissions efficiently with parallel API calls**

I used `Task.WhenAll()` to fetch emissions and measurements concurrently:

```csharp
var measurementsTask = measurementsClient.GetMeasurementsAsync(userId, from, to);
var emissionsTask = emissionsClient.GetEmissionsAsync(from, to);
await Task.WhenAll(measurementsTask, emissionsTask);

var measurements = await measurementsTask;
var emissionFactors = await emissionsTask;
```

That’s how I improved performance by avoiding sequential HTTP requests.

## 4. I wanted to **split measurements into 15-minute periods**

I grouped measurements into 15-minute buckets using:

```csharp
var buckets = measurements
    .GroupBy(m => m.Timestamp / 900 * 900)
    .OrderBy(g => g.Key);
```

That’s how I ensured that each group represents a clean 15-minute window aligned to Unix time.

## 5. I wanted to **calculate kWh correctly from average Watts**

I calculated the energy consumption in kWh for each window like this:

```csharp
var avgWatts = bucket.Average(m => m.Watts);
var kWh = (avgWatts * 0.25) / 1000; // 0.25 for 15 minutes
```

That’s how I converted average power into energy used in each time window.

## 6. I wanted to **match energy with corresponding CO₂ emission factors**

I found the emission factor for each 15-minute window:

```csharp
var factor = emissionFactors
    .FirstOrDefault(e => e.Timestamp >= bucketStart && e.Timestamp < bucketEnd)?.KgPerWattHr ?? 0;
```

That’s how I aligned each kWh value with the correct CO₂ rate.

## 7. I wanted to **calculate total emissions for the entire range**

I multiplied kWh by the emission factor and accumulated the result:

```csharp
totalCO2 += kWh * factor;
```

That’s how I summed up emissions to get the final result.

## 8. I wanted to **validate incoming API requests**

I added constraints to ensure a valid and safe time range:

```csharp
if (from >= to)
{
    return Results.BadRequest("Invalid time range");
}

if (to - from > MaxAllowedTimeRange)
{
    return Results.BadRequest("Time range too large (max 2 weeks)");
}
```

That’s how I prevented invalid queries and protected the system from large payloads.

## 10. I wanted to **make the solution portable and runnable via Docker**

I added Dockerfiles for each of the three services:

### ✅ Calculations API

- Multi-stage Dockerfile with `sdk:8.0` for build and `aspnet:8.0` for runtime.
- Copies all required project references including shared libraries and both Emissions & Measurements APIs.
- Builds and publishes from `calculations-api` folder.

### ✅ Emissions API

- Follows the same multi-stage pattern.
- Copies and restores dependencies from shared libraries.
- Publishes to `/app/publish` and exposes port 80.

### ✅ Measurements API

- Mirrors the structure of the other services.
- Ensures all dependencies are restored and included from shared source folders.

Each Dockerfile:
- Separates **restore**, **build**, and **publish** stages for clean, reproducible builds.
- Ensures that referenced shared projects are copied properly before `dotnet restore`.
- Exposes the service on port 80.

---

I also created a `docker-compose.yml` file to wire them together, defining networks, setting `ASPNETCORE_URLS`, and connecting the services by name. That’s how I made it easy to run and test the full system locally.

## 11. I wanted to **write meaningful tests for the Calculations API**

I wrote a `CalculationsControllerTests` class to cover key paths of the `/emissions/{customer}` endpoint.

### ✅ What I tested

- `200 OK` for a valid request with mocked `IEmissionsCalculator`.
- `503 Service Unavailable` when the calculator throws an exception.
- Smallest valid time window (1 second).
- Largest allowed time window (14 days = 1,209,600 seconds).
- Retry behavior when `IMeasurementsApiClient` fails on the first call.

### ✅ How I did it

- Used **`WebApplicationFactory<Program>`** to spin up the API in-memory.
- Mocked `IEmissionsCalculator`, `IMeasurementsApiClient`, and `IEmissionsApiClient` using **Moq**.
- Injected mocks into the test client via `ConfigureServices`.
- Used **FluentAssertions** for clear and expressive assertions.
- Parsed JSON responses to assert exact values like `"totalCO2Kg": 42.5`.