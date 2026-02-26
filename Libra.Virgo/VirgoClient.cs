using Libra.Virgo.Enum;
using Libra.Virgo.Models;
using System.Net.Sockets;
using System.Text.Json;

namespace Libra.Virgo;

public class VirgoClient
{
    private VirgoConnection? _connection;

    public event Func<string, VirgoMessageType, Task>? MessageReceived;

    public async Task ConnectAsync(string host, int port, AgentInfo agent, CancellationToken ct)
    {
        var tcp = new TcpClient();
        await tcp.ConnectAsync(host, port, ct);

        _connection = new VirgoConnection(tcp);

        _connection.MessageReceived += async (c, dataJson, type) =>
        {
            if (MessageReceived != null)
                await MessageReceived.Invoke(dataJson, type);
        };

        _ = _connection.StartAsync(ct);

        await _connection.SendAsync<string>(JsonSerializer.Serialize(agent, VirgoJson.Options), VirgoMessageType.Register, ct);
    }

    public Task SendAsync<T>(T data, VirgoMessageType type, CancellationToken ct)
    {
        return _connection!.SendAsync<T>(data, type, ct);
    }
}