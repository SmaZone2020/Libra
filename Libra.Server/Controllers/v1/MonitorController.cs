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
    [Route("api/v1/monitor")]
    [ApiController]
    [TypeFilter(typeof(JwtAuthFilter))]
    public class MonitorController(ILogger<MonitorController> logger) : ControllerBase
    {
        private readonly ILogger<MonitorController> _logger = logger;

        [HttpGet("frame/{agentId}")]
        public async Task<ApiResponse<object>> GetFrame(string agentId)
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

                TaskList.FrameTasks.Add(tid, new()
                {
                    AgentId = aid,
                });

                var result = await Runtimes.SendMessageToAgent(aid, VirgoMessageType.Command, new CommandModel()
                {
                    TaskId = tid,
                    Type = CommandType.GetFrame,
                });

                for (int i = 0; i < 3; i++)
                {
                    task = TaskList.FrameTasks.GetValueOrDefault(tid) ?? throw new Exception("任务不存在");
                    if (i >= 30)
                    {
                        return new()
                        {
                            Code = LibraStatusCode.InternalError,
                            Message = "获取帧超时",
                            Timestamp = DateTime.Now.ToUnixTimestamp()
                        };
                    }
                    if (task.IsCompleted) break;

                    task.Result = Encoding.UTF8.GetString(Convert.FromBase64String(task.Result.ToString()));


                    Console.WriteLine($"轮询结果第{i}次");

                    await Task.Delay(500);
                }

                TaskList.FrameTasks.Remove(tid);
                Console.WriteLine(task.Result.ToString().Length);
                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = $"获取帧成功",
                    Data = task.Result,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取帧失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取帧失败",
                    Data = ex.Message,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }


        [HttpGet("camera/{agentId}")]
        public async Task<ApiResponse<object>> GetCameraFrame(string agentId)
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

                TaskList.CameraFrameTasks.Add(tid, new()
                {
                    AgentId = aid,
                });

                var result = await Runtimes.SendMessageToAgent(aid, VirgoMessageType.Command, new CommandModel()
                {
                    TaskId = tid,
                    Type = CommandType.GetCameraFrame,
                });

                for (int i = 0; i < 3; i++)
                {
                    task = TaskList.CameraFrameTasks.GetValueOrDefault(tid) ?? throw new Exception("任务不存在");
                    if (i >= 30)
                    {
                        return new()
                        {
                            Code = LibraStatusCode.InternalError,
                            Message = "获取帧超时",
                            Timestamp = DateTime.Now.ToUnixTimestamp()
                        };
                    }
                    if (task.IsCompleted) break;

                    task.Result = Encoding.UTF8.GetString(Convert.FromBase64String(task.Result.ToString()));


                    Console.WriteLine($"轮询结果第{i}次");

                    await Task.Delay(500);
                }

                TaskList.CameraFrameTasks.Remove(tid);
                Console.WriteLine(task.Result.ToString().Length);
                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = $"获取帧成功",
                    Data = task.Result,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取帧失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取帧失败",
                    Data = ex.Message,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }
    }
}
