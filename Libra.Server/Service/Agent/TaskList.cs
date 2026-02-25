using System.Collections.ObjectModel;
using System.Timers;

namespace Libra.Server.Service.Agent
{
    public class TaskList
    {
        public static Dictionary<Guid, CommandTask> CommandTasks { get; } = [];
        public static Dictionary<Guid, CommandTask> FrameTasks { get; } = [];
        public static Dictionary<Guid, CommandTask> ExplorerTasks { get; } = [];
    }

    public class CommandTask
    {
        public bool IsCompleted { get; set; }
        public Guid AgentId { get; set; }

        public object Result { get; set; } = string.Empty;

        public DateTime StartTime { get; set;} = DateTime.Now;
        public DateTime EndTime { get; set; }
    }
}
