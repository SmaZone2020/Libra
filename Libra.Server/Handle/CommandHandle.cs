using Libra.Server.Service.Agent;
using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

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

            // 差异屏幕流帧：不完成一次性任务，直接推送给 SSE 订阅者
            if (ScreenStreamManager.IsActiveStream(Packet.TaskId))
            {
                try
                {
                    var b64 = Packet.Result?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(b64))
                    {
                        var frameJson = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                        var frame = System.Text.Json.JsonSerializer.Deserialize<ScreenFrame>(
                            frameJson, VirgoJsonSerializerOptions);
                        if (frame != null)
                            ScreenStreamManager.TryPushFrame(Packet.TaskId, frame);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"解析屏幕流帧失败: {ex.Message}");
                }
                return;
            }

            if (TaskList.CommandTasks.TryGetValue(Packet.TaskId, out var task))
            {
                task.Result = Packet.Result;
                task.EndTime = Packet.EndTime;
                task.IsCompleted = true;
            }
            else if (TaskList.FrameTasks.TryGetValue(Packet.TaskId, out var frame2))
            {
                frame2.Result = Packet.Result;
                frame2.EndTime = Packet.EndTime;
                frame2.IsCompleted = true;
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

        private static readonly System.Text.Json.JsonSerializerOptions VirgoJsonSerializerOptions = new()
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
    }
}

