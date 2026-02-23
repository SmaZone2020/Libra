using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Libra.Server.Models.Agent;
using Libra.Server.Models;

namespace Libra.Server.Service
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
        /// Agent 连接字典 (快速查找用)
        /// </summary>
        public static ConcurrentDictionary<Guid, TcpClient> AgentConnections { get; } = new();

        /// <summary>
        /// Agent 会话字典 (内部使用)
        /// </summary>
        public static ConcurrentDictionary<Guid, AgentSession> AgentSessions { get; } = new();

        #endregion

        #region 统计信息

        public static int OnlineCount => AgentConnections.Count;
        public static int TotalCount => AgentInfos.Count;

        #endregion

        #region 操作方法

        /// <summary>
        /// 添加或更新 Agent
        /// </summary>
        public static void UpsertAgent(AgentInfo info, TcpClient client, AgentSession session)
        {
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

            // 更新连接和会话
            AgentConnections[info.AgentId] = client;
            AgentSessions[info.AgentId] = session;
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
            AgentConnections.TryRemove(agentId, out _);
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
        public static TcpClient? GetConnection(Guid agentId)
        {
            return AgentConnections.TryGetValue(agentId, out var client) ? client : null;
        }

        /// <summary>
        /// 获取在线 Agent 列表
        /// </summary>
        public static ObservableCollection<AgentInfo> GetOnlineAgents()
        {
            return new ObservableCollection<AgentInfo>(
                AgentInfos.Where(a => AgentConnections.ContainsKey(a.AgentId))
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