using OpenQA.Selenium;
using System;
using System.Net;

namespace Automa.IO.Unanet
{
    /// <summary>
    /// UnanetAutomation
    /// </summary>
    /// <seealso cref="Automa.IO.Automation" />
    public class UnanetAutomation : Automation
    {
        readonly IUnanetSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnanetAutomation" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        /// <param name="driver">The driver.</param>
        /// <param name="unanetId">The unanet identifier.</param>
        /// <param name="sandbox">if set to <c>true</c> [sandbox].</param>
        public UnanetAutomation(AutomaClient client, IAutoma automa, IWebDriver driver, IUnanetSettings settings) : base(client, automa, driver) => _settings = settings;

        /// <summary>
        /// Tries the go to URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <exception cref="LoginRequiredException"></exception>
        public void TryGoToUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
            var title = _driver.Url;
            //if (url != UnanetUri + "/public/index.htm")
            //    throw new LoginRequiredException();
        }

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <exception cref="AutomaEx.LoginRequiredException"></exception>
        public override void Login(Func<CookieCollection, CookieCollection> cookies, NetworkCredential credential, object tag = null)
        {
            _driver.Navigate().GoToUrl(_settings.UnanetUri + "/home");
            var url = _driver.Url;
            if (!url.StartsWith(_settings.UnanetUri + "/home"))
                throw new LoginRequiredException();
            // username
            var loginElement = _driver.FindElement(By.Name("username"));
            loginElement.SendKeys(credential.UserName);
            // password
            var passwordElement = _driver.FindElement(By.Name("password"));
            passwordElement.SendKeys(credential.Password);
            // done
            passwordElement.SendKeys(Keys.Return);
            url = _driver.Url;
            if (!url.StartsWith(_settings.UnanetUri + "/home"))
                throw new LoginRequiredException();
            //cookies(_automa.Cookies);
        }
    }
}
