
namespace TechChallenge.Calculations.Api.Services;

public interface IEmissionsCalculator
{
    Task<double> CalculateTotalEmissionsAsync(string userId, long from, long to);
}