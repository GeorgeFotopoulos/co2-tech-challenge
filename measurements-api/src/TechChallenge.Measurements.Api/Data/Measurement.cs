namespace TechChallenge.Measurements.Api.Data;

public class Measurement
{
    public required string UserId { get; set; }
    public required long Timestamp { get; set; }
    public required double Watts { get; set; }
}