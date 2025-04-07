using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace HotRAT.Server.Configs
{
    public class ServerConfig
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
    }

    public class ConfigModels
    {
        public static ServerConfig ?serverConfig = JsonConvert.DeserializeObject<ServerConfig>(Program.ReadFile("Configs.serverSetting.json"));
        public static bool Reload()
        {
            try
            {
                serverConfig = JsonConvert.DeserializeObject<ServerConfig>(Program.ReadFile("Configs.serverSetting.json"));
                LoggerModel.AddToLog("重载服务器配置文件", InfoLevel.Normal);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
