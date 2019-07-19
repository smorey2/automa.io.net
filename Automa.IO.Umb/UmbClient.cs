using System;
using System.IO;
using System.Net.Http;

namespace Automa.IO.Umb
{
    /// <summary>
    /// UmbClient
    /// </summary>
    /// <seealso cref="Automa.IO.AutomaClient" />
    public partial class UmbClient : AutomaClient
    {
        string UmbUri => "https://commercialcard.umb.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbClient" /> class.
        /// </summary>
        public UmbClient()
            : base(x => new Automa(x, (ctx, driver) => new UmbAutomation(x, ctx, driver), 0M)) { }

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
                case AccessMode.Request: return ((string)value).IndexOf(">Session Expired<") != -1;
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
        public bool RunReport(string report, Action<HtmlFormPost> action, string executeFolder, Func<string, string> interceptFilename = null)
        {
            string body, url;
            // parse
            {
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{UmbUri}/{report}"));
                var d1 = d0.ExtractSpan("<form name=\"parameterForm\"", "</form>");
                var htmlForm = new HtmlFormPost(HtmlFormPost.Mode.Form, d1);
                action(htmlForm);
                body = htmlForm.ToString();
                url = $"{UmbUri}/{Path.GetDirectoryName(report)}/{htmlForm.Action}";
            }
            // download
            {
                var d0 = this.TryFunc(() => this.DownloadFile(executeFolder, HttpMethod.Get, url, body, interceptFilename: interceptFilename));
                return true;
            }
        }

        #endregion
    }
}
