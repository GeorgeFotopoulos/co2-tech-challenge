using TechChallenge.Emissions.Api;

namespace TechChallenge.Calculations.Api.Clients;

public interface IEmissionsApiClient
{
    Task<List<EmissionResponse>> GetEmissionsAsync(long from, long to);
}
