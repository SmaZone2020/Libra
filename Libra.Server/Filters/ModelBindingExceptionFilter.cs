using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Models.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Libra.Server.Filters
{
    public class ModelBindingExceptionFilter : IActionFilter
    {
        private readonly ILogger<ModelBindingExceptionFilter> _logger;

        public ModelBindingExceptionFilter(ILogger<ModelBindingExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 检查模型绑定是否有错误
            if (!context.ModelState.IsValid)
            {
                // 记录模型绑定错误
                foreach (var entry in context.ModelState)
                {
                    var fieldName = entry.Key;
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning("Model binding error for field '{FieldName}': {ErrorMessage}", fieldName, error.ErrorMessage);
                    }
                }

                // 创建统一的错误响应
                var response = new ApiResponse<string>
                {
                    Code = LibraStatusCode.BadRequest,
                    Message = "请求参数格式错误",
                    Data = string.Empty,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };

                // 返回统一的错误响应
                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // 不需要实现
        }
    }
}