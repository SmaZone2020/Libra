namespace Libra.Virgo.Models;

public class DiskInfo
{
    public string Label { get; set; } = "";
    public string Name { get; set; } = "";
    public string DriveFormat { get; set; } = "";
    public double TotalSize { get; set; }
    public double AvailableSizes { get; set; }
}