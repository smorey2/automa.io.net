﻿using OpenQA.Selenium;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO.Okta
{
    /// <summary>
    /// OktaAutomation.
    /// </summary>
    /// <seealso cref="Automa.IO.Automation" />
    public class OktaAutomation : Automation
    {
        //const string WorkdayUri = "https://wd3.myworkday.com/dentsuaegis/d";
        //const string OktaUri = "https://isobar.okta.com";
        readonly Uri _oktaUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="OktaAutomation" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        /// <param name="oktaUri">The okta URI.</param>
        public OktaAutomation(AutomaClient client, IAutoma automa, Uri oktaUri) : base(client, automa) => _oktaUri = oktaUri;

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
            if (newUrl.StartsWith($"{_oktaUri}/login/login.htm"))
                throw new LoginRequiredException();
            //if (newUrl != WorkdayUri + "/public/index.htm")
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
        /// <exception cref="System.InvalidOperationException">
        /// expected login url
        /// or
        /// expected different url3
        /// or
        /// Push not accepted in time.
        /// or
        /// Not on homepage
        /// </exception>
        public override Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
        {
            // okta
            _driver.Navigate().GoToUrl($"{_oktaUri}/login/login.htm");
            var url = _driver.Url;
            if (!url.StartsWith($"{_oktaUri}/login/login.htm"))
                throw new InvalidOperationException("expected login url");
            // login
            var loginElement = _driver.FindElement(By.Name("username"));
            loginElement.SendKeys(credential.UserName);
            // password
            var passwordElement = _driver.FindElement(By.Name("password"));
            passwordElement.SendKeys(credential.Password);
            // remember
            _driver.JavaScriptClickByElementId(By.Name("remember"));
            passwordElement.SendKeys(Keys.Return);
            // done
            if (!_driver.WaitForUrl(500, $"{_oktaUri}/signin/verify/okta/push", $"{_oktaUri}/app/UserHome"))
                throw new InvalidOperationException("expected different url3");
            if (_driver.Url == $"{_oktaUri}/signin/verify/okta/push")
            {
                // rememberDevice
                var rememberDeviceElement = _driver.JavaScriptClickByElementId(By.Name("rememberDevice"));
                // button-primary
                rememberDeviceElement.SendKeys(Keys.Return);
                if (!_driver.WaitForUrl(3000, $"{_oktaUri}/app/UserHome"))
                    throw new InvalidOperationException("Push not accepted in time.");
            }
            if (!_driver.WaitForUrl(500, $"{_oktaUri}/app/UserHome"))
                throw new InvalidOperationException("Not on homepage");
            //cookies(_automa.Cookies);

            //// workday
            //var app = FindApp("workday", out var appUrl);
            //if (app == null)
            //    throw new InvalidOperationException("unable to find app");
            //app.Click();
            //if (!_driver.WaitForUrl(500, appUrl))
            //    throw new InvalidOperationException("Not on app page");
            //cookies(_automa.Cookies);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Selects the application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// System.Object.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">more than one application found</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">application</exception>
        public override async Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null)
        {
            // okta
            var url = await GoToUrlAsync($"{_oktaUri}/app/UserHome", cancellationToken).ConfigureAwait(false);
            if (!url.StartsWith($"{_oktaUri}/login/login.htm"))
                await _client.TryLoginAsync(false).ConfigureAwait(false);
            var apps = _driver.FindElements(By.ClassName("app-button"));
            if (apps.Count != 1)
                throw new InvalidOperationException("more than one application found");
            switch (application)
            {
                case "workday": return null; // appUrl = $"{WorkdayUri}/home.htmld"; return apps[0];
                default: throw new ArgumentOutOfRangeException(nameof(application), application);
            }
        }
    }
}
