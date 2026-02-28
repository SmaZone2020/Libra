using System.Collections.Generic;
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
[JsonSerializable(typeof(List<Disk>))]
[JsonSerializable(typeof(FileModel))]
[JsonSerializable(typeof(FileModel[]))]
[JsonSerializable(typeof(CommandModel))]
[JsonSerializable(typeof(CommandResult))]
[JsonSerializable(typeof(ScreenFrame))]
[JsonSerializable(typeof(DiffBlock))]
[JsonSerializable(typeof(List<DiffBlock>))]
public partial class AgentJsonContext : JsonSerializerContext
{
}
