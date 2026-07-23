namespace Ecoa.Core.Entities;

public class CyclePath
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string GeoJson { get; set; } = string.Empty;
    public double MinLatitude { get; set; }
    public double MaxLatitude { get; set; }
    public double MinLongitude { get; set; }
    public double MaxLongitude { get; set; }
}
