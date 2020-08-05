using Automa.IO.Proxy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Automa.IO.Controllers
{
    [ApiController, Route("[controller]")]
    public class AutomaController : ControllerBase
    {
        readonly ILogger<AutomaController> _logger;
        readonly ProxyHost _proxyHost;

        public AutomaController(ILogger<AutomaController> logger, ProxyHost proxyHost)
        {
            _logger = logger;
            _proxyHost = proxyHost;
        }

        [HttpGet]
        public byte[] Login(byte[] data) => _proxyHost.Login(data);

        [HttpGet]
        public byte[] SelectApplication(byte[] data) => _proxyHost.SelectApplication(data);

        [HttpGet]
        public byte[] SetDeviceAccessToken(byte[] data) => _proxyHost.SetDeviceAccessToken(data);
    }
}
