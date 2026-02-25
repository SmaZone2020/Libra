using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using Libra.Virgo;
using Libra.Virgo.Models;
using Libra.Virgo.Enum;

namespace Libra.Server.Service.Agent
{
    /// <summary>
    /// Agent 列表管理（线程安全版本）
    /// </summary>
    public class AgentList
    {
        #region 单例

        private static readonly Lazy<AgentList> _instance = new(() => new AgentList());
        public static AgentList Instance => _instance.Value;

        #endregion

        #region 线程安全集合

        /// <summary>
        /// Agent 信息列表 (UI 绑定用)
        /// </summary>
        public static ObservableCollection<AgentInfo> AgentInfos { get; } = new();

        /// <summary>
        /// Agent 连接字典 (快速查找用) - 暂时未使用
        /// </summary>
        // public static ConcurrentDictionary<Guid, TcpClient> AgentConnections { get; } = new();

        /// <summary>
        /// Agent 会话字典 (内部使用)
        /// </summary>
        public static ConcurrentDictionary<Guid, AgentSession> AgentSessions { get; } = new();

        /// <summary>
        /// 消息队列服务（暂时未实现）
        /// </summary>
        // public static MessageQueue MessageQueue { get; } = new();

        #endregion

        #region 统计信息

        public static int OnlineCount => AgentSessions.Count;
        public static int TotalCount => AgentInfos.Count;

        #endregion

        #region 操作方法

        /// <summary>
        /// 添加或更新 Agent
        /// </summary>
        public static void UpsertAgent(AgentInfo info, AgentSession session)
        {
            // 移除旧会话的事件监听
            var existingSession = AgentSessions.TryGetValue(info.AgentId, out var oldSession) ? oldSession : null;
            if (existingSession != null)
            {
                existingSession.Disconnected -= OnSessionDisconnected;
            }

            // 更新 ObservableCollection (需在 UI 线程)
            var existing = GetAgent(info.AgentId);
            if (existing != null)
            {
                var index = AgentInfos.IndexOf(existing);
                AgentInfos[index] = info;
            }
            else
            {
                AgentInfos.Add(info);
            }

            // 更新会话
            AgentSessions[info.AgentId] = session;

            // 添加新会话的事件监听
            session.Disconnected += OnSessionDisconnected;
        }

        /// <summary>
        /// 处理会话断开连接的事件
        /// </summary>
        private static void OnSessionDisconnected(object sender, AgentSessionEventArgs e)
        {
            Console.WriteLine($"会话断开连接事件: AgentId = {e.AgentId}");
            RemoveAgent(e.AgentId);
        }

        /// <summary>
        /// 移除 Agent
        /// </summary>
        public static void RemoveAgent(Guid agentId)
        {
            var agent = GetAgent(agentId);
            if (agent != null)
            {
                AgentInfos.Remove(agent);
            }
            AgentSessions.TryRemove(agentId, out _);
        }

        /// <summary>
        /// 获取 Agent 信息
        /// </summary>
        public static AgentInfo? GetAgent(Guid agentId)
        {
            return AgentInfos.FirstOrDefault(a => a.AgentId == agentId);
        }

        /// <summary>
        /// 获取 Agent 连接
        /// </summary>
        public static VirgoConnection? GetConnection(Guid agentId)
        {
            // 暂时返回 null，因为我们没有直接存储 VirgoConnection
            // 实际使用时，应该从 AgentSession 中获取
            return null;
        }

        /// <summary>
        /// 获取在线 Agent 列表
        /// </summary>
        public static ObservableCollection<AgentInfo> GetOnlineAgents()
        {
            return new ObservableCollection<AgentInfo>(
                AgentInfos.Where(a => AgentSessions.ContainsKey(a.AgentId))
            );
        }

        /// <summary>
        /// 按条件筛选 Agent
        /// </summary>
        public static ObservableCollection<AgentInfo> FilterAgents(
            string? os = null,
            bool? isAdmin = null,
            bool? isIdle = null)
        {
            var query = AgentInfos.AsEnumerable();

            if (!string.IsNullOrEmpty(os))
                query = query.Where(a => a.OsVersion?.Contains(os, StringComparison.OrdinalIgnoreCase) == true);

            if (isAdmin.HasValue)
                query = query.Where(a => a.Privilege.IsAdmin == isAdmin.Value);

            if (isIdle.HasValue)
                query = query.Where(a => a.IsIdle == isIdle.Value);

            return new ObservableCollection<AgentInfo>(query);
        }

        /// <summary>
        /// 向特定 Agent 发送消息
        /// </summary>
        /// <param name="agentId">Agent ID</param>
        /// <param name="message">要发送的消息内容</param>
        /// <returns>是否发送成功</returns>
        public static async Task<bool> SendMessageToAgentAsync(Guid agentId, VirgoMessageType type, object data)
        {
            // 直接发送
            if (AgentSessions.TryGetValue(agentId, out var session))
            {
                return await session.SendMessageAsync(type, data);
            }
            return false;
        }

        #endregion

        #region 事件

        public static event EventHandler<AgentListChangedEventArgs>? AgentListChanged;

        protected virtual void OnAgentListChanged(AgentListChangedEventArgs e)
        {
            AgentListChanged?.Invoke(this, e);
        }

        #endregion
    }

    /// <summary>
    /// Agent 列表变更事件
    /// </summary>
    public class AgentListChangedEventArgs : EventArgs
    {
        public Guid AgentId { get; set; }
        public ChangeType ChangeType { get; set; }
        public AgentInfo? AgentInfo { get; set; }
    }

    public enum ChangeType
    {
        Added,
        Removed,
        Updated,
        Heartbeat
    }
}