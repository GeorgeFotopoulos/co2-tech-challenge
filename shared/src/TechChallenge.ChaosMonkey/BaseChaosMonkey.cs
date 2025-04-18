using SecurityDriven.Core;

namespace TechChallenge.ChaosMonkey;

public abstract class BaseChaosMonkey(ChausChance chaosChance, CryptoRandom cryptoRandom)
    : IChaosMonkey
{
    public ValueTask UnleashChaos() =>
        ShouldUnleashChaos()
            ? DoChaos()
            : ValueTask.CompletedTask;

    protected abstract ValueTask DoChaos();

    protected virtual bool ShouldUnleashChaos()
    {
        var randomValue = cryptoRandom.NextDouble();

        return randomValue < chaosChance.Probability;
    }
}