using Automa.IO.Proxy;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.v201809;
using System;

namespace Automa.IO.GoogleAdwords
{
    /// <summary>
    /// GoogleAdwordsContext
    /// </summary>
    public partial class GoogleAdwordsClient : AutomaClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleAdwordsClient" /> class.
        /// </summary>
        /// <param name="proxyOptions">The proxy options.</param>
        /// <exception cref="System.ArgumentNullException">f</exception>
        public GoogleAdwordsClient(IProxyOptions proxyOptions = null)
            : base(client => new Automa(client, automa => new GoogleAdwordsAutomation(client, automa)), proxyOptions)
        {
            Logger = Console.WriteLine;
            AdWordsUser = new AdWordsUser();
            AdWordsUser.Config.EnableGzipCompression = true;
        }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public override string AccessToken
        {
            get => AdWordsUser.Config.OAuth2RefreshToken;
            set => AdWordsUser.Config.OAuth2RefreshToken = value;
        }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string AppId
        {
            get => AdWordsUser.Config.OAuth2ClientId;
            set => AdWordsUser.Config.OAuth2ClientId = value;
        }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        public string AppSecret
        {
            get => AdWordsUser.Config.OAuth2ClientSecret;
            set => AdWordsUser.Config.OAuth2ClientSecret = value;
        }

        /// <summary>
        /// Gets or sets the developer token.
        /// </summary>
        /// <value>
        /// The developer token.
        /// </value>
        public string DeveloperToken
        {
            get => ((AdWordsAppConfig)AdWordsUser.Config).DeveloperToken;
            set => ((AdWordsAppConfig)AdWordsUser.Config).DeveloperToken = value;
        }

        /// <summary>
        /// Gets the ad words user.
        /// </summary>
        /// <value>
        /// The ad words user.
        /// </value>
        public AdWordsUser AdWordsUser { get; private set; }

        void EnsureAppIdAndSecret()
        {
            if (string.IsNullOrEmpty(AppId) || string.IsNullOrEmpty(AppSecret) || string.IsNullOrEmpty(DeveloperToken))
                throw new InvalidOperationException("AppId, AppSecret, and DeveloperToken are required for this operation.");
        }

        /// <summary>
        /// Tests the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public void Test(string accountId)
        {
            EnsureAppIdAndSecret();
            ((AdWordsAppConfig)AdWordsUser.Config).ClientCustomerId = accountId;
            var campaignService = (CampaignService)AdWordsUser.GetService(AdWordsService.v201809.CampaignService);
            var selector = new Selector
            {
                fields = new string[] { Campaign.Fields.Id, Campaign.Fields.Name, Campaign.Fields.Status },
                paging = Paging.Default,
            };
            CampaignPage page;
            do
            {
                // Get the campaigns.
                page = campaignService.get(selector);

                // Display the results.
                if (page != null && page.entries != null)
                {
                    var i = selector.paging.startIndex;
                    foreach (var campaign in page.entries)
                    {
                        Console.WriteLine($"{i + 1}) Campaign with id = '{campaign.id}', name = '{campaign.name}' and status = '{campaign.status}'  was found.");
                        i++;
                    }
                }
                selector.paging.IncreaseOffset();
            } while (selector.paging.startIndex < page.totalNumEntries);
            Console.WriteLine($"Number of campaigns found: {page.totalNumEntries}");
        }
    }
}
