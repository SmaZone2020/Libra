// Libra Agent 启动程序
using Libra.Agent.Service;
using System.Threading;

Console.WriteLine("Libra Agent 启动中...");
Console.WriteLine("=====================");

try
{
    // 生成唯一的 agent ID
    string agentId = Guid.NewGuid().ToString();
    string serverIp = "127.0.0.1";
    int serverPort = 8888;

    Console.WriteLine($"Agent ID: {agentId}");
    Console.WriteLine($"服务器 IP: {serverIp}:{serverPort}");
    Console.WriteLine("使用标准 TCP 连接");

    // 创建通信服务
    var communicationService = new CommunicationService(agentId, serverIp, serverPort);

    // 注册事件处理器
    communicationService.Connected += (sender, e) => Console.WriteLine("已连接到服务器");
    communicationService.Disconnected += (sender, e) => Console.WriteLine("已从服务器断开连接");
    communicationService.Registered += (sender, e) => Console.WriteLine("注册成功");
    communicationService.RegistrationFailed += (sender, e) => Console.WriteLine("注册失败");

    // 创建取消令牌
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (sender, e) =>
    {
        Console.WriteLine("正在关闭...");
        cts.Cancel();
        e.Cancel = true;
    };

    // 初始化连接
    Console.WriteLine("正在初始化连接...");
    try
    {
        await communicationService.InitializeAsync(cts.Token);
        Console.WriteLine("初始化成功");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"初始化失败: {ex.Message}");
        Console.WriteLine("将在主循环中尝试重新连接...");
    }

    // 启动主循环
    Console.WriteLine("正在启动主循环...");
    await communicationService.StartLoopAsync(cts.Token);
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
