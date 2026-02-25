using System.IO;
using System.Text.Json;

namespace Libra.Server.Service.Auth
{
    public class TotpConfigManager
    {
        private const string ConfigFileName = "totp-config.json";
        private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        private static TotpConfig _config;
        private static readonly object _lock = new();

        public class TotpConfig
        {
            public string SecretKey { get; set; }
            public bool IsFirstPing { get; set; } = true;
        }

        public static string GetSecretKey()
        {
            lock (_lock)
            {
                if (_config == null)
                {
                    LoadConfig();
                }
                return _config.SecretKey;
            }
        }

        public static bool IsFirstPing()
        {
            lock (_lock)
            {
                if (_config == null)
                {
                    LoadConfig();
                }
                return _config.IsFirstPing;
            }
        }

        public static void MarkFirstPingComplete()
        {
            lock (_lock)
            {
                if (_config == null)
                {
                    LoadConfig();
                }
                _config.IsFirstPing = false;
                SaveConfig();
            }
        }

        private static void LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigPath);
                    _config = JsonSerializer.Deserialize<TotpConfig>(json);
                }
                catch
                {
                    CreateNewConfig();
                }
            }
            else
            {
                CreateNewConfig();
            }

            if (_config == null || string.IsNullOrEmpty(_config.SecretKey))
            {
                CreateNewConfig();
            }
        }

        private static void CreateNewConfig()
        {
            _config = new TotpConfig
            {
                SecretKey = TotpService.GenerateSecretKey(),
                IsFirstPing = true
            };
            SaveConfig();
        }

        private static void SaveConfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // 配置保存失败，使用内存中的配置
            }
        }

        public static void Initialize()
        {
            LoadConfig();
        }
    }
}