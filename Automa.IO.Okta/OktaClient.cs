using Automa.IO.Proxy;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Args = System.Collections.Generic.Dictionary<string, object>;

namespace Automa.IO.Okta
{
    public partial class OktaClient : AutomaClient
    {
        readonly Uri _oktaUri;
        readonly string _oktaId;

        /// <summary>
        /// Initializes a new instance of the <see cref="OktaClient"/> class.
        /// </summary>
        /// <param name="oktaUri">The okta URI.</param>
        /// <param name="oktaId">The okta identifier.</param>
        public OktaClient(Uri oktaUri, string oktaId, IProxyOptions proxyOptions = null)
            : base(client => new Automa(client, automa => new OktaAutomation(client, automa, oktaUri), driverOptions: DriverOptions), proxyOptions)
        {
            _oktaUri = oktaUri;
            _oktaId = oktaId;
        }

        static void DriverOptions(DriverOptions driverOptions)
        {
            var options = (ChromeOptions)driverOptions;
        }

        #region Parse/Get

        /// <summary>
        /// Parses the client arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        protected static OktaClient ParseClientArgs(Args args) => new OktaClient(
            args.TryGetValue("oktaUri", out var z) ? new Uri(((JsonElement)z).GetString()) : null,
            args.TryGetValue("oktaId", out z) ? ((JsonElement)z).GetString() : null);

        /// <summary>
        /// Gets the client arguments.
        /// </summary>
        /// <returns></returns>
        public override Args GetClientArgs() =>
            new Args
            {
                { "_base", base.GetClientArgs() },
                { "oktaUri", _oktaUri.ToString() },
                { "oktaId", _oktaId },
            };

        #endregion

        #region Login

        public override bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null)
        {
            switch (mode)
            {
                case AccessMode.Request: return ((string)value).IndexOf($"/authgwy/{_oktaId}/login.htmld") != -1;
            }
            return false;
        }

        #endregion

        #region Report

        public Task<object> GetReportAsync(string url)
        {
            //var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{WorkdayUri}/d/home.htmld"));
            //var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{WorkdayUri}{url}"));
            return Task.FromResult<object>(null);
        }

        #endregion
    }
}
