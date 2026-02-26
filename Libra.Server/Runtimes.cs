using Libra.Server.Handle;
using Libra.Server.Service;
using Libra.Server.Service.Agent;
using Libra.Virgo;
using Libra.Virgo.Enum;
using Libra.Virgo.Models;
using Newtonsoft.Json;

namespace Libra.Server
{
    public static class Runtimes
    {
        public static VirgoServer? VirgoServer { get; private set; }
        public static DateTime StartTime { get; } = DateTime.Now;


        public static void Initialize(int port = 8888)
        {
            if (VirgoServer == null)
            {
                VirgoServer = new VirgoServer(port);
                
                VirgoServer.AgentRegistered += async (connection, agentInfo) =>
                {
                    Console.WriteLine($"Agent 注册成功: {agentInfo.AgentId}, {agentInfo.Location}");

                    var session = new AgentSession(connection, agentInfo.AgentId);
                    AgentList.UpsertAgent(agentInfo, session);
                };

                VirgoServer.MessageReceived += async (connection, dataJson, type) =>
                {
                    Console.WriteLine($"收到消息: {type}");
                    //Console.WriteLine($"数据: {dataJson}");
                    DataStreamLog.Add(dataJson.Length);
                    MainHandle.Handle(type, JsonConvert.DeserializeObject<object>(dataJson));

                };

                // 启动 Virgo 服务端
                var cts = new CancellationTokenSource();
                _ = VirgoServer.StartAsync(cts.Token);
                Console.WriteLine($"Virgo Server started on port {port}");

                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    cts.Cancel();
                    Console.WriteLine("Virgo Server stopped");
                };
            }
        }

        /// <summary>
        /// 向特定 Agent 发送消息
        /// </summary>
        /// <param name="agentId">Agent ID</param>
        /// <param name="message">消息内容</param>
        /// <returns>是否发送成功</returns>
        public static async Task<bool> SendMessageToAgent(Guid agentId, VirgoMessageType type, object data)
        {
            DataStreamLogOutput.Add(JsonConvert.SerializeObject(data).Length);
            return await AgentList.SendMessageToAgentAsync(agentId, type, data);
        }
    }
}