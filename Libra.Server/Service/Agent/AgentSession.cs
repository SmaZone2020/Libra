using Libra.Virgo;
using Libra.Virgo.Enum;
using Libra.Virgo.Models;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Libra.Server.Service.Agent
{
    public class AgentSession
    {
        private readonly VirgoConnection _connection;
        private readonly Timer _heartbeatTimer;
        private readonly Timer _idleTimer;
        private DateTime _lastHeartbeat;
        private DateTime _lastMouseActivity;
        private bool _isIdle;

        public event EventHandler<AgentSessionEventArgs> Heartbeat;
        public event EventHandler<AgentSessionEventArgs> IdleStatusChanged;
        public event EventHandler<AgentSessionEventArgs> Disconnected;

        public Guid AgentId { get; set; }
        public bool IsIdle => _isIdle;
        public DateTime LastHeartbeat => _lastHeartbeat;
        public DateTime LastMouseActivity => _lastMouseActivity;
        public bool IsConnected => _connection != null;

        /// <summary>
        /// 发送消息给 Agent
        /// </summary>
        /// <param name="message">要发送的消息内容</param>
        /// <returns>是否发送成功</returns>
        public async Task<bool> SendMessageAsync(VirgoMessageType type, object data)
        {
            if (!IsConnected)
            {
                return false;
            }

            try
            {
                // 先将data对象序列化为JSON字符串
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data, options);
                await _connection.SendAsync(dataJson, type, CancellationToken.None);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送消息给 Agent {AgentId} 时出错: {ex.Message}");
                return false;
            }
        }

        public AgentSession(VirgoConnection connection, Guid agentId)
        {
            _connection = connection;
            AgentId = agentId;
            _lastHeartbeat = DateTime.Now;
            _lastMouseActivity = DateTime.Now;
            _isIdle = false;

            _heartbeatTimer = new Timer(60000);
            _heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;

            _idleTimer = new Timer(30000);
            _idleTimer.Elapsed += IdleTimer_Elapsed;

            _connection.Disconnected += (c) =>
            {
                Stop();
                Disconnected?.Invoke(this, new AgentSessionEventArgs { AgentId = AgentId });
            };

            Start();
        }

        public void Start()
        {
            _heartbeatTimer.Start();
            _idleTimer.Start();
        }

        public void Stop()
        {
            _heartbeatTimer.Stop();
            _idleTimer.Stop();
        }

        public void UpdateHeartbeat()
        {
            _lastHeartbeat = DateTime.Now;
            Heartbeat?.Invoke(this, new AgentSessionEventArgs { AgentId = AgentId });
        }

        public void UpdateMouseActivity(int x, int y)
        {
            _lastMouseActivity = DateTime.Now;
            if (_isIdle)
            {
                _isIdle = false;
                IdleStatusChanged?.Invoke(this, new AgentSessionEventArgs { AgentId = AgentId, IsIdle = false });
            }
        }

        private void HeartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((DateTime.Now - _lastHeartbeat).TotalSeconds > 120)
            {
                // 心跳超时，处理断开连接
                Console.WriteLine($"Agent {AgentId} 心跳超时 {(DateTime.Now - _lastHeartbeat).TotalSeconds}s");
                //Stop();
                //Disconnected?.Invoke(this, new AgentSessionEventArgs { AgentId = AgentId });
            }
        }

        private void IdleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((DateTime.Now - _lastMouseActivity).TotalMinutes >= 2 && !_isIdle)
            {
                _isIdle = true;
                IdleStatusChanged?.Invoke(this, new AgentSessionEventArgs { AgentId = AgentId, IsIdle = true });
            }
        }
    }

    public class AgentSessionEventArgs : EventArgs
    {
        public Guid AgentId { get; set; }
        public bool IsIdle { get; set; }
    }
}
