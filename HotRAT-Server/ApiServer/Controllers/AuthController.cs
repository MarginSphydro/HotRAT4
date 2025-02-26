using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HotRAT_Server.Controllers
{
    [ApiController]
    [Route("api/")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpGet("auth")]
        public IActionResult AuthToken(string token)
        {
            if (token == Models.AuthModel.Token())
            {
                return Ok(new { code = 200, data = true });
            }
            else
            {
                return Ok(new { code = 203, data = false });
            }
        }

        [HttpGet("auth/build")]
        public IActionResult BuildToken(string key)
        {
            if(string.IsNullOrEmpty(key))
                return Ok(new { code = 203, data = "not found key." });

            return Ok(new { code = 200, data = Models.AuthModel.Build(key) });
        }

        [HttpGet("time")]
        public IActionResult GetTime()
        {
            return Ok(new { code = 200, data = DateTime.Now.ToString("yyyyMMddHH") });
        }
    }
}
