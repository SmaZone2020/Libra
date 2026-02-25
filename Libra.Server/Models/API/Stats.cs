namespace Libra.Server.Models.API
{
    public class Stats
    {
        public int OnlineCount { get; set; }  //活动数量
        public int IdleCount { get; set; }    //挂机数量
        

        public long StartTime { get; set; } //服务启动时间

        public int Ping { get; set; } //请求延迟 MS
    }
}
