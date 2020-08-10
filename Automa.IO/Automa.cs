using Automa.IO.Drivers;
using Automa.IO.Proxy;
using OpenQA.Selenium;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Automa.IO
{
    /// <summary>
    /// IAutoma
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IAutoma : IDisposable
    {
        /// <summary>
        /// Gets or sets the cookies.
        /// </summary>
        /// <value>The cookies.</value>
        CookieCollection Cookies { get; set; }
        /// <summary>
        /// Customs the asynchronous.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <returns></returns>
        Task<(object value, ICustom custom)> CustomAsync(Type registration, object param = null, object tag = null, decimal timeoutInSeconds = -1M);
        /// <summary>
        /// Gets the driver.
        /// </summary>
        /// <value>The driver.</value>
        AbstractDriver Driver { get; }
        /// <summary>
        /// Logins this instance.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        Task LoginAsync(object tag = null, decimal timeoutInSeconds = -1M);
        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="selectTimeoutInSeconds">The select timeout in seconds.</param>
        /// <returns>System.Object.</returns>
        Task<object> SelectApplicationAsync(string application, object tag = null, decimal selectTimeoutInSeconds = -1M);
        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="code">The code.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        Task SetDeviceAccessTokenAsync(string url, string code, object tag = null, decimal timeoutInSeconds = -1M);
    }

    /// <summary>
    /// EmptyAutoma.
    /// </summary>
    /// <seealso cref="Automa.IO.IAutoma" />
    internal class InternalEmptyAutoma : IAutoma
    {
        public CookieCollection Cookies { get; set; } = new CookieCollection();
        public AbstractDriver Driver => null;
        public void Dispose() { }
        public Task<(object value, ICustom custom)> CustomAsync(Type registration, object param = null, object tag = null, decimal timeoutInSeconds = -1) => throw new NotSupportedException();
        public Task LoginAsync(object tag = null, decimal timeoutInSeconds = -1) => Task.CompletedTask;
        public Task SetDeviceAccessTokenAsync(string url, string code, object tag = null, decimal timeoutInSeconds = -1) => throw new NotSupportedException();
        public Task<object> SelectApplicationAsync(string application, object tag = null, decimal selectTimeoutInSeconds = -1) => throw new NotSupportedException();
    }

    /// <summary>
    /// Automa
    /// </summary>
    /// <seealso cref="Automa.IO.IAutoma" />
    public class Automa : IAutoma
    {
        public static string UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

        readonly IAutomation _automation;
        readonly AutomaClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Automa" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automationFactory">The automation factory.</param>
        /// <param name="defaultTimeoutInSeconds">The default timeout in seconds.</param>
        /// <param name="driverOptions">The driver options.</param>
        public Automa(AutomaClient client, Func<IAutoma, IAutomation> automationFactory, decimal defaultTimeoutInSeconds = 60M, Action<DriverOptions> driverOptions = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Driver = client.ProxyOptions == null || string.IsNullOrEmpty(client.ProxyOptions.ProxyUri)
                ? (AbstractDriver)Activator.CreateInstance(client.DriverType, driverOptions)
                : new ProxyDriver(client, client.ProxyOptions.ProxyToken);
            _automation = automationFactory?.Invoke(this) ?? throw new ArgumentNullException(nameof(automationFactory));
            DefaultTimeoutInSeconds = defaultTimeoutInSeconds;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose()
        {
            if (_client != null)
                _client.Automa = AutomaClient.EmptyAutoma;
            Driver.Dispose();
        }

        /// <summary>
        /// Gets or sets the default timeout is seconds.
        /// </summary>
        /// <value>The default timeout is seconds.</value>
        public decimal DefaultTimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets the cookies.
        /// </summary>
        /// <value>The cookies.</value>
        /// <exception cref="ArgumentNullException">value</exception>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public CookieCollection Cookies
        {
            get => Driver.Cookies;
            set => Driver.Cookies = value;
        }

        /// <summary>
        /// Customs the asynchronous.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="selectTimeoutInSeconds">The select timeout in seconds.</param>
        /// <returns></returns>
        public Task<(object value, ICustom custom)> CustomAsync(Type registration, object param = null, object tag = null, decimal selectTimeoutInSeconds = -1M)
        {
            if (!AutomaClient.CustomRegistry.TryGetValue(registration, out var custom))
                throw new ArgumentOutOfRangeException(nameof(registration), registration.ToString());
            //if (selectTimeoutInSeconds == -1M) selectTimeoutInSeconds = DefaultTimeoutInSeconds;
            Func<object> func = () => _automation.CustomAsync(registration, custom, param, tag).ConfigureAwait(false).GetAwaiter().GetResult();
            try
            {
                if (selectTimeoutInSeconds > 0) return Task.FromResult((func.TimeoutInvoke((int)(selectTimeoutInSeconds * 1000M)), custom));
                else return Task.FromResult((func(), custom));
            }
            catch (Exception e) { Console.WriteLine(e); throw; }
        }

        /// <summary>
        /// Gets the driver.
        /// </summary>
        /// <value>The driver.</value>
        public AbstractDriver Driver { get; }

        /// <summary>
        /// Logins this instance.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        public Task LoginAsync(object tag = null, decimal timeoutInSeconds = -1M)
        {
            _client.ParseConnectionString();
            if (timeoutInSeconds == -1M) timeoutInSeconds = DefaultTimeoutInSeconds;
            Action action = () => _automation.LoginAsync(_client.CookieGetSetAsync, _client.GetNetworkCredential(), tag).Wait();
            try
            {
                if (timeoutInSeconds > 0) action.TimeoutInvoke((int)(timeoutInSeconds * 1000M));
                else action();
                return Task.CompletedTask;
            }
            catch (Exception e) { Console.WriteLine(e); throw; }
        }

        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="selectTimeoutInSeconds">The select timeout in seconds.</param>
        /// <returns>System.Object.</returns>
        public Task<object> SelectApplicationAsync(string application, object tag = null, decimal selectTimeoutInSeconds = -1M)
        {
            if (selectTimeoutInSeconds == -1M) selectTimeoutInSeconds = DefaultTimeoutInSeconds;
            Func<object> func = () => _automation.SelectApplicationAsync(application, tag).ConfigureAwait(false).GetAwaiter().GetResult();
            try
            {
                if (selectTimeoutInSeconds > 0) return Task.FromResult(func.TimeoutInvoke((int)(selectTimeoutInSeconds * 1000M)));
                else return Task.FromResult(func());
            }
            catch (Exception e) { Console.WriteLine(e); throw; }
        }

        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="code">The code.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        public Task SetDeviceAccessTokenAsync(string url, string code, object tag = null, decimal timeoutInSeconds = -1M)
        {
            if (timeoutInSeconds == -1M) timeoutInSeconds = DefaultTimeoutInSeconds;
            Action action = () => TryAction(x => _automation.SetDeviceAccessTokenAsync(url, code).Wait(), tag, timeoutInSeconds);
            try
            {
                if (timeoutInSeconds > 0) action.TimeoutInvoke((int)(timeoutInSeconds * 1000M));
                else action();
                return Task.CompletedTask;
            }
            catch (Exception e) { Console.WriteLine(e); throw; }
        }

        void TryAction(Action<IAutoma> action, object tag = null, decimal timeoutInSeconds = -1M)
        {
            if (timeoutInSeconds == -1M) timeoutInSeconds = DefaultTimeoutInSeconds;
            try { action(this); }
            catch (LoginRequiredException)
            {
                LoginAsync(tag, timeoutInSeconds).Wait();
                action(this);
            }
        }

        T TryFunc<T>(Func<IAutoma, T> action, object tag = null, decimal timeoutInSeconds = -1M)
        {
            if (timeoutInSeconds == -1M) timeoutInSeconds = DefaultTimeoutInSeconds;
            try { return action(this); }
            catch (LoginRequiredException)
            {
                LoginAsync(tag, timeoutInSeconds).Wait();
                return action(this);
            }
        }
    }
}