using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Models.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Libra.Server.Filters
{
    public class GlobalExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            // 记录详细的异常信息到日志
            _logger.LogError(context.Exception, "Unhandled exception occurred");

            // 创建统一的错误响应
            var response = new ApiResponse<string>
            {
                Code = LibraStatusCode.InternalError,
                Message = "服务器内部错误，请稍后重试",
                Data = string.Empty,
                Timestamp = DateTime.Now.ToUnixTimestamp()
            };

            // 设置响应状态码为 500
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // 返回统一的错误响应
            context.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            // 标记异常已处理
            context.ExceptionHandled = true;

            await Task.CompletedTask;
        }
    }
}