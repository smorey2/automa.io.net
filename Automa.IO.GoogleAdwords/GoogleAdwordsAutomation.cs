using OpenQA.Selenium;

namespace Automa.IO.GoogleAdwords
{
    /// <summary>
    /// GoogleAdwordsAutomation
    /// </summary>
    /// <seealso cref="Automa.IO.Automation" />
    public class GoogleAdwordsAutomation : Automation
    {
        const string GoogleUri = "https://www.google.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleAdwordsAutomation" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        public GoogleAdwordsAutomation(AutomaClient client, IAutoma automa) : base(client, automa) { }
    }
}
