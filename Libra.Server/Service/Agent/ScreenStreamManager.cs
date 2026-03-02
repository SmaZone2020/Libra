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
    /// 管理屏幕流的 SSE 订阅，按 (agentId, quality) 隔离
    /// </summary>
    public static class ScreenStreamManager
    {
        private static readonly ConcurrentDictionary<(Guid, string), Guid> _agentStreams = new();
        private static readonly ConcurrentDictionary<Guid, StreamState> _streams = new();

        private sealed class StreamState
        {
            private readonly object _lock = new();
            private readonly List<Channel<ScreenFrame>> _channels = [];

            /// <summary>添加订阅者，返回是否首个</summary>
            public bool Add(Channel<ScreenFrame> ch)
            {
                lock (_lock) { _channels.Add(ch); return _channels.Count == 1; }
            }

            /// <summary>移除订阅者，返回是否已空</summary>
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
        /// 订阅屏幕流，首个订阅者触发 Agent 推流
        /// </summary>
        public static async Task<(Guid StreamId, Channel<ScreenFrame> Channel)> SubscribeAsync(
            Guid agentId, string quality)
        {
            var key = (agentId, quality);
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
                    Type      = CommandType.StartScreenStream,
                    Parameter = [quality]
                });
            }

            return (streamId, ch);
        }

        /// <summary>
        /// 取消订阅，最后一个离开时通知 Agent 停止
        /// </summary>
        public static async Task UnsubscribeAsync(
            Guid agentId, string quality, Channel<ScreenFrame> channel)
        {
            var key = (agentId, quality);
            if (!_agentStreams.TryGetValue(key, out var streamId)) return;
            if (!_streams.TryGetValue(streamId, out var state)) return;

            bool empty = state.Remove(channel);
            if (empty)
            {
                _streams.TryRemove(streamId, out _);
                _agentStreams.TryRemove(key, out _);

                await Runtimes.SendMessageToAgent(agentId, VirgoMessageType.Command, new CommandModel
                {
                    TaskId = streamId,
                    Type   = CommandType.StopScreenStream
                });
            }
        }

        /// <summary>推送帧给所有订阅者</summary>
        public static bool TryPushFrame(Guid streamId, ScreenFrame frame)
        {
            if (_streams.TryGetValue(streamId, out var state))
            {
                state.Push(frame);
                return true;
            }
            return false;
        }

        public static bool IsActiveStream(Guid streamId) =>
            _streams.ContainsKey(streamId);
    }
}
