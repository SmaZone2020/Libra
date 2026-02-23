using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Libra.Server.Filters
{
    public class JwtAuthFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 从请求头获取Authorization字段
            var authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
            
            if (string.IsNullOrEmpty(authHeader))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    Code = LibraStatusCode.Unauthorized,
                    Message = "无权限",
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                });
                return;
            }

            // 提取token
            var token = authHeader.StartsWith("Bearer ") 
                ? authHeader.Substring(7) 
                : authHeader;

            // 验证token
            if (!JwtValidate.Validate(token))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    Code = LibraStatusCode.Unauthorized,
                    Message = "无效的Token",
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                });
                return;
            }
        }
    }
}
