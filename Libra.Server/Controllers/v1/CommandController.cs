using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Filters;
using Libra.Server.Models.API;
using Libra.Server.Service;
using Libra.Server.Service.Agent;
using Libra.Virgo.Enum;
using Libra.Virgo.Models.MessageType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Libra.Server.Controllers.v1
{
    [Route("api/v1/command")]
    [ApiController]
    [TypeFilter(typeof(JwtAuthFilter))]
    public class CommandController(ILogger<AgentController> logger) : ControllerBase
    {
        private readonly ILogger<AgentController> _logger = logger;

        [HttpPost("shell/{agentId}")]
        public async Task<ApiResponse<object>> RunShell(string agentId, [FromBody] string command)
        {
            try
            {
                var aid = Guid.Parse(agentId);
                if (AgentList.AgentInfos.Where(x => x.AgentId == aid) == null)
                {
                    return new()
                    {
                        Code = LibraStatusCode.AgentOffline,
                        Message = "Notfound Agent",
                        Timestamp = DateTime.Now.ToUnixTimestamp()
                    };
                }

                var tid = Guid.NewGuid();
                var task = new CommandTask();

                TaskList.CommandTasks.Add(tid, new()
                {
                    AgentId = aid,
                });

                var result = await Runtimes.SendMessageToAgent(aid, VirgoMessageType.Command, new CommandModel()
                {
                    TaskId = tid,
                    Type = CommandType.Shell,
                    Parameter = [command]
                });

                for (int i = 0; i < 16; i++)
                {
                    task = TaskList.CommandTasks.GetValueOrDefault(tid) ?? throw new Exception("任务不存在");
                    if (i >= 15)
                    {
                        return new()
                        {
                            Code = LibraStatusCode.InternalError,
                            Message = "执行shell超时",
                            Timestamp = DateTime.Now.ToUnixTimestamp()
                        };
                    }
                    if (task.IsCompleted) break;

                    task.Result = Encoding.UTF8.GetString(Convert.FromBase64String(task.Result.ToString()));


                    await Task.Delay(999);
                }

                TaskList.CommandTasks.Remove(tid);
                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = $"执行成功",
                    Data = task,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行shell失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "执行shell失败",
                    Data = ex.Message,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }
    }
}
