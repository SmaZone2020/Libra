using Libra.Virgo.Enum;
using Libra.Virgo.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace Libra.Virgo;

public class VirgoServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<Guid, VirgoConnection> _clients = new();

    public event Func<VirgoConnection, AgentInfo, Task>? AgentRegistered;
    public event Func<VirgoConnection, string, VirgoMessageType, Task>? MessageReceived;

    public VirgoServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _listener.Start();

        while (!ct.IsCancellationRequested)
        {
            var tcp = await _listener.AcceptTcpClientAsync(ct);
            var conn = new VirgoConnection(tcp);

            _ = HandleClientAsync(conn, ct);
        }
    }

    private async Task HandleClientAsync(VirgoConnection conn, CancellationToken ct)
    {
        bool registered = false;

        conn.MessageReceived += async (c, dataJson, type) =>
        {
            if (!registered)
            {
                if (type != VirgoMessageType.Register)
                {
                    c.Disconnect();
                    return;
                }

                var agent = JsonSerializer.Deserialize<AgentInfo>(dataJson, VirgoJson.Options);

                if (agent == null)
                {
                    c.Disconnect();
                    return;
                }

                registered = true;
                _clients[c.Id] = c;

                if (AgentRegistered != null)
                    await AgentRegistered.Invoke(c, agent);
            }
            else
            {
                if (MessageReceived != null)
                    await MessageReceived.Invoke(c, dataJson, type);
            }
        };

        conn.Disconnected += c =>
        {
            _clients.TryRemove(c.Id, out _);
        };

        _ = conn.StartAsync(ct);

        await Task.Delay(10000);
        if (!registered)
            conn.Disconnect();
    }

    public async Task SendToClientAsync(VirgoConnection conn, object data, VirgoMessageType type, CancellationToken ct)
    {
        var dataJson = JsonSerializer.Serialize(data, VirgoJson.Options);
        await conn.SendAsync(dataJson, type, ct);
    }
}