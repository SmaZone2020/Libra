using Libra.Agent.Models.Module;
using Libra.Virgo.Models;
using Libra.Virgo.Models.MessageType;
using System.Text.Json.Serialization;

namespace Libra.Agent;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AgentInfo))]
[JsonSerializable(typeof(City))]
[JsonSerializable(typeof(Disk))]
[JsonSerializable(typeof(CommandModel))]
public partial class AgentJsonContext : JsonSerializerContext
{
}