// Libra Agent 启动程序
using Libra.Agent;
using Libra.Virgo.Enum;
using System.Threading;

Console.WriteLine("Libra Agent 启动中...");
Console.WriteLine("=====================");

try
{
    string serverIp = "127.0.0.1";
    int serverPort = 8888;

    Console.WriteLine($"Agent ID: {Runtimes.AgentId}");
    Console.WriteLine($"服务器 IP: {serverIp}:{serverPort}");

    // 创建取消令牌
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (sender, e) =>
    {
        Console.WriteLine("正在关闭...");
        cts.Cancel();
        e.Cancel = true;
    };

    // 初始化连接
    Console.WriteLine("初始化Virgo连接...");
    try
    {
        await Runtimes.Initialize(serverIp, serverPort);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"连接失败: {ex.Message}");
        Console.WriteLine("将在主循环中尝试重新连接...");
    }

    // 启动主循环
    Console.WriteLine("正在启动主循环...");
    var random = new Random();
    while (!cts.Token.IsCancellationRequested)
    {
        try
        {
            // 发送心跳
            await Runtimes.SendMessage(VirgoMessageType.Heartbeat, new { Status = "alive", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() });
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]已发送心跳");

            await Task.Delay(30000, cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"主循环出错: {ex.Message}");
            // 异常退避策略
            await Task.Delay(60000, cts.Token);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
}
finally
{
    Console.WriteLine("Agent 已停止。");
    Console.WriteLine("按任意键退出...");
}
