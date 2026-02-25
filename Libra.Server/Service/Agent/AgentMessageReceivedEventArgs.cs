namespace Libra.Server.Service.Agent
{
    public class AgentMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Agent 唯一标识
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// Agent 会话（可用于主动回发数据）
        /// </summary>
        public AgentSession Session { get; set; }

        /// <summary>
        /// 【输出】处理后的响应数据（可选）
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// 【输出】是否已处理（避免默认回显）
        /// </summary>
        public bool Handled { get; set; }
    }
}