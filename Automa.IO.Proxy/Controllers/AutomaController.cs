using Automa.IO.Proxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Automa.IO.Controllers
{
    [ApiController, Route("[controller]"), AllowAnonymous]
    public class AutomaController : ControllerBase
    {
        readonly ILogger<AutomaController> _logger;
        readonly ProxyHost _proxyHost;

        public AutomaController(ILogger<AutomaController> logger, ProxyHost proxyHost)
        {
            _logger = logger;
            _proxyHost = proxyHost;
        }

        [HttpGet()]
        public string Default() => "OK";

        [HttpGet("[action]")]
        public async Task Open() => await _proxyHost.OpenAsync(HttpContext, _logger, x => x == Config.ProxyToken);
    }
}
