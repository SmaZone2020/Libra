namespace Libra.Server.Enum
{
    public enum LibraStatusCode
    {
        // 通用
        Success = 200,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,

        // 业务错误 (4xx)
        AgentOffline = 4001,      // 目标Agent不在线
        TaskInvalid = 4002,       // 任务参数校验失败
        PluginNotFound = 4003,    // 插件ID不存在
        TokenExpired = 40101,     // Access Token过期
        TokenInvalid = 40102,     // Token签名无效
        PermissionDenied = 40301, // 角色权限不足

        // 服务端错误 (5xx)
        InternalError = 50001,
        DatabaseError = 50002,
        EncryptionError = 50003,
    }
}
