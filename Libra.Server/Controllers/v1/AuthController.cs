using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Models.API;
using Libra.Server.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Libra.Server.Controllers.v1
{
    [Route("api/v1")]
    [ApiController]
    public class AuthController(ILogger<AuthController> logger) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;

        [HttpGet("ping")]
        public async Task<ApiResponse<string>> Ping([FromQuery] string type = "1")
        {
            string ?data = null;
            string message;

            if (TotpConfigManager.IsFirstPing())
            {
                if(type == "1")
                    data = TotpService.GenerateQrCodeUrl(TotpConfigManager.GetSecretKey(), "Admin", "Libra");
                else
                    data = TotpConfigManager.GetSecretKey();
                message = "请将此密钥添加到您的令牌程序中，令牌只展示一次！"; 
                TotpConfigManager.MarkFirstPingComplete();
            }
            else
            {
                message = "Login Success";
            }

            return new()
            {
                Code = LibraStatusCode.Success,
                Message = message,
                Data = data,
                Timestamp = DateTime.Now.ToUnixTimestamp()
            };
        }

        [HttpPost("login")]
        public async Task<ApiResponse<string>> Login([FromBody] string code)
        {
            var secretKey = TotpConfigManager.GetSecretKey();
            if (!TotpService.VerifyCode(secretKey, code))
            {
                return new()
                {
                    Code = LibraStatusCode.BadRequest,
                    Message = "验证码错误",
                    Data = string.Empty,
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }

            var token = JwtService.GenerateToken();

            return new()
            {
                Code = LibraStatusCode.Success,
                Message = "Login Success",
                Data = token,
                Timestamp = DateTime.Now.ToUnixTimestamp()
            };
        }

        [HttpPost("status")]
        public async Task<ApiResponse<object>> ValidateToken([FromBody] string token)
        {
            bool isValid = JwtService.ValidateToken(token);
            var expiration = JwtService.GetTokenExpiration(token);

            if (isValid && expiration.HasValue)
            {
                return new()
                {
                    Code = LibraStatusCode.Success,
                    Message = "令牌有效",
                    Data = new
                    {
                        Valid = true,
                        Expiration = expiration.ToUnixTimestampSafe()
                    },
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
            else
            {
                return new()
                {
                    Code = LibraStatusCode.Unauthorized,
                    Message = "令牌无效或已过期",
                    Data = new
                    {
                        Valid = false,
                        Expiration = expiration.ToUnixTimestampSafe()
                    },
                    Timestamp = DateTime.Now.ToUnixTimestamp()
                };
            }
        }

    }
}
