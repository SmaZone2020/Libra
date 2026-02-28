using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Libra.Server.Service.Agent
{
    public static class FileDownloadManager
    {
        private static readonly ConcurrentDictionary<Guid, Channel<byte[]?>> _channels = new();

        public static Channel<byte[]?> Register(Guid taskId)
        {
            var ch = Channel.CreateBounded<byte[]?>(new BoundedChannelOptions(32)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
            _channels[taskId] = ch;
            return ch;
        }

        public static bool IsActive(Guid taskId) => _channels.ContainsKey(taskId);

        public static void PushChunk(Guid taskId, byte[] data)
        {
            if (_channels.TryGetValue(taskId, out var ch))
                ch.Writer.TryWrite(data);
        }

        public static void Complete(Guid taskId)
        {
            if (_channels.TryRemove(taskId, out var ch))
                ch.Writer.TryComplete();
        }

        public static void Cancel(Guid taskId)
        {
            if (_channels.TryRemove(taskId, out var ch))
                ch.Writer.TryComplete(new OperationCanceledException());
        }
    }
}
