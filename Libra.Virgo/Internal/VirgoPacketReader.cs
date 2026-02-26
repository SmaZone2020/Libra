using System.Buffers;
using System.Net.Sockets;

namespace Libra.Virgo;

internal class VirgoPacketReader
{
    private readonly NetworkStream _stream;

    public VirgoPacketReader(NetworkStream stream)
    {
        _stream = stream;
    }

    public async Task<string> ReadAsync(CancellationToken ct)
    {
        byte[] lengthBuffer = new byte[4];

        await ReadExactAsync(lengthBuffer, 4, ct);


        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthBuffer);

        int length = BitConverter.ToInt32(lengthBuffer);

        if (length <= 0 || length > 10 * 1024 * 1024)
            throw new InvalidDataException("Invalid packet size");

        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            await ReadExactAsync(buffer, length, ct);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task ReadExactAsync(byte[] buffer, int length, CancellationToken ct)
    {
        int offset = 0;

        while (offset < length)
        {
            int read = await _stream.ReadAsync(buffer, offset, length - offset, ct);
            if (read == 0)
                throw new IOException("Connection closed");

            offset += read;
        }
    }
}