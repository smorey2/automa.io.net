using Automa.IO.Proxy;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Automa.IO.Umb
{
    /// <summary>
    /// UmbClient
    /// </summary>
    /// <seealso cref="Automa.IO.AutomaClient" />
    public partial class UmbClient : AutomaClient
    {
        public string UmbUri => "https://commercialcard.umb.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbClient" /> class.
        /// </summary>
        /// <param name="proxyOptions">The proxy options.</param>
        public UmbClient(IProxyOptions proxyOptions = null)
            : base(client => new Automa(client, automa => new UmbAutomation(client, automa), 0M), proxyOptions) { }

        /// <summary>
        /// Ensures the access.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null)
        {
            switch (mode)
            {
                case AccessMode.Request: return ((string)value).IndexOf("Session Expired") != -1;
            }
            return false;
        }

        #region Report

        /// <summary>
        /// Runs the report.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="action">The action.</param>
        /// <param name="executeFolder">The execute folder.</param>
        /// <param name="interceptFilename">The intercept filename.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="InvalidOperationException">unexpected form action returned from unanet</exception>
        public async Task<bool> RunReportAsync(string report, Action<HtmlFormPost> action, string executeFolder, Func<string, string> interceptFilename = null)
        {
            string body, url;
            // parse
            {
                var d0 = await this.TryFunc(() => this.DownloadDataAsync(HttpMethod.Get, $"{UmbUri}/{report}"));
                if (d0.IndexOf("Session Expired") != -1)
                    throw new LoginRequiredException();
                var d1 = d0.ExtractSpan("<form name=\"parameterForm\"", "</form>");
                if (string.IsNullOrEmpty(d1))
                    throw new InvalidOperationException("Report error");
                var htmlForm = new HtmlFormPost(d1);
                action(htmlForm);
                body = htmlForm.ToString();
                url = $"{UmbUri}/{Path.GetDirectoryName(report)}/{htmlForm.Action}";
            }
            // download
            {
                var d0 = await this.TryFunc(() => this.DownloadFileAsync(executeFolder, HttpMethod.Get, url, body, interceptFilename: interceptFilename));
                return !string.IsNullOrEmpty(d0);
            }
        }

        #endregion
    }
}
