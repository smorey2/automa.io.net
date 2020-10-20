using OpenQA.Selenium;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO.Adp
{
    /// <summary>
    /// AdpAutomation
    /// </summary>
    public class AdpAutomation : Automation
    {
        const string WorkforcenowAdpUri = "https://workforcenow.adp.com";
        const string EwalletAdpUri = "https://ewallet.adp.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="AdpAutomation" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        public AdpAutomation(AutomaClient client, IAutoma automa) : base(client, automa) { }

        /// <summary>
        /// Goes to URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// System.String.
        /// </returns>
        /// <exception cref="LoginRequiredException"></exception>
        public override Task<string> GoToUrlAsync(string url, CancellationToken? cancellationToken = null)
        {
            _driver.Navigate().GoToUrl(url);
            var newUrl = _driver.Url;
            if (newUrl != $"{WorkforcenowAdpUri}/public/index.htm")
                throw new LoginRequiredException();
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
            _driver.Navigate().GoToUrl($"{WorkforcenowAdpUri}/portal/admin.jsp");
            var url = _driver.Url;
            if (!url.StartsWith($"{EwalletAdpUri}/auth/enroll/adpLogin.faces"))
                throw new LoginRequiredException();
            // login
            var loginContainer = _driver.FindElement(By.ClassName("user-id"));
            var loginElement = loginContainer.FindElement(By.TagName("input"));
            loginElement.SendKeys(credential.UserName);
            loginElement.SendKeys(Keys.Return);
            // password
            //var passwordContainer = WaitForElement(1000, By.ClassName("password"));
            //if (passwordContainer == null)
            //    throw new LoginRequiredException();
            if (!_driver.WaitForDisplay(out var elements, 1000, By.ClassName("password")))
                throw new LoginRequiredException();
            var passwordElement = elements[0].FindElement(By.TagName("input"));
            passwordElement.SendKeys(credential.Password);
            passwordElement.SendKeys(Keys.Return);
            // done
            if (!_driver.WaitForUrl(2000, $"{WorkforcenowAdpUri}/theme/index.html", $"{WorkforcenowAdpUri}/theme/admin.html"))
                throw new LoginRequiredException();
            cookies(_automa.Cookies);
            return Task.CompletedTask;
        }
    }
}
