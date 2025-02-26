using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo
{
    public class HPClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        public async Task ConnectAsync(string serverIp, int serverPort)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(serverIp, serverPort);
            _stream = _client.GetStream();

            Console.WriteLine("连接到服务器，等待指令");

            _ = Task.Run(ReceiveData);
        }

        public async Task SendDataAsync(uint flag, string content, string token)
        {
            if (_stream == null) throw new InvalidOperationException("操你妈，连不上了");

            var packet = $"{token}\n{flag}\n{content}";

            var packetBytes = Encoding.UTF8.GetBytes(packet);

            await _stream.WriteAsync(packetBytes, 0, packetBytes.Length);
            Console.WriteLine($"已发送数据:\nToken={token}\nFlag={flag}\nContent={content}");
        }


        private async Task ReceiveData()
        {
            byte[] buffer = new byte[32768];
            while (true)
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    var lines = Encoding.UTF8.GetString(buffer, 0, bytesRead).Split("\n");
                    if (lines.Length >= 2)
                    {
                        var waitCode = lines[0];
                        string data = string.Join("\n", lines.Skip(1));
                        ClientHandle.Handle(waitCode, data);
                    }
                }
            }
        }

        public void Close()
        {
            _stream?.Close();
            _client?.Close();
        }
    }
}
