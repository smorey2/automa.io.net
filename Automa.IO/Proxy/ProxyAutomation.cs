using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Args = System.Collections.Generic.Dictionary<string, object>;

namespace Automa.IO.Proxy
{
    /// <summary>
    /// ProxyAutomation
    /// </summary>
    /// <seealso cref="Automa.IO.Automation" />
    public class ProxyAutomation : Automation
    {
        readonly WebApiClient _api;

        public ProxyAutomation(AutomaClient client, IAutoma automa, IProxyOptions options) : base(client, automa) => _api = new WebApiClient(options);

        public override async Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
        {
            var response = await _api.Post<LoginResponse>("Login", new Args
            {
                { "_client", _client.GetClientArgs() },
                { "credential", credential },
                { "tag", (tag, tag?.GetType().AssemblyQualifiedName) },
            }, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null)
        {
            var response = await _api.Post<SelectApplicationResponse>("SelectApplication", new Args
            {
                { "_client", _client.GetClientArgs() },
                { "application", application },
                { "tag", (tag, tag?.GetType().AssemblyQualifiedName) },
            }, cancellationToken).ConfigureAwait(false);
            return null;
        }

        public override async Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null)
        {
            var response = await _api.Post<SetDeviceAccessTokenResponse>("SetDeviceAccessToken", new Args
            {
                { "_client", _client.GetClientArgs() },
                { "url", url },
                { "userCode", userCode },
                { "tag", (tag, tag?.GetType().AssemblyQualifiedName) },
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
