using SecurityDriven.Core;

namespace TechChallenge.ChaosMonkey;

public class ErrorChaosMonkey(ChausChance chaosChance, CryptoRandom cryptoRandom)
    : BaseChaosMonkey(chaosChance, cryptoRandom)
{
    protected override ValueTask DoChaos() => throw new Exception("Chaos in the Program!");
}