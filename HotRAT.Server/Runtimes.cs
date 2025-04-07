using HotRAT.Server.Configs;
using HotRAT.Server.Models;
using System.Collections.Concurrent;

namespace HotRAT.Server
{
    public class Runtimes
    {
        public static ConcurrentDictionary<Guid, ClientConnection> _clients = new();
        public static string Token() => Libs.Auth.TokenModel.Build(ConfigModels.serverConfig.Key);
    }
}
