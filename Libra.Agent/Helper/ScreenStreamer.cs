using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Libra.Agent.Helper
{
    /// <summary>
    /// 差异屏幕流，持续捕获并只发送变化区块
    /// </summary>
    internal static class ScreenStreamer
    {
        private static CancellationTokenSource? _cts;
        private static Guid _streamId;
        private static string _quality = "720p";
        private static readonly SemaphoreSlim _gate = new(1, 1);

        public static async Task StartAsync(Guid streamId, string quality = "720p", int targetFps = 10)
        {
            await _gate.WaitAsync();
            try
            {
                // 停止旧流（如果有）
                _cts?.Cancel();
                _cts?.Dispose();

                _streamId = streamId;
                _quality  = quality;
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => RunAsync(targetFps, _cts.Token));
            }
            finally
            {
                _gate.Release();
            }
        }

        public static void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private static async Task RunAsync(int fps, CancellationToken ct)
        {
            int intervalMs = 1000 / fps;
            byte[]? prevPixels = null;
            int prevWidth = 0, prevHeight = 0;

            while (!ct.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var (frame, rawPixels) = MonitorHelper.CaptureWithDiff(
                        prevPixels, prevWidth, prevHeight, _streamId, _quality);

                    prevPixels = rawPixels;
                    prevWidth = frame.ScreenWidth;
                    prevHeight = frame.ScreenHeight;

                    // 无变化时跳过发送
                    if (!frame.IsFull && (frame.Blocks == null || frame.Blocks.Count == 0))
                    {
                        sw.Stop();
                        int skip = Math.Max(0, intervalMs - (int)sw.ElapsedMilliseconds);
                        if (skip > 0) await Task.Delay(skip, ct);
                        continue;
                    }

                    var frameJson = JsonSerializer.Serialize(frame, AgentJsonContext.Default.ScreenFrame);
                    await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult
                    {
                        TaskId = _streamId,
                        Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(frameJson)),
                        EndTime = DateTime.Now
                    });
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    //D Console.WriteLine($"ScreenStreamer 错误: {ex.Message}");
                    await Task.Delay(1000, ct);
                    continue;
                }

                sw.Stop();
                int delay = Math.Max(0, intervalMs - (int)sw.ElapsedMilliseconds);
                if (delay > 0) await Task.Delay(delay, ct);
            }
        }
    }
}
