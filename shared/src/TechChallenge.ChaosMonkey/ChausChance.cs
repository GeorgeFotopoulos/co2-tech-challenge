namespace TechChallenge.ChaosMonkey;

public record struct ChausChance(double Probability)
{
    public static ChausChance FromPercentage(double percentage)
    {
        if (percentage is < 0.0 or > 100.0)
            throw new ArgumentOutOfRangeException(nameof(percentage));

        return new ChausChance(percentage / 100.0);
    }
}