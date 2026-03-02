using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Libra.Agent.Service
{
    public static class CryptoService
    {
        private static readonly byte[] SessionKey = Encoding.UTF8.GetBytes("32ByteSecretKeyForAes256!!");
        private static readonly byte[] HmacKey = Encoding.UTF8.GetBytes("32ByteSecretKeyForHmacSha!!");

        /// <summary>
        /// 加密并签名
        /// </summary>
        public static (string cipher, string nonce, string sig) EncryptAndSign(object payloadObj)
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            string plainJson = JsonSerializer.Serialize(payloadObj, options);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainJson);

            // AES-GCM 加密
            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] cipherText = new byte[plainBytes.Length];
            byte[] tag = new byte[16];

            using (var aes = new AesGcm(SessionKey))
            {
                aes.Encrypt(nonce, plainBytes, cipherText, tag);
            }

            byte[] cipherWithTag = new byte[cipherText.Length + tag.Length];
            Buffer.BlockCopy(cipherText, 0, cipherWithTag, 0, cipherText.Length);
            Buffer.BlockCopy(tag, 0, cipherWithTag, cipherText.Length, tag.Length);

            // HMAC 签名
            // 注意：实际签名应包含 Envelope 中的明文部分，此处简化为对密文签名
            string sigInput = Convert.ToBase64String(cipherWithTag);
            byte[] sigBytes;
            using (var hmac = new HMACSHA256(HmacKey))
            {
                sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(sigInput));
            }

            return (Convert.ToBase64String(cipherWithTag), Convert.ToBase64String(nonce), Convert.ToBase64String(sigBytes));
        }

        /// <summary>
        /// 验证签名并解密
        /// </summary>
        public static T DecryptAndVerify<T>(string cipherB64, string nonceB64, string sigB64)
        {
            byte[] cipherWithTag = Convert.FromBase64String(cipherB64);
            byte[] expectedSig;
            using (var hmac = new HMACSHA256(HmacKey))
            {
                expectedSig = hmac.ComputeHash(Encoding.UTF8.GetBytes(cipherB64));
            }

            byte[] providedSig = Convert.FromBase64String(sigB64);
            if (!CryptographicOperations.FixedTimeEquals(expectedSig, providedSig))
                throw new SecurityException("Signature verification failed");

            byte[] tag = new byte[16];
            byte[] cipherText = new byte[cipherWithTag.Length - 16];
            Buffer.BlockCopy(cipherWithTag, 0, cipherText, 0, cipherText.Length);
            Buffer.BlockCopy(cipherWithTag, cipherText.Length, tag, 0, 16);

            byte[] nonce = Convert.FromBase64String(nonceB64);
            byte[] plainBytes = new byte[cipherText.Length];

            using (var aes = new AesGcm(SessionKey))
            {
                aes.Decrypt(nonce, cipherText, tag, plainBytes);
            }

            string plainJson = Encoding.UTF8.GetString(plainBytes);
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            return JsonSerializer.Deserialize<T>(plainJson, options);
        }
    }
}
