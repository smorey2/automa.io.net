using Automa.IO.Proxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Args = System.Collections.Generic.Dictionary<string, object>;

namespace Automa.IO.Controllers
{
    [ApiController, Route("[controller]"), AllowAnonymous]
    public class AutomaController : ControllerBase
    {
        readonly ILogger<AutomaController> _logger;
        readonly WebHost _proxyHost;

        public AutomaController(ILogger<AutomaController> logger, WebHost proxyHost)
        {
            _logger = logger;
            _proxyHost = proxyHost;
        }

        [HttpGet()]
        public string Default() => "OK";

        [HttpPost("[action]")]
        public async Task<WebHostResponse<LoginResponse>> Login([FromBody] Args args) => await new WebHostResponse<LoginResponse>().Handle(() => _proxyHost.LoginAsync(args));

        [HttpPost("[action]")]
        public async Task<WebHostResponse<SelectApplicationResponse>> SelectApplication([FromBody] Args args) => await new WebHostResponse<SelectApplicationResponse>().Handle(() => _proxyHost.SelectApplicationAsync(args));

        [HttpPost("[action]")]
        public async Task<WebHostResponse<SetDeviceAccessTokenResponse>> SetDeviceAccessToken([FromBody] Args args) => await new WebHostResponse<SetDeviceAccessTokenResponse>().Handle(() => _proxyHost.SetDeviceAccessTokenAsync(args));
    }
}
