using OpenQA.Selenium;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO.Unanet
{
    /// <summary>
    /// UnanetAutomation
    /// </summary>
    /// <seealso cref="Automa.IO.Automation" />
    public class UnanetAutomation : Automation
    {
        readonly IUnanetOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnanetAutomation" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        /// <param name="options">The options.</param>
        public UnanetAutomation(AutomaClient client, IAutoma automa, IUnanetOptions options) : base(client, automa) => _options = options;

        /// <summary>
        /// Goes to URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="LoginRequiredException"></exception>
        public override Task<string> GoToUrlAsync(string url, CancellationToken? cancellationToken = null)
        {
            _driver.Navigate().GoToUrl(url);
            var newUrl = _driver.Url;
            //if (newUrl != $"{UnanetUri}/public/index.htm")
            //    throw new LoginRequiredException();
            return Task.FromResult(newUrl);
        }

        /// <summary>
        /// Logins the specified cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="LoginRequiredException">
        /// </exception>
        public override Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
        {
            _driver.Navigate().GoToUrl($"{_options.UnanetUri}/home");
            var url = _driver.Url;
            if (!url.StartsWith($"{_options.UnanetUri}/home"))
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
            if (!url.StartsWith($"{_options.UnanetUri}/home"))
                throw new LoginRequiredException();
            //cookies(_automa.Cookies);
            return Task.CompletedTask;
        }
    }
}
