using OpenQA.Selenium;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO.Facebook
{
    /// <summary>
    /// FacebookAutomation
    /// </summary>
    /// <seealso cref="Automa.IO.Automation" />
    public class FacebookAutomation : Automation
    {
        const string FacebookUri = "https://www.facebook.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAutomation" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        public FacebookAutomation(AutomaClient client, IAutoma automa) : base(client, automa) { }

        /// <summary>
        /// Tries the go to URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="LoginRequiredException"></exception>
        public override Task<string> GoToUrlAsync(string url, CancellationToken? cancellationToken = null)
        {
            _driver.Navigate().GoToUrl(url);
            var title = _driver.Title;
            if (title.Contains("Log in") || title.Contains("Log In"))
                throw new LoginRequiredException();
            return Task.FromResult(title);
        }

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="LoginRequiredException"></exception>
        public override Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
        {
            _driver.Navigate().GoToUrl(FacebookUri);
            var loginElement = _driver.FindElement(By.Id("email"));
            loginElement.SendKeys(credential.UserName);
            var passwordElement = _driver.FindElement(By.Id("pass"));
            passwordElement.SendKeys(credential.Password);
            var loginLabel = _driver.FindElement(By.Id("loginbutton"));
            var loginButton = loginLabel.FindElement(By.TagName("input"));
            loginButton.Click();
            var title = _driver.Title;
            if (title.Contains("Log in") || title.Contains("Log In"))
                throw new LoginRequiredException();
            //cookies(_automa.Cookies);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userCode">The code.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public override async Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null)
        {
            var title = await GoToUrlAsync(url);
            if (!title.Contains("Devices"))
                throw new InvalidOperationException();
            var loginElement = _driver.FindElement(By.Name("user_code"));
            loginElement.SendKeys(userCode);
            var continueButton = _driver.FindElement(By.ClassName("oauth_device_code_continue_button"));
            if (continueButton.Displayed) continueButton.Click();
            else
            {
                var loginButton = _driver.FindElement(By.ClassName("selected"));
                if (loginButton.Displayed) loginButton.Click();
            }
            for (var i = 0; i < 3; i++)
            {
                var pageSource = _driver.PageSource;
                if (pageSource.Contains("Success!"))
                    return;
                var confirmElement = _driver.FindElement(By.Name("__CONFIRM__"));
                if (confirmElement != null && confirmElement.Displayed)
                    confirmElement.Click();
            }
        }
    }
}
