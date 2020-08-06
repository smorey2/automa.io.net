using System.Net.Http;
using System.Threading.Tasks;

namespace Automa.IO.Facebook
{
    public partial class FacebookClient
    {
        /// <summary>
        /// Creates the custom audience.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns>System.String.</returns>
        public async Task<string> CreateCustomAudience(long accountId)
        {
            EnsureAppIdAndSecret();
            return await this.TryFuncAsync(async () =>
           {
               var r = await this.DownloadJsonAsync(HttpMethod.Post, $"{BASEv}/act_{accountId}/customaudiences".ExpandPathAndQuery(new { subtype = "CUSTOM" })).ConfigureAwait(false);
               return (string)null;
           });
        }
    }
}
