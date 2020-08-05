using System;
using System.Net;
using System.Net.Http;
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
        static readonly IHttp _http = Default.Http();
        readonly IProxyOptions _options;

        public ProxyAutomation(AutomaClient client, IAutoma automa, IProxyOptions options) : base(client, automa) => _options = options ?? throw new ArgumentNullException(nameof(options));

        public override async Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
        {
            var i = await _http.Execute<ProxyResponse>(new HttpRequestMessage(HttpMethod.Get, $"{_options.ProxyUri}/Automa/Login"), cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        }

        public override async Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null)
        {
            var i = await _http.Execute<ProxyResponse>(new HttpRequestMessage(HttpMethod.Get, $"{_options.ProxyUri}/Automa/SelectApplication"), cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
            return null;
        }

        public override async Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null)
        {
            var i = await _http.Execute<ProxyResponse>(new HttpRequestMessage(HttpMethod.Get, $"{_options.ProxyUri}/Automa/SetDeviceAccessToken"), cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        }
    }
}
