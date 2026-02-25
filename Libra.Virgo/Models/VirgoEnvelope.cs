using Libra.Virgo.Enum;

namespace Libra.Virgo.Models;

public class VirgoEnvelope<T>
{
    public VirgoMessageType Type { get; set; }
    public T? Data { get; set; }
}