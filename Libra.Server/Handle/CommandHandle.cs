using Libra.Server.Service.Agent;
using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using Newtonsoft.Json;

namespace Libra.Server.Handle
{
    internal class CommandHandle
    {
        static CommandResult Packet = new();
        public static void Handle(object? pack)
        {
            if (pack == null) return;

            //Console.WriteLine($"收到命令结果：{JsonConvert.SerializeObject(pack)}");

            if (pack is Newtonsoft.Json.Linq.JObject jObject)
            {
                Packet = jObject.ToObject<CommandResult>();
            }
            else
            {
                try
                {
                    Packet = (CommandResult)pack;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"类型转换失败: {ex.Message}");
                    return;
                }
            }

            if (Packet == null) return;

            if (TaskList.CommandTasks.TryGetValue(Packet.TaskId, out var task))
            {
                task.Result = Packet.Result;
                task.EndTime = Packet.EndTime;
                task.IsCompleted = true;
            }
            else if (TaskList.FrameTasks.TryGetValue(Packet.TaskId, out var frame))
            {
                frame.Result = Packet.Result;
                frame.EndTime = Packet.EndTime;
                frame.IsCompleted = true;
            }
            else if (TaskList.ExplorerTasks.TryGetValue(Packet.TaskId, out var explorer))
            {
                explorer.Result = Packet.Result;
                explorer.EndTime = Packet.EndTime;
                explorer.IsCompleted = true;

            }
            else if (TaskList.CameraFrameTasks.TryGetValue(Packet.TaskId, out var camera))
            {
                camera.Result = Packet.Result;
                camera.EndTime = Packet.EndTime;
                camera.IsCompleted = true;
            }

        }

    }
}
