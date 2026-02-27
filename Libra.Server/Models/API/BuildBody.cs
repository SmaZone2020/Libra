namespace Libra.Server.Models.API
{
    public class BuildBody
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 0;

        public string Token { get; set; } = string.Empty;
    }
}
