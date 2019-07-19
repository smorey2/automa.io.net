using OpenQA.Selenium;
using System;
using System.Net;

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
        /// <returns>System.String.</returns>
        string GoToUrl(string url);

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        void Login(Func<CookieCollection, CookieCollection> cookies, NetworkCredential credential, object tag = null);

        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userCode">The user code.</param>
        /// <param name="tag">The tag.</param>
        void SetDeviceAccessToken(string url, string userCode, object tag = null);

        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>System.Object.</returns>
        object SelectApplication(string application, object tag = null);
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
        /// <param name="driver">The driver.</param>
        public Automation(AutomaClient client, IAutoma automa, IWebDriver driver)
        {
            _client = client;
            _automa = automa;
            _driver = driver;
        }

        /// <summary>
        /// Goes to URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <exception cref="LoginRequiredException"></exception>
        public virtual string GoToUrl(string url) => throw new NotSupportedException();

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <exception cref="NotSupportedException"></exception>
        public virtual void Login(Func<CookieCollection, CookieCollection> cookies, NetworkCredential credential, object tag = null) => throw new NotSupportedException();

        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual object SelectApplication(string application, object tag = null) => throw new NotSupportedException();

        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userCode">The user code.</param>
        /// <param name="tag">The tag.</param>
        /// <exception cref="NotSupportedException"></exception>
        public virtual void SetDeviceAccessToken(string url, string userCode, object tag = null) => throw new NotSupportedException();
    }
}
