using Libra.Server.Models;
using Libra.Server.Models.Agent;
using Libra.Server.Service;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Libra.Server.Service
{
    public class TcpServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        private readonly int _port;

        public event EventHandler<AgentConnectedEventArgs> AgentConnected;
        public event EventHandler<AgentDisconnectedEventArgs> AgentDisconnected;

        public TcpServer(int port = 8888)
        {
            _port = port;
        }

        public void Start()
        {
            _isRunning = true;
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"TCP 服务器已启动，端口: {_port}");

            // 开始接受连接
            Task.Run(() => AcceptConnectionsAsync());
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            Console.WriteLine("TCP 服务器已停止");
        }

        private async Task AcceptConnectionsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine($"新客户端连接: {client.Client.RemoteEndPoint}");

                    // 处理客户端连接
                    _ = HandleClientAsync(client);
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Console.WriteLine($"接受连接时出错: {ex.Message}");
                    }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = null;
            Guid agentId = Guid.NewGuid();
            bool registered = false;
            DateTime connectionTime = DateTime.Now;

            try
            {
                stream = client.GetStream();

                // 创建接收数据的缓冲区
                byte[] buffer = new byte[4096];
                int bytesRead;

                // 设置注册超时
                var registrationTimeout = Task.Delay(10000); // 10 秒
                var receiveTask = stream.ReadAsync(buffer, 0, buffer.Length);

                // 等待注册数据或超时
                var completedTask = await Task.WhenAny(receiveTask, registrationTimeout);

                if (completedTask == registrationTimeout)
                {
                    // 注册超时 - 断开连接
                    Console.WriteLine($"客户端 {client.Client.RemoteEndPoint} 在 10 秒内未能完成注册");
                    return;
                }

                // 读取注册数据
                bytesRead = await receiveTask;
                if (bytesRead == 0)
                {
                    Console.WriteLine($"客户端 {client.Client.RemoteEndPoint} 在注册前断开连接");
                    return;
                }

                // 处理注册数据
                string registrationData = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"收到注册数据: {registrationData}");

                // Parse AgentInfo
                var agentInfo = JsonSerializer.Deserialize<AgentInfo>(registrationData);
                if (agentInfo == null)
                {
                    Console.WriteLine($"来自 {client.Client.RemoteEndPoint} 的注册数据无效");
                    return;
                }

                // Set agent ID and update timestamp
                agentInfo.AgentId = agentId;
                agentInfo.LastHeartbeat = DateTime.Now;
                Console.WriteLine($"接收到 Agent 注册信息: ID={agentId}, Location={agentInfo.Location}, OS={agentInfo.OsVersion}");

                // 创建 agent 会话
                var session = new AgentSession(client, agentId);

                // 添加到 agent 列表
                AgentList.UpsertAgent(agentInfo, client, session);
                registered = true;

                // 触发连接事件
                AgentConnected?.Invoke(this, new AgentConnectedEventArgs { AgentId = agentId, AgentInfo = agentInfo });
                Console.WriteLine($"Agent {agentId} 注册成功");

                // 开始接收数据
                while (_isRunning && client.Connected)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    // 处理接收到的数据
                    string data = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"收到来自 agent {agentId} 的数据: {data}");

                    // 更新心跳
                    session.UpdateHeartbeat();

                    // 发送响应（暂时回显）
                    byte[] response = System.Text.Encoding.UTF8.GetBytes($"服务器已收到: {data}");
                    await stream.WriteAsync(response, 0, response.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理客户端 {client.Client.RemoteEndPoint} 时出错: {ex.Message}");
            }
            finally
            {
                // 清理
                if (registered)
                {
                    AgentList.RemoveAgent(agentId);
                    AgentDisconnected?.Invoke(this, new AgentDisconnectedEventArgs { AgentId = agentId });
                }
                Console.WriteLine($"客户端 {client.Client.RemoteEndPoint} 已断开连接");
                stream?.Dispose();
                client.Dispose();
                
            }
        }
    }

    public class AgentConnectedEventArgs : EventArgs
    {
        public Guid AgentId { get; set; }
        public AgentInfo AgentInfo { get; set; }
    }

    public class AgentDisconnectedEventArgs : EventArgs
    {
        public Guid AgentId { get; set; }
    }
}