namespace Libra.Server.Models.Agent
{
    public class City
    {
        /*
         {
          "code": 1,
          "ip": "222.90.9.73",
          "city": "Xi’an",
          "country": "CN",
          "isp": "null",
          "msg": "LEEE.TECH"
        }
        */
        public int code { get; set; }
        public string ip { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string isp { get; set; }
        public string msg { get; set; }
    }
}
