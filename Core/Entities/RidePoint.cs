namespace Ecoa.Core.Entities;

public class RidePoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RideId { get; set; }
    public Ride Ride { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Accuracy { get; set; }
    public double? Speed { get; set; }
    public DateTime RecordedAt { get; set; }
    public bool? NearCyclePath { get; set; }
    public double? DistanceToPath { get; set; }
}
