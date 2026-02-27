using Libra.Virgo.Enum;
using Libra.Virgo.Models;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Libra.Virgo;

public class VirgoConnection
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly VirgoPacketReader _reader;

    public Guid Id { get; } = Guid.NewGuid();
    public DateTime LastActive { get; private set; } = DateTime.UtcNow;

    public event Func<VirgoConnection, string, VirgoMessageType, Task>? MessageReceived;
    public event Action<VirgoConnection>? Disconnected;

    public VirgoConnection(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _reader = new VirgoPacketReader(_stream);
    }

    public async Task StartAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var json = await _reader.ReadAsync(ct);

                    if (string.IsNullOrWhiteSpace(json))
                        continue;

                    LastActive = DateTime.UtcNow;

                    try
                    {
                        // 清理JSON字符串，移除可能的空字节和其他无效字符
                        json = json.Trim();
                        json = new string(json.Where(c => c >= 32 || c == '\n' || c == '\r' || c == '\t').ToArray());
                        
                        using var doc = JsonDocument.Parse(json);
                        {
                            var root = doc.RootElement;
                            var typeValue = root.GetProperty("type").GetInt32();
                            var type = (VirgoMessageType)typeValue;

                            string dataJson = root.GetProperty("data").ToString();

                            if (MessageReceived != null)
                                await MessageReceived.Invoke(this, dataJson, type);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"解析消息失败: {ex.Message}, {ex.StackTrace}");
                        Console.WriteLine($"原始消息: {json}");
                    }
                }
            }
            catch { }
            finally
            {
                Disconnect();
            }
        }

    public async Task SendAsync<T>(T data, VirgoMessageType type, CancellationToken ct)
    {
        if (!_client.Connected)
            return;

        var envelope = new VirgoEnvelope<T>()
        {
            Type = type,
            Data = data
        };

        string json = JsonSerializer.Serialize(envelope, VirgoJson.Options);
        
        byte[] payload = Encoding.UTF8.GetBytes(json);

        byte[] length = BitConverter.GetBytes(payload.Length);

        Console.WriteLine($"发送消息: {json} , {length}");


        if (BitConverter.IsLittleEndian)
            Array.Reverse(length);

        try
        {
            await _stream.WriteAsync(length, ct);
            await _stream.WriteAsync(payload, ct);
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("连接已关闭");
        }
    }

    public void Disconnect()
    {
        try { _client.Close(); } catch { }
        Disconnected?.Invoke(this);
    }
}