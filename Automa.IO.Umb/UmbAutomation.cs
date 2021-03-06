﻿using OpenQA.Selenium;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
        public UmbAutomation(AutomaClient client, IAutoma automa) : base(client, automa) { }

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
            _driver.Navigate().GoToUrl($"{UmbIdentityUri}/login");
            var url = _driver.Url;
            if (!url.StartsWith($"{UmbIdentityUri}/login"))
                throw new LoginRequiredException();
            // username
            var loginElement = _driver.FindElement(By.Name("username"));
            loginElement.SendKeys(credential.UserName);
            // password
            var passwordElement = _driver.FindElement(By.Name("password"));
            passwordElement.SendKeys(credential.Password);
            // done
            passwordElement.SendKeys(Keys.Return);
            // login successful?
            url = _driver.Url;
            if (!url.StartsWith($"{UmbUri}/Site/#/home"))
                throw new LoginRequiredException();
            // password maintenance - password expired 
            //var rpElement = _driver.FindElement(By.Id("RP"));
            //if (rpElement != null)
            //{
            //    throw new LoginRequiredException("password expired");
            //}
            //cookies(_automa.Cookies);
            return Task.CompletedTask;
        }
    }
}