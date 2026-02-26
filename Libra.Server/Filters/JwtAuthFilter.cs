using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Filters;
using Libra.Server.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Libra.Server.Filters
{
    public class JwtAuthFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
            DataStreamLogOutput.Add(context.HttpContext.Request.ContentLength ?? 0);
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
