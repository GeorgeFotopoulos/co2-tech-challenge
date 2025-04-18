namespace TechChallenge.Measurements.Api.Data;

public interface IMeasurementsRepository
{
    IAsyncEnumerable<Measurement> GetMeasurementsAsync(
        string userId,
        long from,
        long to,
        CancellationToken cancellationToken);
}