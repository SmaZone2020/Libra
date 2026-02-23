using System.Text.Json.Serialization;

namespace Libra.Server.Models.Agent
{
    public class C2Envelope
    {
        /// <summary>
        /// 消息唯一 ID (防重放)
        /// </summary>
        [JsonPropertyName("mid")]
        public string MessageId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Agent ID (标识来源)
        /// </summary>
        [JsonPropertyName("aid")]
        public string AgentId { get; set; }

        /// <summary>
        /// 时间戳 (Unix Milliseconds, 防重放窗口校验)
        /// </summary>
        [JsonPropertyName("ts")]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// 消息类型 (1=Heartbeat, 2=TaskResult, 3=FileChunk)
        /// </summary>
        [JsonPropertyName("type")]
        public int MessageType { get; set; }

        /// <summary>
        /// 加密后的负载 (Base64)
        /// 内容：AES-GCM(Ciphertext + Tag)
        /// </summary>
        [JsonPropertyName("payload")]
        public string EncryptedPayload { get; set; }

        /// <summary>
        /// AES-GCM Nonce (Base64, 12 bytes)
        /// </summary>
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

        /// <summary>
        /// HMAC 签名 (Base64, 完整性校验)
        /// 签名内容：mid + aid + ts + type + payload + nonce
        /// </summary>
        [JsonPropertyName("sig")]
        public string Signature { get; set; }
    }

    /// <summary>
    /// 解密后的内部业务数据结构
    /// </summary>
    public class C2Payload
    {
        [JsonPropertyName("cmd")]
        public string Command { get; set; } // "heartbeat", "shell", "upload"

        [JsonPropertyName("data")]
        public object Data { get; set; } 

        [JsonPropertyName("err")]
        public string Error { get; set; }
    }
}
