// Libra Agent 启动程序
using Libra.Agent;
using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using System.Threading;

//D Console.WriteLine("Libra Agent 启动中...");
//D Console.WriteLine("=====================");

try
{
    string serverIp = "{IP.IP.IP.IP}";//116.62.22.115
    int serverPort = 20230602;
    string token = "{AuthToken}";

    //D Console.WriteLine($"Agent ID: {Runtimes.AgentId}");
    //D Console.WriteLine($"服务器 IP: {serverIp}:{serverPort}");

    // 创建取消令牌
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (sender, e) =>
    {
        //D Console.WriteLine("正在关闭...");
        cts.Cancel();
        e.Cancel = true;
    };

    // 初始化连接
    //D Console.WriteLine("初始化Virgo连接...");
    try
    {
        await Runtimes.Initialize(serverIp, serverPort);

        var random = new Random();
        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                // 发送心跳
                await Runtimes.SendMessage(VirgoMessageType.Heartbeat, new HeartbeatMessage { Status = "alive", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() });
                //D Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]已发送心跳");

                await Task.Delay(30000, cts.Token);
            }
            catch (Exception ex)
            {
                //D Console.WriteLine($"主循环出错: {ex.Message}");
                await Task.Delay(60000, cts.Token);
            }
        }
    }
    catch (Exception ex)
    {
        //D Console.WriteLine($"连接失败: {ex.Message}");
    }
}
catch (Exception ex)
{
    //D Console.WriteLine($"错误: {ex.Message}");
    //D Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
}
finally
{
    //D Console.WriteLine("Agent 已停止。");
    //D Console.WriteLine("按任意键退出...");
}
