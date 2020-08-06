using Automa.IO.Proxy;
using System;
using System.Text.Json;
using Args = System.Collections.Generic.Dictionary<string, object>;

namespace Automa.IO.Facebook
{
    /// <summary>
    /// FacebookContext
    /// </summary>
    /// <seealso cref="Automa.IO.AutomaClient" />
    public partial class FacebookClient : AutomaClient
    {
        /// <summary>
        /// The base
        /// </summary>
        protected const string BASE = "https://graph.facebook.com";
        /// <summary>
        /// The basev
        /// </summary>
        protected const string BASEv = "https://graph.facebook.com/v3.3";

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookClient" /> class.
        /// </summary>
        /// <param name="proxyOptions">The proxy options.</param>
        /// <exception cref="System.ArgumentNullException">f</exception>
        public FacebookClient(IProxyOptions proxyOptions = null)
            : base(client => new Automa(client, automa => new FacebookAutomation(client, automa)), proxyOptions)
        {
            RequestedScope = "manage_pages,ads_management"; //read_insights,leads_retrieval
            Logger = Console.WriteLine;
        }

        /// <summary>
        /// Gets or sets the requested scope.
        /// </summary>
        /// <value>The requested scope.</value>
        public string RequestedScope { get; set; }

        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the application secret.
        /// </summary>
        /// <value>The application secret.</value>
        public string AppSecret { get; set; }

        /// <summary>
        /// Gets or sets the client token.
        /// </summary>
        /// <value>The client token.</value>
        public string ClientToken { get; set; }

        #region Parse/Get

        /// <summary>
        /// Parses the client arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        protected static FacebookClient ParseClientArgs(Args args) => new FacebookClient
        {
            RequestedScope = args.TryGetValue("RequestedScope", out var z) ? ((JsonElement)z).GetString() : null,
            AppId = args.TryGetValue("AppId", out z) ? ((JsonElement)z).GetString() : null,
            AppSecret = args.TryGetValue("AppSecret", out z) ? ((JsonElement)z).GetString() : null,
            ClientToken = args.TryGetValue("ClientToken", out z) ? ((JsonElement)z).GetString() : null,
        };

        /// <summary>
        /// Gets the client arguments.
        /// </summary>
        /// <returns></returns>
        public override Args GetClientArgs() =>
            new Args
            {
                { "_base", base.GetClientArgs() },
                { "RequestedScope", RequestedScope },
                { "AppId", AppId },
                { "AppSecret", AppSecret },
                { "ClientToken", ClientToken },
            };

        #endregion

        #region Ensure

        void EnsureAppIdAndSecret()
        {
            if (string.IsNullOrEmpty(AppId) || string.IsNullOrEmpty(AppSecret))
                throw new InvalidOperationException("AppId and AppSecret are required for this operation.");
        }

        void EnsureAppIdAndToken()
        {
            if (string.IsNullOrEmpty(AppId) || string.IsNullOrEmpty(ClientToken))
                throw new InvalidOperationException("AppId and ClientToken are required for this operation.");
        }

        void EnsureRequestedScope()
        {
            if (string.IsNullOrEmpty(RequestedScope))
                throw new InvalidOperationException("RequestedScope is required for this operation.");
        }

        #endregion
    }
}
