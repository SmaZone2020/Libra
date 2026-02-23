using Libra.Server.Enum;
using Libra.Server.Extensions;
using Libra.Server.Service;

namespace Libra.Server.Filters
{
    public class JwtValidate
    {
        public static bool Validate(string token)
        {
            bool isValid = JwtService.ValidateToken(token);
            var expiration = JwtService.GetTokenExpiration(token);

            if (isValid && expiration.HasValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
