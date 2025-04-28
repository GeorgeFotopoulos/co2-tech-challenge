using TechChallenge.Measurements.Api;

namespace TechChallenge.Calculations.Api.Clients;

public interface IMeasurementsApiClient
{
    Task<List<MeasurementResponse>> GetMeasurementsAsync(string userId, long from, long to);
}
