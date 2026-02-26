namespace Libra.Virgo.Models.MessageType;

public class HeartbeatMessage
{
    public string Status { get; set; } = "alive";
    public long Timestamp { get; set; }
}