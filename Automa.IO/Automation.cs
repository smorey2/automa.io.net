using OpenQA.Selenium;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO
{
    /// <summary>
    /// IAutomation
    /// </summary>
    public interface IAutomation
    {
        /// <summary>
        /// Goes to URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// System.String.
        /// </returns>
        Task<string> GoToUrlAsync(string url, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userCode">The user code.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// System.Object.
        /// </returns>
        Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null);
    }

    /// <summary>
    /// GenericAutomation.
    /// </summary>
    /// <seealso cref="Automa.IO.IAutomation" />
    public class Automation : IAutomation
    {
        protected readonly AutomaClient _client;
        protected readonly IAutoma _automa;
        protected readonly IWebDriver _driver;

        /// <summary>
        /// Initializes a new instance of the <see cref="Automation" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        public Automation(AutomaClient client, IAutoma automa)
        {
            _client = client;
            _automa = automa;
            _driver = automa.Driver?.Driver;
        }

        /// <summary>
        /// Goes to URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="LoginRequiredException"></exception>
        public virtual Task<string> GoToUrlAsync(string url, CancellationToken? cancellationToken = null) => throw new NotSupportedException();

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null) => throw new NotSupportedException();

        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// System.Object.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null) => throw new NotSupportedException();

        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userCode">The user code.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public virtual Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null) => throw new NotSupportedException();
    }
}
