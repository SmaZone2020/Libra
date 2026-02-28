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
    [Route("api/v1/explorer")]
    [ApiController]
    [TypeFilter(typeof(JwtAuthFilter))]
    public class ExplorerController(ILogger<ExplorerController> logger) : ControllerBase
    {
        private readonly ILogger<ExplorerController> _logger = logger;

        [HttpPost("getfiles/{agentId}")]
        public async Task<ApiResponse<object>> GetFiles(string agentId, [FromBody] string path)
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

                TaskList.ExplorerTasks.Add(tid, new()
                {
                    AgentId = aid,
                });

                var result = await Runtimes.SendMessageToAgent(aid, VirgoMessageType.Command, new CommandModel()
                {
                    TaskId = tid,
                    Type = CommandType.GetFiles,
                    Parameter = [path]
                });

                for (int i = 0; i < 3; i++)
                {
                    task = TaskList.ExplorerTasks.GetValueOrDefault(tid) ?? throw new Exception("任务不存在");
                    if (i >= 30)
                    {
                        return new()
                        {
                            Code = LibraStatusCode.InternalError,
                            Message = "获取内容超时",
                            Timestamp = DateTime.Now.ToUnixTimestamp()
                        };
                    }
                    if (task.IsCompleted) break;

                    task.Result = Encoding.UTF8.GetString(Convert.FromBase64String(task.Result.ToString()));


                    Console.WriteLine($"轮询结果第{i}次");

                    await Task.Delay(500);
                }

                TaskList.ExplorerTasks.Remove(tid);
                Console.WriteLine(task.Result.ToString().Length);
                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = $"获取内容成功",
                    Data = task.Result,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取内容失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取内容失败",
                    Data = ex.Message,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }

        [HttpPost("getfile/{agentId}")]
        public async Task<ApiResponse<object>> GetFile(string agentId, [FromBody] string filepath)
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

                TaskList.ExplorerTasks.Add(tid, new()
                {
                    AgentId = aid,
                });

                var result = await Runtimes.SendMessageToAgent(aid, VirgoMessageType.Command, new CommandModel()
                {
                    TaskId = tid,
                    Type = CommandType.ReadFile,
                    Parameter = [filepath]
                });

                for (int i = 0; i < 3; i++)
                {
                    task = TaskList.ExplorerTasks.GetValueOrDefault(tid) ?? throw new Exception("任务不存在");
                    if (i >= 30)
                    {
                        return new()
                        {
                            Code = LibraStatusCode.InternalError,
                            Message = "获取内容超时",
                            Timestamp = DateTime.Now.ToUnixTimestamp()
                        };
                    }
                    if (task.IsCompleted) break;

                    task.Result = Encoding.UTF8.GetString(Convert.FromBase64String(task.Result.ToString()));


                    Console.WriteLine($"轮询结果第{i}次");

                    await Task.Delay(500);
                }

                TaskList.ExplorerTasks.Remove(tid);
                Console.WriteLine(task.Result.ToString().Length);
                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = $"获取内容成功",
                    Data = new
                    {
                        FileName = Path.GetFileName(filepath),
                        Content = task.Result,
                    },
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取内容失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取内容失败",
                    Data = ex.Message,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }

        [HttpPost("disks/{agentId}")]
        public async Task<ApiResponse<object>> GetDisks(string agentId)
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

                TaskList.ExplorerTasks.Add(tid, new()
                {
                    AgentId = aid,
                });

                var result = await Runtimes.SendMessageToAgent(aid, VirgoMessageType.Command, new CommandModel()
                {
                    TaskId = tid,
                    Type = CommandType.GetDisks
                });

                for (int i = 0; i < 3; i++)
                {
                    task = TaskList.ExplorerTasks.GetValueOrDefault(tid) ?? throw new Exception("任务不存在");
                    if (i >= 30)
                    {
                        return new()
                        {
                            Code = LibraStatusCode.InternalError,
                            Message = "获取内容超时",
                            Timestamp = DateTime.Now.ToUnixTimestamp()
                        };
                    }
                    if (task.IsCompleted) break;

                    task.Result = Encoding.UTF8.GetString(Convert.FromBase64String(task.Result.ToString()));


                    Console.WriteLine($"轮询结果第{i}次");

                    await Task.Delay(500);
                }

                TaskList.ExplorerTasks.Remove(tid);
                Console.WriteLine(task.Result.ToString().Length);
                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = $"获取内容成功",
                    Data = task.Result,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取内容失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取内容失败",
                    Data = ex.Message,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }

        [HttpGet("download/{agentId}")]
        public async Task DownloadFile(string agentId, [FromQuery] string filepath)
        {
            var aid = Guid.Parse(agentId);
            var tid = Guid.NewGuid();
            var channel = FileDownloadManager.Register(tid);

            try
            {
                await Runtimes.SendMessageToAgent(aid, VirgoMessageType.Command, new CommandModel
                {
                    TaskId = tid,
                    Type = CommandType.ReadFileStream,
                    Parameter = [filepath]
                });

                var fileName = Path.GetFileName(filepath);
                Response.ContentType = "application/octet-stream";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{Uri.EscapeDataString(fileName)}\"";

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, HttpContext.RequestAborted);

                await foreach (var chunk in channel.Reader.ReadAllAsync(linked.Token))
                {
                    if (chunk == null || chunk.Length == 0) break;
                    await Response.Body.WriteAsync(chunk, linked.Token);
                    await Response.Body.FlushAsync(linked.Token);
                }
            }
            catch (OperationCanceledException)
            {
                FileDownloadManager.Cancel(tid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件下载失败");
                FileDownloadManager.Cancel(tid);
                if (!Response.HasStarted)
                {
                    Response.StatusCode = 500;
                }
            }
        }
    }
}
