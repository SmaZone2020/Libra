using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Libra.Server.Models
{
    public class BasicMessage
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("flag")]
        public int Flag { get; set; }

        [JsonPropertyName("par")]
        public object[] Params { get; set; } = [];

        [JsonPropertyName("data")]
        public object Data { get; set; }

        [JsonPropertyName("sign")]
        public string Sign { get; set; } = "";

        private static readonly JsonSerializerOptions _defaultOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, _defaultOptions);
        }

        public static BasicMessage FromJson(string json)
        {
            return JsonSerializer.Deserialize<BasicMessage>(json, _defaultOptions);
        }
    }
}