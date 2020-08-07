using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO.Proxy
{
    /// <summary>
    /// ProxyAutomation
    /// </summary>
    /// <seealso cref="Automa.IO.Automation" />
    public class ProxyAutomation : Automation
    {
        readonly ProxyDriver _proxyDriver;

        public ProxyAutomation(AutomaClient client, IAutoma automa) : base(client, automa) => _proxyDriver = (ProxyDriver)automa.Driver;

        public override Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
            => _proxyDriver.LoginAsync(cookies, credential, tag, cancellationToken);

        public override Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null)
            => _proxyDriver.SelectApplicationAsync(application, tag, cancellationToken);

        public override Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null)
            => _proxyDriver.SetDeviceAccessTokenAsync(url, userCode, tag, cancellationToken);
    }
}
