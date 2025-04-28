using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;
using TechChallenge.Emissions.Api;
using Microsoft.Extensions.Logging;

namespace TechChallenge.Calculations.Api.Clients;

public class EmissionsApiClient(
    HttpClient httpClient,
    IMemoryCache cache,
    ILogger<EmissionsApiClient> logger) : IEmissionsApiClient
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<EmissionResponse>> GetEmissionsAsync(long from, long to)
    {
        var cacheKey = $"emissions-{from}-{to}";

        if (cache.TryGetValue(cacheKey, out List<EmissionResponse>? cached))
        {
            return cached ?? [];
        }

        using var response = await httpClient.GetAsync($"/emissions?from={from}&to={to}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning("No emissions data for {From}-{To}", from, to);
            return [];
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var emissions = JsonSerializer.Deserialize<List<EmissionResponse>>(content, _jsonOptions)
            ?? [];

        cache.Set(cacheKey, emissions, TimeSpan.FromMinutes(15));
        return emissions;
    }
}