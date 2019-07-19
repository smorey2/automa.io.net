using System.Net.Http;

namespace Automa.IO.Facebook
{
    public partial class FacebookClient
    {
        /// <summary>
        /// Creates the custom audience.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns>System.String.</returns>
        public string CreateCustomAudience(long accountId)
        {
            EnsureAppIdAndSecret();
            return this.TryFunc<string>(() =>
            {
                var r = this.DownloadJson(HttpMethod.Post, $"{BASEv}/act_{accountId}/customaudiences".ExpandPathAndQuery(new { subtype = "CUSTOM" }));
                return null;
            });
        }
    }
}
