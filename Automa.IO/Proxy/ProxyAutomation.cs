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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyAutomation"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        public ProxyAutomation(AutomaClient client, IAutoma automa) : base(client, automa) => _proxyDriver = (ProxyDriver)automa.Driver;

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
            => _proxyDriver.LoginAsync(cookies, credential, tag, cancellationToken);

        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// System.Object.
        /// </returns>
        public override Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null)
            => _proxyDriver.SelectApplicationAsync(application, tag, cancellationToken);

        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userCode">The user code.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null)
            => _proxyDriver.SetDeviceAccessTokenAsync(url, userCode, tag, cancellationToken);

        /// <summary>
        /// Customs the asynchronous.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override Task<object> CustomAsync(Type registration, ICustom custom, object param = null, object tag = null, CancellationToken? cancellationToken = null)
            => _proxyDriver.CustomAsync(registration, custom, param, tag, cancellationToken);
    }
}
