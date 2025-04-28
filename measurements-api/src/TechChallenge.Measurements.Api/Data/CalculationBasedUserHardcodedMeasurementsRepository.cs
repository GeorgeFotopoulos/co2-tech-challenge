using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using TechChallenge.Common.Exceptions;
using TechChallenge.DataSimulator;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;

namespace TechChallenge.Measurements.Api.Data;

public class CalculationBasedUserHardcodedMeasurementsRepository(
    TimeProvider timeProvider,
    IPointsProvider pointsProvider,
    ILogger<CalculationBasedUserHardcodedMeasurementsRepository> logger
) : IMeasurementsRepository
{
    private static readonly IReadOnlyDictionary<string, double> UserHardcodedFactors =
        new Dictionary<string, double>
        {
            { "alpha", 113.23 },
            { "betta", 214.34 },
            { "gamma", 115.45 },
            { "delta", 136.56 },
            { "epsilon", 517.67 },
            { "zeta", 218.78 },
            { "eta", 619.89 },
            { "theta", 120.00 },
        };

    public async IAsyncEnumerable<Measurement> GetMeasurementsAsync(
        string userId,
        long from,
        long to,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!UserHardcodedFactors.ContainsKey(userId))
        {
            logger.LogWarning("User {userId} was not found", userId);
            throw new NotFoundException("User was not found");
        }

        DateTimeOffset dataLimit = timeProvider.GetUtcNow();
        to = Math.Min(to, dataLimit.ToUnixTimeSeconds());

        var userSeed = CalculateSeed(userId);
        var factor = UserHardcodedFactors[userId];
        foreach (var point in pointsProvider.GetPoints(
                     from,
                     to,
                     userSeed,
                     factor))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new Measurement
            {
                UserId = userId,
                Timestamp = point.Timestamp,
                Watts = point.Value
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