using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Models.API;
using System.Text.Json;

namespace Libra.Server.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // 处理所有未捕获的错误
                _logger.LogError(ex, "Unhandled exception");
                await HandleGenericErrorAsync(context, ex);
            }
        }

        private async Task HandleGenericErrorAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<string>
            {
                Code = LibraStatusCode.InternalError,
                Message = "服务器内部错误，请稍后重试",
                Data = string.Empty,
                Timestamp = DateTime.Now.ToUnixTimestamp()
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    // 扩展方法，用于注册错误处理中间件
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}