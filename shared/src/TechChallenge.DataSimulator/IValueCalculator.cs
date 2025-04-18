namespace TechChallenge.DataSimulator;

public interface IValueCalculator<in TContext, out TValue>
{
    TValue Calculate(TContext context);
}