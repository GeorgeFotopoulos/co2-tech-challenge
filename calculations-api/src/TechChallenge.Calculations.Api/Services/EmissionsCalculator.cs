using TechChallenge.Calculations.Api.Clients;
using Microsoft.Extensions.Logging;

namespace TechChallenge.Calculations.Api.Services;

public class EmissionsCalculator(
    IMeasurementsApiClient measurementsClient,
    IEmissionsApiClient emissionsClient,
    ILogger<EmissionsCalculator> logger) : IEmissionsCalculator
{
    public async Task<double> CalculateTotalEmissionsAsync(string userId, long from, long to)
    {
        logger.LogInformation("Processing emissions calculation for user {UserId} from {From} to {To}", userId, from, to);

        var measurementsTask = measurementsClient.GetMeasurementsAsync(userId, from, to);
        var emissionsTask = emissionsClient.GetEmissionsAsync(from, to);
        await Task.WhenAll(measurementsTask, emissionsTask);

        var measurements = await measurementsTask;
        var emissionFactors = await emissionsTask;

        var buckets = measurements
            .GroupBy(m => m.Timestamp / 900 * 900)
            .OrderBy(g => g.Key);

        double totalCO2 = 0;

        foreach (var bucket in buckets)
        {
            var bucketStart = bucket.Key;
            var bucketEnd = bucketStart + 900;

            // Find matching emission factor
            var factor = emissionFactors
                .FirstOrDefault(e => e.Timestamp >= bucketStart && e.Timestamp < bucketEnd)?.KgPerWattHr ?? 0; // Default to 0 if no factor found

            // Calculate kWh: (avg watts * 0.25) / 1000
            var avgWatts = bucket.Average(m => m.Watts);
            var kWh = (avgWatts * 0.25) / 1000;

            totalCO2 += kWh * factor;
        }

        logger.LogInformation("Calculated {TotalCO2} kg CO₂ for user {UserId} ({From}-{To})", totalCO2, userId, from, to);
        return Math.Round(totalCO2, 4);
    }
}