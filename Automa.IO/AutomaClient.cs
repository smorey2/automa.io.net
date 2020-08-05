using Automa.IO.Proxy;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Automa.IO
{
    /// <summary>
    /// AutomaClient
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class AutomaClient : ITryMethod, IHasCookies, IDisposable
    {
        public static readonly IAutoma EmptyAutoma = new InternalEmptyAutoma();
        readonly Func<AutomaClient, IAutoma> _automaFactory;
        IAutoma _automa;

        /// <summary>
        /// The _logger
        /// </summary>
        protected Action<string> _logger = x => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaClient" /> class.
        /// </summary>
        /// <param name="automaFactory">The client factory.</param>
        public AutomaClient(Func<AutomaClient, IAutoma> automaFactory = null, IProxyOptions proxyOptions = null)
        {
            _automaFactory = automaFactory;
            ProxyOptions = proxyOptions;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Automa = null;

        /// <summary>
        /// Gets the proxy options.
        /// </summary>
        /// <value>
        /// The proxy options.
        /// </value>
        public IProxyOptions ProxyOptions { get; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        /// <exception cref="ArgumentNullException">value</exception>
        public Action<string> Logger
        {
            get => _logger;
            set => _logger = value ?? throw new ArgumentNullException("value");
        }

        /// <summary>
        /// Gets the automa.
        /// </summary>
        /// <value>The automa.</value>
        public IAutoma Automa
        {
            get => _automa ??= _automaFactory?.Invoke(this) ?? EmptyAutoma;
            set
            {
                if (_automa != value)
                {
                    if (_automa != null && value != EmptyAutoma)
                        _automa.Dispose();
                    _automa = value == EmptyAutoma ? null : value;
                }
            }
        }

        public async Task<IWebDriver> GetDriverAsync(object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            await AutomaLoginAsync(false, tag, loginTimeoutInSeconds);
            return Automa.Driver.Driver;
        }

        /// <summary>
        /// Gets or sets the type of the driver.
        /// </summary>
        /// <value>
        /// The type of the driver.
        /// </value>
        public Type DriverType { get; set; } = typeof(ChromeDriver);

        #region Credentials

        /// <summary>
        /// Gets or sets the service login.
        /// </summary>
        /// <value>The service login.</value>
        public string ServiceLogin { get; set; }

        /// <summary>
        /// Gets or sets the service password.
        /// </summary>
        /// <value>The service password.</value>
        public string ServicePassword { get; set; }

        /// <summary>
        /// Gets or sets the service credential.
        /// </summary>
        /// <value>The service credential.</value>
        public string ServiceCredential { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the connection parameters.
        /// </summary>
        /// <value>
        /// The connection parameters.
        /// </value>
        public Dictionary<string, string> ConnectionParams { get; set; }

        /// <summary>
        /// Ensures the service login and password.
        /// </summary>
        /// <exception cref="InvalidOperationException">ServiceCredential or ServiceLogin and ServicePassword are required for this operation.</exception>
        /// <exception cref="System.InvalidOperationException">ServiceCredential or, ServiceLogin and ServicePassword are required for this operation.</exception>
        //protected void EnsureServiceLoginAndPassword()
        //{
        //    if (string.IsNullOrEmpty(ServiceCredential))
        //        throw new ArgumentNullException(nameof(ServiceCredential), "Argument required for this operation.");
        //    if (string.IsNullOrEmpty(ServiceLogin))
        //        throw new ArgumentNullException(nameof(ServiceLogin), "Argument required for this operation.");
        //    if (string.IsNullOrEmpty(ServicePassword))
        //        throw new ArgumentNullException(nameof(ServicePassword), "Argument required for this operation.");
        //}

        /// <summary>
        /// Gets the network credential.
        /// </summary>
        /// <returns>NetworkCredential.</returns>
        /// <exception cref="InvalidOperationException">Unable to read credential store</exception>
        public virtual NetworkCredential GetNetworkCredential()
        {
            if (string.IsNullOrEmpty(ServiceCredential))
                return new NetworkCredential { UserName = ServiceLogin, Password = ServicePassword };
            if (CredentialManager.TryRead(ServiceCredential, CredentialManager.CredentialType.GENERIC, out var credential) != 0)
                throw new InvalidOperationException("Unable to read credential store");
            return new NetworkCredential { UserName = credential.UserName, Password = credential.CredentialBlob };
        }

        /// <summary>
        /// Parses the connection string.
        /// </summary>
        public virtual void ParseConnectionString()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return;
            foreach (var param in ConnectionString.Split(';'))
            {
                if (string.IsNullOrEmpty(param)) continue;
                var kv = param.Split(new[] { '=' }, 2);
                if (kv.Length > 1 && string.Equals(kv[0], "Credential", StringComparison.OrdinalIgnoreCase)) ServiceCredential = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "User Id", StringComparison.OrdinalIgnoreCase)) ServiceLogin = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "Password", StringComparison.OrdinalIgnoreCase)) ServicePassword = kv[1];
                else (ConnectionParams = ConnectionParams ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).Add(kv[0].ToLowerInvariant(), kv.Length > 1 ? kv[1] : null);
            }
            ConnectionString = null;
        }

        /// <summary>
        /// Gets the certificate.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <param name="storeName">Name of the store.</param>
        /// <param name="location">The location.</param>
        /// <returns>X509Certificate2.</returns>
        public virtual X509Certificate2 GetCertificate(string thumbprint, string storeName = "MY", StoreLocation location = StoreLocation.LocalMachine)
        {
            var store = new X509Store(storeName, location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            var x509Certificate = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)[0];
            store.Close();
            return x509Certificate;
        }

        #endregion

        #region AccessToken

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public virtual string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the access token writer.
        /// </summary>
        /// <value>The access token writer.</value>
        public Action<string> AccessTokenWriter { get; set; }

        /// <summary>
        /// Accesses the token flush.
        /// </summary>
        public Task AccessTokenFlushAsync() { AccessTokenWriter?.Invoke(AccessToken); return Task.CompletedTask; }

        #endregion

        #region Cookies

        /// <summary>
        /// Gets or sets the cookies.
        /// </summary>
        /// <value>The cookies.</value>
        public CookieCollection Cookies { get; set; } = new CookieCollection();

        /// <summary>
        /// Gets or sets the type of the cookie storage.
        /// </summary>
        /// <value>The type of the cookie storage.</value>
        public CookieStorageType CookieStorageType { get; set; }

        /// <summary>
        /// Gets or sets the cookies writer.
        /// </summary>
        /// <value>The cookies writer.</value>
        public Action<byte[]> CookiesWriter { get; set; }

        /// <summary>
        /// Cookieses the flush.
        /// </summary>
        public Task CookiesFlushAsync() { CookiesWriter?.Invoke(CookiesBytes); return Task.CompletedTask; }

        /// <summary>
        /// Gets or sets the cookies value.
        /// </summary>
        /// <value>The cookies value.</value>
        public byte[] CookiesBytes
        {
            get => this.GetCookiesAsync(CookieStorageType).GetAwaiter().GetResult();
            set => this.SetCookiesAsync(value, CookieStorageType).Wait();
        }

        #endregion

        #region TryMethods

        /// <summary>
        /// Ensures the access.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null) => true;

        /// <summary>
        /// Tries the login.
        /// </summary>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        public virtual Task TryLoginAsync(bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M) => AutomaLoginAsync(closeAfter, tag, loginTimeoutInSeconds);

        /// <summary>
        /// Automas the login.
        /// </summary>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="WebDriverException"></exception>
        protected virtual async Task AutomaLoginAsync(bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = -1M)
        {
            _logger("AutomaClient::Login");
            try
            {
                await Automa.LoginAsync(tag, loginTimeoutInSeconds);
                Cookies = Automa.Cookies;
                await CookiesFlushAsync();
            }
            catch (Exception e)
            {
                _logger(e.Message);
                if (e.Message.StartsWith("session not created:")) throw new WebDriverException(e.Message, e);
                else throw;
            }
            finally
            {
                if (closeAfter)
                    try { Automa.Dispose(); }
                    catch { }
                _logger("AutomaClient::Done");
            }
        }

        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="selectTimeoutInSeconds">The select timeout in seconds.</param>
        /// <returns>System.Object.</returns>
        public virtual async Task<object> AutomaSelectApplicationAsync(string application, object tag = null, decimal selectTimeoutInSeconds = -1M)
        {
            _logger("AutomaClient::SelectApplication");
            await Automa.SelectApplicationAsync(application, tag, selectTimeoutInSeconds);
            return null;
        }

        #endregion
    }
}