using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Filters;
using Libra.Server.Models;
using Libra.Server.Models.API;
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
                    Ping = (int)(DateTime.Now.ToUnixTimestampMs() - t) //MS
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
    }
}