using FlashCap;
using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Libra.Agent.Helper
{
    internal static class CameraStreamer
    {
        private sealed class CameraStream : IAsyncDisposable
        {
            public Guid StreamId;
            public int CameraIndex;
            public CaptureDevice? Device;

            public async ValueTask DisposeAsync()
            {
                if (Device != null)
                {
                    await Device.StopAsync();
                    await Device.DisposeAsync();
                    Device = null;
                }
            }
        }

        private static readonly ConcurrentDictionary<int, CameraStream> _streams = new();

        public static void Start(int cameraIndex, Guid streamId, int targetFps = 10)
        {
            // Stop existing stream for this camera
            if (_streams.TryRemove(cameraIndex, out var old))
                old.DisposeAsync().AsTask().GetAwaiter().GetResult();

            var stream = new CameraStream
            {
                StreamId = streamId,
                CameraIndex = cameraIndex
            };
            _streams[cameraIndex] = stream;

            // Fire and forget the async open
            _ = StartAsync(stream, targetFps);
        }

        private static async Task StartAsync(CameraStream stream, int targetFps)
        {
            try
            {
                var descriptor = CameraHelper.GetDescriptor(stream.CameraIndex);
                if (descriptor == null)
                {
                    //D Console.WriteLine($"CameraStreamer[{stream.CameraIndex}]: device not found");
                    return;
                }

                var chars = CameraHelper.ChooseCharacteristics(descriptor, targetFps);
                if (chars == null)
                {
                    //D Console.WriteLine($"CameraStreamer[{stream.CameraIndex}]: no suitable format");
                    return;
                }

                stream.Device = await descriptor.OpenAsync(
                    chars,
                    async bufferScope =>
                    {
                        await OnFrameArrived(stream, bufferScope);
                    });

                await stream.Device.StartAsync();
            }
            catch (Exception ex)
            {
                //D Console.WriteLine($"CameraStreamer[{stream.CameraIndex}] start error: {ex.Message}");
            }
        }

        private static async Task OnFrameArrived(CameraStream stream, PixelBufferScope bufferScope)
        {
            try
            {
                var imageData = bufferScope.Buffer.CopyImage();
                if (imageData == null || imageData.Length == 0) return;

                var jpegData = CameraHelper.ToJpeg(imageData, 60);

                var frame = new ScreenFrame
                {
                    StreamId = stream.StreamId,
                    IsFull = true,
                    ScreenWidth = 0,
                    ScreenHeight = 0,
                    Data = Convert.ToBase64String(jpegData)
                };

                var frameJson = JsonSerializer.Serialize(frame, AgentJsonContext.Default.ScreenFrame);
                await Runtimes.SendMessage(VirgoMessageType.Command, new CommandResult
                {
                    TaskId = stream.StreamId,
                    Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(frameJson)),
                    EndTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                //D Console.WriteLine($"CameraStreamer[{stream.CameraIndex}] frame error: {ex.Message}");
            }
        }

        public static void Stop(int cameraIndex)
        {
            if (_streams.TryRemove(cameraIndex, out var stream))
                stream.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        public static void StopAll()
        {
            foreach (var key in _streams.Keys)
                Stop(key);
        }
    }
}
