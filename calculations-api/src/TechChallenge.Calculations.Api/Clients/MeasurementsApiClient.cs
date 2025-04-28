using TechChallenge.Measurements.Api;

namespace TechChallenge.Calculations.Api.Clients;

public class MeasurementsApiClient(HttpClient httpClient) : IMeasurementsApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<MeasurementResponse>> GetMeasurementsAsync(string userId, long from, long to)
    {
        var response = await _httpClient.GetAsync($"/measurements/{userId}?from={from}&to={to}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MeasurementResponse>>() ?? [];
    }
}
