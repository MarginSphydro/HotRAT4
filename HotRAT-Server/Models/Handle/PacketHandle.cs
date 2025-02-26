using System.Text;
using System.Text.Unicode;

namespace HotRAT_Server.Models.Handle
{
    public class PacketHandle
    {
        public static void Handle(Packet packet, ClientInfo cinfo)
        {
            var Token = packet.Token;
            var Flag = packet.Flag;
            var Content = packet.Content;

            var conLines = Content.Split("\n");
            switch (Flag)
            {
                case 1:
                    // 注册
                    var clientInfo = ClientInfo.FromString(Content);
                    clientInfo.ClientID = cinfo.ClientID;
                    clientInfo.IP = cinfo.IP;
                    clientInfo.Port = cinfo.Port;
                    clientInfo.ConnectTime = cinfo.ConnectTime;
                    clientInfo._client = cinfo._client;

                    var existingClient = Program.server._clients.FirstOrDefault(c => c.Value.ClientID == cinfo.ClientID);
                    if (existingClient.Value != null)
                    {
                        Program.server._clients[existingClient.Key] = clientInfo;
                    }
                    break;
                case 2:
                    //完成待办 传入结果
                    if(conLines.Length >= 3)
                    {
                        var sessionID = conLines[0];
                        var waitCode = conLines[1];
                        var result = string.Join("\n", conLines[2]);
                        Program.server.UpdateMessageState(sessionID, result, waitCode);
                    }
                    break;
                default:
                    Console.WriteLine("Unknown packet identifier.");
                    break;
            }
        }
    }
}
