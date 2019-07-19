using OpenQA.Selenium;
using System;
using System.Net;

namespace Automa.IO.Umb
{
    /// <summary>
    /// UmbAutomation
    /// </summary>
    /// <seealso cref="Automa.IO.Automation" />
    public class UmbAutomation : Automation
    {
        string UmbIdentityUri => "https://identity.commercialcard.umb.com";
        string UmbUri => "https://commercialcard.umb.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbAutomation" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        /// <param name="driver">The driver.</param>
        public UmbAutomation(AutomaClient client, IAutoma automa, IWebDriver driver) : base(client, automa, driver) { }

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <exception cref="LoginRequiredException">
        /// </exception>
        /// <exception cref="AutomaEx.LoginRequiredException"></exception>
        public override void Login(Func<CookieCollection, CookieCollection> cookies, NetworkCredential credential, object tag = null)
        {
            _driver.Navigate().GoToUrl(UmbIdentityUri + "/login");
            var url = _driver.Url;
            if (!url.StartsWith(UmbIdentityUri + "/login"))
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
            if (!url.StartsWith(UmbUri + "/Site/#/home"))
                throw new LoginRequiredException();
            //cookies(_automa.Cookies);
        }
    }
}