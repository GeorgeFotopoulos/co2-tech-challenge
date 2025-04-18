namespace TechChallenge.DataSimulator;

public abstract class BasePointsProvider(IValueCalculator<SeededContext, double> calculator) : IPointsProvider
{
    public IEnumerable<Point> GetPoints(
        long fromTimestamp,
        long toTimestamp,
        int seed,
        double factor)
    {
        var step = GetTimestampIncrement(seed);

        var start =
            fromTimestamp % step == 0
                ? fromTimestamp
                : (fromTimestamp / step + 1) * step;
        for (var timestamp = start; timestamp <= toTimestamp; timestamp += step)
        {
            var calculationContext = new SeededContext(timestamp, seed, factor);
            yield return new Point(timestamp, calculator.Calculate(calculationContext));
        }
    }

    protected abstract int GetTimestampIncrement(int seed);
}