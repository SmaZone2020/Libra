using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Filters;
using Libra.Server.Models.API;
using Libra.Server.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Libra.Server.Controllers.v1
{
    [Route("api/v1")]
    [ApiController]
    [TypeFilter(typeof(JwtAuthFilter))]
    public class AgentController(ILogger<AgentController> logger) : ControllerBase
    {
        private readonly ILogger<AgentController> _logger = logger;

        [HttpGet("agents/online")]
        public async Task<ApiResponse<object>> GetOnlineAgents()
        {
            try
            {
                var onlineAgents = AgentList.GetOnlineAgents();
                var count = onlineAgents.Count;

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
                _logger.LogError(ex, "获取在线 Agent 列表失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取在线 Agent 列表失败",
                    Data = null,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }

        [HttpGet("agents/stats")]
        public async Task<ApiResponse<object>> GetAgentStats()
        {
            try
            {
                var stats = new
                {
                    OnlineCount = AgentList.OnlineCount,
                    TotalCount = AgentList.TotalCount,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };

                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = "获取 Agent 统计信息成功",
                    Data = stats,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 Agent 统计信息失败");
                return new()
                {
                    Code = LibraStatusCode.InternalError,
                    Message = "获取 Agent 统计信息失败",
                    Data = null,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }
    }
}