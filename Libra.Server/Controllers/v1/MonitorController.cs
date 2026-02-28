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
using System.Text.Json;

namespace Libra.Server.Controllers.v1
{
    [Route("api/v1/monitor")]
    [ApiController]
    [TypeFilter(typeof(JwtAuthFilter))]
    public class MonitorController(ILogger<MonitorController> logger) : ControllerBase
    {
        private readonly ILogger<MonitorController> _logger = logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

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
        public async Task<ApiResponse<object>> GetCameraFrame(string agentId, [FromQuery] int cameraIndex = 0)
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
                    Parameter = [$"{cameraIndex}"]
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

                    await Task.Delay(500);
                }

                TaskList.CameraFrameTasks.Remove(tid);
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
                _logger.LogError(ex, "获取摄像头帧失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取摄像头帧失败",
                    Data = ex.Message,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }

        /// <summary>
        /// SSE 摄像头流。首个订阅者自动触发 Agent 开始推流，全部断开后自动停止。
        /// cameraIndex: 摄像头索引（默认 0）
        /// fps: 帧率（默认 10）
        /// 数据格式：text/event-stream，每帧一条 data: {...}\n\n
        /// isFull=true，data 字段为 base64 JPEG
        /// </summary>
        [HttpGet("camera/stream/{agentId}")]
        public async Task StreamCamera(
            string agentId,
            [FromQuery] int cameraIndex = 0,
            [FromQuery] int fps = 10,
            CancellationToken ct = default)
        {
            if (!Guid.TryParse(agentId, out var aid))
            {
                Response.StatusCode = 400;
                return;
            }

            fps = Math.Clamp(fps, 1, 30);

            Response.Headers["Content-Type"] = "text/event-stream; charset=utf-8";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";
            Response.Headers["Connection"] = "keep-alive";

            var (_, channel) = await CameraStreamManager.SubscribeAsync(aid, cameraIndex, fps);
            try
            {
                await foreach (var frame in channel.Reader.ReadAllAsync(ct))
                {
                    var json = JsonSerializer.Serialize(frame, _jsonOptions);
                    var line = $"data: {json}\n\n";
                    await Response.WriteAsync(line, ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException) { /* 客户端断开 */ }
            finally
            {
                await CameraStreamManager.UnsubscribeAsync(aid, cameraIndex, channel);
            }
        }

        /// <summary>
        /// SSE 差异屏幕流。首个订阅者自动触发 Agent 开始推流，全部断开后自动停止。
        /// quality 可选：native | 1080p | 720p（默认）| 540p | 370p
        /// 数据格式：text/event-stream，每帧一条 data: {...}\n\n
        /// isFull=true → data 字段为完整 base64 JPEG；isFull=false → blocks 字段为变化区块列表
        /// </summary>
        [HttpGet("stream/{agentId}")]
        public async Task StreamScreen(
            string agentId,
            [FromQuery] string quality = "720p",
            CancellationToken ct = default)
        {
            Guid aid;
            if (!Guid.TryParse(agentId, out aid))
            {
                Response.StatusCode = 400;
                return;
            }

            // 只允许合法的 quality 值
            if (!new[] { "native", "1080p", "720p", "540p", "370p" }.Contains(quality))
                quality = "720p";

            Response.Headers["Content-Type"] = "text/event-stream; charset=utf-8";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";
            Response.Headers["Connection"] = "keep-alive";

            var (_, channel) = await ScreenStreamManager.SubscribeAsync(aid, quality);
            try
            {
                await foreach (var frame in channel.Reader.ReadAllAsync(ct))
                {
                    var json = JsonSerializer.Serialize(frame, _jsonOptions);
                    var line = $"data: {json}\n\n";
                    await Response.WriteAsync(line, ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException) { /* 客户端断开 */ }
            finally
            {
                await ScreenStreamManager.UnsubscribeAsync(aid, quality, channel);
            }
        }
    }
}

