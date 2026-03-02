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
                    var session = new AgentSession(connection, agentInfo.AgentId);
                    AgentList.UpsertAgent(agentInfo, session);
                };

                VirgoServer.MessageReceived += async (connection, dataJson, type) =>
                {
                    DataStreamLog.Add(dataJson.Length);
                    MainHandle.Handle(type, JsonConvert.DeserializeObject<object>(dataJson));

                };

                var cts = new CancellationTokenSource();
                _ = VirgoServer.StartAsync(cts.Token);

                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    cts.Cancel();
                };
            }
        }

        /// <summary>
        /// 向指定 Agent 发送消息
        /// </summary>
        public static async Task<bool> SendMessageToAgent(Guid agentId, VirgoMessageType type, object data)
        {
            DataStreamLogOutput.Add(JsonConvert.SerializeObject(data).Length);
            return await AgentList.SendMessageToAgentAsync(agentId, type, data);
        }
    }
}