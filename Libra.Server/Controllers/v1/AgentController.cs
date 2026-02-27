using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Filters;
using Libra.Server.Models;
using Libra.Server.Models.API;
using Libra.Server.Service;
using Libra.Server.Service.Agent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Libra.Server.Controllers.v1
{
    [Route("api/v1/agents")]
    [ApiController]
    [TypeFilter(typeof(JwtAuthFilter))]
    public class AgentController(ILogger<AgentController> logger) : ControllerBase
    {
        private readonly ILogger<AgentController> _logger = logger;

        [HttpGet("online")]
        public async Task<ApiResponse<object>> GetOnlineAgents([FromQuery] int type = 0)
        {
            try
            {
                var onlineAgents = AgentList.GetOnlineAgents();
                var count = onlineAgents.Count;

                if(type == 1)
                {
                    return new()
                    {
                        Code = LibraStatusCode.Success,
                        Message = $"获取到 {count} 个在线 Agent",
                        Data = new
                        {
                            Count = count,
                            Agents = onlineAgents.Select(a => a.AgentId)
                        },
                        Timestamp = DateTime.Now.ToUnixTimestamp()
                    };
                }

                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = $"获取到 {count} 个在线 Agent",
                    Data = new
                    {
                        Count = count,
                        Agents = onlineAgents
                    },
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取在线列表失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取在线列表失败",
                    Data = null,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }

        [HttpGet("stats")]
        public async Task<ApiResponse<Stats>> GetAgentStats([FromQuery] long t)
        {
            try
            {
                var stats = new Stats()
                {
                    OnlineCount = AgentList.OnlineCount,
                    IdleCount = AgentList.AgentInfos.Count(x => x.IsIdle),
                    StartTime = Program.StartTime.ToUnixTimestamp(),
                    Ping = (int)(DateTime.Now.ToUnixTimestampMs() - t), //MS
                    StreamHour = DataStreamLog.LastHour,
                    StreamHourOut = DataStreamLogOutput.LastHour
                };

                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = "获取统计信息成功",
                    Data = stats,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取统计信息失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取统计信息失败",
                    Data = null,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }

        [HttpPost("build")]
        public async Task<ApiResponse<object>> GetBuildInfo([FromBody] BuildBody body )
        {
            try
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libra.Agent.dll");
                var buildPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Build");

                if (!System.IO.File.Exists(filePath))
                {
                    return new()
                    {
                        Code = LibraStatusCode.InternalError,
                        Message = "构建失败",
                    };
                }

                if(!Directory.Exists(buildPath)) Directory.CreateDirectory(buildPath);
                var guid = Guid.NewGuid().ToString();
                var outPath = Path.Combine(buildPath, guid, $"{guid}.exe");

                Directory.CreateDirectory(Path.Combine(buildPath, guid));
                System.IO.File.Copy(filePath, outPath);

                if(BinaryPatcher.ReplaceString(outPath, "{IP.IP.IP.IP}", body.Host) &&
                   BinaryPatcher.ReplaceInt32(outPath, 20230602, body.Port) &&
                   BinaryPatcher.ReplaceString(outPath, "{AuthToken}", body.Token))
                {
                    return new()
                    {
                        Code = LibraStatusCode.Success,
                        Message = "构建成功",
                        Data = new
                        {
                            FileName = $"{guid}.exe",
                            Content = Convert.ToBase64String(System.IO.File.ReadAllBytes(outPath))
                        },
                        Timestamp = DateTime.Now.ToUnixTimestamp()
                    };
                }


                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = "构建失败",
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "构建失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "构建失败",
                };
            }
        }
    }
}