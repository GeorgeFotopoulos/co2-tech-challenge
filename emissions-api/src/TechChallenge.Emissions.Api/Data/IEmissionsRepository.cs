namespace TechChallenge.Emissions.Api.Data;

public interface IEmissionsRepository
{
    IAsyncEnumerable<Emission> GetAsync(
        long from,
        long to,
        CancellationToken cancellationToken);
}