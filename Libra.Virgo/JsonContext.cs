using Libra.Virgo.Models;
using Libra.Virgo.Models.MessageType;
using System.Text.Json.Serialization;

namespace Libra.Virgo;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AgentInfo))]
[JsonSerializable(typeof(CommandModel))]
[JsonSerializable(typeof(CommandResult))]
[JsonSerializable(typeof(HeartbeatMessage))]
[JsonSerializable(typeof(DiskInfo))]
[JsonSerializable(typeof(VirgoEnvelope<object>))]
[JsonSerializable(typeof(VirgoEnvelope<AgentInfo>))]
[JsonSerializable(typeof(VirgoEnvelope<CommandModel>))]
[JsonSerializable(typeof(VirgoEnvelope<CommandResult>))]
[JsonSerializable(typeof(VirgoEnvelope<HeartbeatMessage>))]
[JsonSerializable(typeof(VirgoEnvelope<string>))]
[JsonSerializable(typeof(string))]
public partial class VirgoJsonContext : JsonSerializerContext
{
}