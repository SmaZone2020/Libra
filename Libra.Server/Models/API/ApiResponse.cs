using Libra.Server.Enum;

namespace Libra.Server.Models.API
{
    public class ApiResponse<T>
    {
        public LibraStatusCode Code { get; set; }   // 业务状态码
        public string ?Message { get; set; }    // 消息
        public string ?RequestId { get; set; }  // 请求追踪ID
        public T ?Data { get; set; }            // 业务数据 payload
        public long Timestamp { get; set; }    // 服务端时间戳 (毫秒)
    }
}
