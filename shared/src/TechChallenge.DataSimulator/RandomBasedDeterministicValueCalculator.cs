using SecurityDriven.Core;
using System;

namespace TechChallenge.DataSimulator;

public class RandomBasedDeterministicValueCalculator : IValueCalculator<SeededContext, double>
{
    public double Calculate(SeededContext context)
    {
        var random = new CryptoRandom(context.Seed);
        var timestampRandom = new CryptoRandom((int)context.Timestamp);

        return random.NextDouble()*timestampRandom.NextDouble()*context.Factor;
    }
}