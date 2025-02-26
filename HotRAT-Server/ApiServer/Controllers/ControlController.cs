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

    }
}
