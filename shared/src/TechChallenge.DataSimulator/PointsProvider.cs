namespace TechChallenge.DataSimulator;

public class PointsProvider(
    int minTimestampIncrement,
    int maxTimestampIncrement,
    IValueCalculator<SeededContext, double> calculator)
    : BasePointsProvider(calculator)
{
    protected override int GetTimestampIncrement(int seed)
    {
        var random = new Random(seed);
        return random.Next(minTimestampIncrement, maxTimestampIncrement);
    }
}