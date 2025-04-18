using SecurityDriven.Core;

namespace TechChallenge.ChaosMonkey;

public class DelayChaosMonkey(TimeSpan delay, ChausChance chaosChance, CryptoRandom cryptoRandom)
    : BaseChaosMonkey(chaosChance, cryptoRandom)
{
    protected override async ValueTask DoChaos() => await Task.Delay(delay).ConfigureAwait(false);
}