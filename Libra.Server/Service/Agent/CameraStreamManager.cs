using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Libra.Server.Service.Agent
{
    /// <summary>
    /// 管理 Agent 摄像头流的 SSE 订阅。
    /// 以 (agentId, cameraIndex) 为 key，不同摄像头互相独立。
    /// 帧使用 ScreenFrame(IsFull=true) 承载 base64 JPEG。
    /// </summary>
    public static class CameraStreamManager
    {
        // (agentId, cameraIndex) → streamId
        private static readonly ConcurrentDictionary<(Guid, int), Guid> _agentStreams = new();

        // streamId → 流状态
        private static readonly ConcurrentDictionary<Guid, StreamState> _streams = new();

        private sealed class StreamState
        {
            private readonly object _lock = new();
            private readonly List<Channel<ScreenFrame>> _channels = [];

            public bool Add(Channel<ScreenFrame> ch)
            {
                lock (_lock) { _channels.Add(ch); return _channels.Count == 1; }
            }

            public bool Remove(Channel<ScreenFrame> ch)
            {
                lock (_lock)
                {
                    _channels.Remove(ch);
                    ch.Writer.TryComplete();
                    return _channels.Count == 0;
                }
            }

            public void Push(ScreenFrame frame)
            {
                lock (_lock)
                {
                    foreach (var ch in _channels)
                        ch.Writer.TryWrite(frame);
                }
            }
        }

        /// <summary>
        /// SSE 端点订阅。首个订阅者向 Agent 发送 StartCameraStream（含 cameraIndex 和 fps）。
        /// </summary>
        public static async Task<(Guid StreamId, Channel<ScreenFrame> Channel)> SubscribeAsync(
            Guid agentId, int cameraIndex, int fps = 10)
        {
            var key      = (agentId, cameraIndex);
            var streamId = _agentStreams.GetOrAdd(key, _ => Guid.NewGuid());
            var state    = _streams.GetOrAdd(streamId, _ => new StreamState());

            var ch = Channel.CreateBounded<ScreenFrame>(new BoundedChannelOptions(60)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });

            bool isFirst = state.Add(ch);
            if (isFirst)
            {
                await Runtimes.SendMessageToAgent(agentId, VirgoMessageType.Command, new CommandModel
                {
                    TaskId    = streamId,
                    Type      = CommandType.StartCameraStream,
                    Parameter = [$"{cameraIndex}", $"{fps}"]
                });
            }

            return (streamId, ch);
        }

        /// <summary>
        /// SSE 连接断开时取消订阅；最后一个订阅者离开则通知 Agent 停止。
        /// </summary>
        public static async Task UnsubscribeAsync(
            Guid agentId, int cameraIndex, Channel<ScreenFrame> channel)
        {
            var key = (agentId, cameraIndex);
            if (!_agentStreams.TryGetValue(key, out var streamId)) return;
            if (!_streams.TryGetValue(streamId, out var state)) return;

            bool empty = state.Remove(channel);
            if (empty)
            {
                _streams.TryRemove(streamId, out _);
                _agentStreams.TryRemove(key, out _);

                await Runtimes.SendMessageToAgent(agentId, VirgoMessageType.Command, new CommandModel
                {
                    TaskId    = streamId,
                    Type      = CommandType.StopCameraStream,
                    Parameter = [$"{cameraIndex}"]
                });
            }
        }

        /// <summary>将来自 Agent 的帧推送给所有订阅者</summary>
        public static bool TryPushFrame(Guid streamId, ScreenFrame frame)
        {
            if (_streams.TryGetValue(streamId, out var state))
            {
                state.Push(frame);
                return true;
            }
            return false;
        }

        public static bool IsActiveStream(Guid streamId) => _streams.ContainsKey(streamId);
    }
}
