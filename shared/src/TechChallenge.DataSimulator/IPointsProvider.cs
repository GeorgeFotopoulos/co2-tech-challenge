using System.Collections.Generic;

namespace TechChallenge.DataSimulator;

public interface IPointsProvider
{
    IEnumerable<Point> GetPoints(
        long fromTimestamp,
        long toTimestamp,
        int seed,
        double factor);
}