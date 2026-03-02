using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using Libra.Virgo;
using Libra.Virgo.Models;
using Libra.Virgo.Enum;

namespace Libra.Server.Service.Agent
{
    /// <summary>
    /// Agent 列表管理
    /// </summary>
    public class AgentList
    {
        private static readonly Lazy<AgentList> _instance = new(() => new AgentList());
        public static AgentList Instance => _instance.Value;

        public static ObservableCollection<AgentInfo> AgentInfos { get; } = new();

        public static ConcurrentDictionary<Guid, AgentSession> AgentSessions { get; } = new();

        public static int OnlineCount => AgentSessions.Count;
        public static int TotalCount => AgentInfos.Count;

        public static void UpsertAgent(AgentInfo info, AgentSession session)
        {
            var existingSession = AgentSessions.TryGetValue(info.AgentId, out var oldSession) ? oldSession : null;
            if (existingSession != null)
            {
                existingSession.Disconnected -= OnSessionDisconnected;
            }

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

            AgentSessions[info.AgentId] = session;

            session.Disconnected += OnSessionDisconnected;
        }

        private static void OnSessionDisconnected(object sender, AgentSessionEventArgs e)
        {
            RemoveAgent(e.AgentId);
        }

        public static void RemoveAgent(Guid agentId)
        {
            var agent = GetAgent(agentId);
            if (agent != null)
            {
                AgentInfos.Remove(agent);
            }
            AgentSessions.TryRemove(agentId, out _);
        }

        public static AgentInfo? GetAgent(Guid agentId)
        {
            return AgentInfos.FirstOrDefault(a => a.AgentId == agentId);
        }

        public static VirgoConnection? GetConnection(Guid agentId)
        {
            return null;
        }

        public static ObservableCollection<AgentInfo> GetOnlineAgents()
        {
            return new ObservableCollection<AgentInfo>(
                AgentInfos.Where(a => AgentSessions.ContainsKey(a.AgentId))
            );
        }

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

        public static async Task<bool> SendMessageToAgentAsync(Guid agentId, VirgoMessageType type, object data)
        {
            if (AgentSessions.TryGetValue(agentId, out var session))
            {
                return await session.SendMessageAsync(type, data);
            }
            return false;
        }

        public static event EventHandler<AgentListChangedEventArgs>? AgentListChanged;

        protected virtual void OnAgentListChanged(AgentListChangedEventArgs e)
        {
            AgentListChanged?.Invoke(this, e);
        }
    }

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