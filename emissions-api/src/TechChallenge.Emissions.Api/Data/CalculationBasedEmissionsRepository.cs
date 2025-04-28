using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using TechChallenge.DataSimulator;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Collections.Generic;
using System;

namespace TechChallenge.Emissions.Api.Data;

public class CalculationBasedEmissionsRepository(
    IPointsProvider pointsProvider,
    TimeProvider timeProvider,
    ILogger<CalculationBasedEmissionsRepository> logger
) : IEmissionsRepository
{
    private const string Kye = "emissions";
    private const double Factor = 10;

    public async IAsyncEnumerable<Emission> GetAsync(
        long from,
        long to,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        DateTimeOffset dataLimit = timeProvider.GetUtcNow().AddDays(1);
        to = Math.Min(to, dataLimit.ToUnixTimeSeconds());
        var userSeed = CalculateSeed(Kye);
        foreach (var point in pointsProvider.GetPoints(
                     from,
                     to,
                     userSeed,
                     Factor))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new Emission
            {
                Timestamp = point.Timestamp,
                KgPerWattHr = point.Value
            };
        }
    }

    private static int CalculateSeed(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return BitConverter.ToInt32(hashBytes, 0);
    }
}