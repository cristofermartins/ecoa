namespace Ecoa.Core.Entities;

public class RideImuSample
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RideId { get; set; }
    public Ride Ride { get; set; } = null!;
    public DateTime RecordedAt { get; set; }
    public double AccelX { get; set; }
    public double AccelY { get; set; }
    public double AccelZ { get; set; }
    public double GyroX { get; set; }
    public double GyroY { get; set; }
    public double GyroZ { get; set; }
}