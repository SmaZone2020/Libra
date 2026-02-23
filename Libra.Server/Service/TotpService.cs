using System;
using System.Security.Cryptography;
using OtpNet;
namespace Libra.Server.Service
{

    public static class TotpService
    {
        private const int CodeDigits = 6;
        private const int TimeStepSeconds = 30;
        private const int AllowedTimeDrift = 1;

        /// <summary>
        /// 生成新的 2FA 密钥（Base32 编码）
        /// </summary>
        public static string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key).TrimEnd('=');
        }

        /// <summary>
        /// 生成 otpauth:// 二维码 URL
        /// </summary>
        public static string GenerateQrCodeUrl(string secretKey, string accountName, string issuer)
        {
            var encodedIssuer = Uri.EscapeDataString(issuer);
            var encodedAccount = Uri.EscapeDataString(accountName);

            return $"otpauth://totp/{encodedIssuer}:{encodedAccount}" +
                   $"?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits={CodeDigits}&period={TimeStepSeconds}";
        }

        /// <summary>
        /// 验证用户输入的 6 位验证码
        /// </summary>
        public static bool VerifyCode(string secretKey, string code)
        {
            if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code) || code.Length != 6)
                return false;

            try
            {
                var keyBytes = Base32Encoding.ToBytes(secretKey);
                var totp = new Totp(keyBytes, TimeStepSeconds, OtpHashMode.Sha1, CodeDigits);

                var verificationWindow = new VerificationWindow(
                    previous: AllowedTimeDrift,
                    future: AllowedTimeDrift
                );

                return totp.VerifyTotp(
                    code,
                    out long timeStepMatched,
                    verificationWindow
                );
            }
            catch
            {
                return false; // 安全失败
            }
        }

        /// <summary>
        /// 【调试用】获取当前应生成的验证码 ✅ 已修复
        /// </summary>
        public static string GetCurrentCodeForTesting(string secretKey)
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes, TimeStepSeconds, OtpHashMode.Sha1, CodeDigits);

            // ✅ ComputeTotp 不接受参数，使用系统当前时间
            return totp.ComputeTotp();
        }

        /// <summary>
        /// 【高级】带自定义时间的验证码生成（仅测试用）
        /// </summary>
        public static string GetCurrentCodeAtTime(string secretKey, DateTimeOffset customTime)
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes, TimeStepSeconds, OtpHashMode.Sha1, CodeDigits);

            return totp.ComputeTotp(customTime.DateTime);
        }
    }
}
