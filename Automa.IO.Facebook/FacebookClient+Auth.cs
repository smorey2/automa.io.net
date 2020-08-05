using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO.Facebook
{
    public partial class FacebookClient
    {
        Dictionary<long, string> _accounts;

        void InterceptRequest(HttpWebRequest r) => r.Headers["Authorization"] = $"Bearer {AccessToken}";
        void InterceptRequestForAccount(HttpWebRequest r, long id)
        {
            string accessToken;
            if (_accounts == null || !_accounts.TryGetValue(id, out accessToken))
                throw new InvalidOperationException($"Unable to find Account {id} Access Token");
            r.Headers["Authorization"] = $"Bearer {accessToken}";
        }

        /// <summary>
        /// Ensures the access.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="System.UnauthorizedAccessException">User does not have page access</exception>
        public override bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null)
        {
            switch (mode)
            {
                case AccessMode.Exception:
                    {
                        var valueAsString = (string)value;
                        return valueAsString.IndexOf("(400) Bad Request") != -1 || valueAsString.IndexOf("(401) Unauthorized") != -1;
                    }
                case AccessMode.Request:
                    {
                        if (value is string valueAsString)
                        {
                            if (valueAsString.IndexOf("User does not have page access") != -1)
                                throw new UnauthorizedAccessException("User does not have page access");
                            int loginStart, loginEnd;
                            if ((loginStart = valueAsString.IndexOf("<form id=\"login_form\" action=\"/login.php?login_attempt=1&amp;")) > -1 && (loginEnd = valueAsString.IndexOf("</form>", loginStart)) > -1)
                                return true;
                        }
                        return false;
                    }
            }
            return false;
        }

        /// <summary>
        /// Tries the login.
        /// </summary>
        /// <param name="closeAfter">if set to <c>true</c> [close after].</param>
        /// <param name="tag">The tag.</param>
        /// <param name="loginTimeoutInSeconds">The login timeout in seconds.</param>
        public override async Task TryLoginAsync(bool closeAfter = true, object tag = null, decimal loginTimeoutInSeconds = 30)
        {
            using (var automa = Automa)
            {
                await AutomaLoginAsync(false, tag, loginTimeoutInSeconds);
                await SetDeviceAccessTokenAsync(RequestedScope, Automa.SetDeviceAccessTokenAsync, tag, loginTimeoutInSeconds);
                await SetExtendedAccessTokenAsync();
                await AccessTokenFlushAsync();
            }
        }

        /// <summary>
        /// Gets me.
        /// </summary>
        /// <returns>dynamic.</returns>
        public async Task<dynamic> GetMeAsync()
        {
            var r = await this.TryFunc(typeof(WebException), () => this.DownloadJsonAsync(HttpMethod.Get, $"{BASEv}/me?fields=id,name", interceptRequest: InterceptRequest), tag: true);
            return new
            {
                id = (string)r["id"],
                name = (string)r["name"],
            };
        }

        /// <summary>
        /// Loads me accounts.
        /// </summary>
        public async Task LoadMeAccountsAsync()
        {
            var r = await this.TryFunc(typeof(WebException), () => this.DownloadJsonAsync(HttpMethod.Get, $"{BASEv}/me/accounts?fields=id,name,access_token", interceptRequest: InterceptRequest), tag: true);
            _accounts = new Dictionary<long, string>();
            foreach (var i in (JArray)r["data"])
                _accounts.Add(long.Parse((string)i["id"]), (string)i["access_token"]);
        }

        /// <summary>
        /// Sets the device access token.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="action">The action.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public async Task<bool> SetDeviceAccessTokenAsync(string scope, Func<string, string, object, decimal, Task> action, object tag = null, decimal timeoutInSeconds = 30M)
        {
            EnsureAppIdAndToken();
            var r = await this.DownloadJsonAsync(HttpMethod.Post, $"{BASEv}/device/login?access_token={AppId}|{ClientToken}&scope={scope}");
            var code = (string)r["code"];
            var user_code = (string)r["user_code"];
            var verification_uri = (string)r["verification_uri"];
            var expiresInSeconds = (long)r["expires_in"];
            var intervalMilliseconds = (int)(long)r["interval"] * 1000;
            await action(verification_uri, user_code, tag, timeoutInSeconds);

            var polling = true;
            do
            {
                Console.Write(".");
                Thread.Sleep(intervalMilliseconds);
                try
                {
                    var r2 = await this.DownloadJsonAsync(HttpMethod.Post, $"{BASEv}/device/login_status?access_token={AppId}|{ClientToken}&code={code}");
                    AccessToken = (string)r2["access_token"];
                    return true;
                }
                catch (Exception e)
                {
                    var message = e.Message.Substring(e.Message.IndexOf(") ") + 2);
                    Console.Write(message);
                    switch (message)
                    {
                        case "authorization_pending": break;
                        case "authorization_declined": polling = false; break;
                        case "slow_down": break;
                        case "code_expired": polling = false; break;
                    }
                }
            }
            while (polling);
            return false;
        }

        /// <summary>
        /// Sets the extended access token.
        /// </summary>
        public async Task SetExtendedAccessTokenAsync()
        {
            EnsureAppIdAndSecret();
            var r = await this.DownloadJsonAsync(HttpMethod.Get, $"{BASE}/oauth/access_token?grant_type=fb_exchange_token&client_id={AppId}&client_secret={AppSecret}&fb_exchange_token={AccessToken}");
            AccessToken = (string)r["access_token"];
        }
    }
}
