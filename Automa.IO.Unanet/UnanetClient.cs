using Automa.IO.Proxy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Args = System.Collections.Generic.Dictionary<string, object>;

namespace Automa.IO.Unanet
{
    /// <summary>
    /// UnanetClient
    /// </summary>
    public partial class UnanetClient : AutomaClient
    {
        /// <summary>
        /// The download timeout in seconds
        /// </summary>
        protected readonly static int DownloadTimeoutInSeconds = 150;

        /// <summary>
        /// The bot
        /// </summary>
        public static readonly DateTime BOT = new DateTime(0);

        /// <summary>
        /// Initializes a new instance of the <see cref="UnanetClient" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="proxyOptions">The proxy options.</param>
        public UnanetClient(IUnanetOptions options, IProxyOptions proxyOptions = null)
            : base(client => new Automa(client, automa => new UnanetAutomation(client, automa, options)), proxyOptions)
        {
            Options = options;
            UnanetUri = Options.UnanetUri;
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public IUnanetOptions Options { get; }

        /// <summary>
        /// Gets the unanet URI.
        /// </summary>
        /// <value>
        /// The unanet URI.
        /// </value>
        public string UnanetUri { get; }

        #region Parse/Get

        /// <summary>
        /// Parses the client arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        protected static UnanetClient ParseClientArgs(Args args) => new UnanetClient(
            args.TryGetValue("options", out var z) ? ((JsonElement)z).GetObject<UnanetOptions>() : null);

        /// <summary>
        /// Gets the client arguments.
        /// </summary>
        /// <returns></returns>
        public override Args GetClientArgs() =>
            new Args
            {
                { "_base", base.GetClientArgs() },
                { "options", Options },
            };

        #endregion

        #region Login

        /// <summary>
        /// Ensures the access.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if XXXX, <c>false</c> otherwise.
        /// </returns>
        public override bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null)
        {
            switch (mode)
            {
                case AccessMode.Request: return ((string)value).IndexOf("<form name=\"login\"") != -1;
            }
            return false;
        }

        #endregion

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
        public async Task<string> RunReportAsync(string report, Func<string, HtmlFormPost, string> action, string executeFolder = null, Func<string, string> interceptFilename = null)
        {
            string body, url;
            // parse
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/reports/{report}/search")).ConfigureAwait(false);
                var d1 = d0.ExtractSpan("<form method=\"post\" name=\"search\"", "</form>");
                var htmlForm = new HtmlFormPost(d1);
                body = action(d1, htmlForm);
                if (body == null)
                    return null;
                url = UnanetUri + GetPostUrl(htmlForm.Action);
            }
            // download
            {
                var d0 = await this.TryFuncAsync(() => executeFolder != null
                    ? this.DownloadFileAsync(executeFolder, HttpMethod.Post, $"{url}/csv", body, interceptFilename: interceptFilename)
                    : this.DownloadDataAsync(HttpMethod.Post, url, body)).ConfigureAwait(false);
                return d0;
            }
        }

        public string GetPostUrl(string action)
        {
            if (action == null || !action.StartsWith("/roundarch/action"))
                throw new InvalidOperationException("unexpected form action returned from unanet");
            return action.Substring(17);
        }

        #endregion

        #region Parse

        /// <summary>
        /// Parses the list.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="entityPrefix">The entity prefix.</param>
        /// <returns>Dictionary&lt;System.String, Tuple&lt;System.String, System.String&gt;[]&gt;.</returns>
        public Dictionary<string, (string, string)[]> ParseList(string source, string entityPrefix)
        {
            var htmlList = source.ToHtmlDocument();
            var rows = htmlList.DocumentNode.Descendants("tr")
                .Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value.StartsWith(entityPrefix))
                .ToDictionary(
                    x => x.Attributes["id"].Value.Substring(entityPrefix.Length).Trim(),
                    x => x.Descendants("td").Select(y => new
                    {
                        klass = y.Attributes["class"]?.Value,
                        value = WebUtility.HtmlDecode(WebUtility.HtmlDecode(y.InnerText?.Trim())).Trim(),
                    })
                    .Select(y => (y.value, y.klass))
                    .ToArray()
                );
            return rows;
        }

        #endregion

        #region Get

        class AutoCompleteResult
        {
            public class DataType
            {
                public class Value
                {
                    public string Key { get; set; }
                    public string Label { get; set; }
                }
                public string Error { get; set; }
                public bool Exceeded { get; set; }
                public int Limit { get; set; }
                public Value[] Results { get; set; }
            }
            public DataType Data { get; set; }
        }

        /// <summary>
        /// Gets the automatic complete.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="matchStr">The match string.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        /// <exception cref="InvalidOperationException">Autocomplete Exceeded</exception>
        public async Task<Dictionary<string, string>> GetAutoCompleteAsync(string field, string matchStr = null, bool startsWith = true, string legalEntityKey = null)
        {
            var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/autocomplete?field={field}&matchStr={HttpUtility.UrlEncode(matchStr)}&leKey={legalEntityKey}")).ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<AutoCompleteResult>(d0)?.Data;
            if (!string.IsNullOrEmpty(data.Error))
                throw new InvalidOperationException(data.Error);
            if (data.Exceeded)
                throw new InvalidOperationException("Autocomplete Exceeded");
            var results = data.Results;
            if (matchStr != null && startsWith)
                results = results.Where(x => x.Label.StartsWith(matchStr, StringComparison.OrdinalIgnoreCase)).ToArray();
            return results.ToDictionary(x => x.Key, x => x.Label);
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <param name="menuClass">The menu class.</param>
        /// <param name="menuName">Name of the menu.</param>
        /// <param name="value">The value.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        public async Task<Dictionary<string, string>> GetOptionsAsync(string menuClass, string menuName, string value)
        {
            var htmlForm = new HtmlFormPost();
            htmlForm.Add("menuClass", "value", $"com.unanet.page.criteria.{menuClass}");
            htmlForm.Add("menuName", "value", menuName);
            htmlForm.Add("multiple", "value", "true");
            if (menuName == "account")
            {
                htmlForm.Add("account_mod", "value", "true");
                htmlForm.Add("account_acctCode_fltr", "value", value);
                htmlForm.Add("account_acctDesc_fltr", "value", string.Empty);
            }
            var body = htmlForm.ToString();
            var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/options", body)).ConfigureAwait(false);
            var htmlSelect = d0.ToHtmlDocument();
            var rows = htmlSelect.DocumentNode.Descendants("option")
                .ToDictionary(x => x.Attributes["value"].Value, x => x.Attributes["text"].Value);
            return rows;
        }

        /// <summary>
        /// Gets the entities by export.
        /// </summary>
        /// <param name="exportKey">The export key.</param>
        /// <param name="action">The action.</param>
        /// <param name="executeFolder">The execute folder.</param>
        /// <param name="interceptFilename">The intercept filename.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <returns>
        ///   <c>true</c> if XXXX, <c>false</c> otherwise.
        /// </returns>
        public async Task<(bool success, string message, bool hasFile, object tag)> GetEntitiesByExportAsync(string exportKey, Func<string, HtmlFormPost, object> action, string executeFolder, Func<string, string> interceptFilename = null, int? timeoutInSeconds = null)
        {
            string body;
            object tag;
            // parse
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/admin/export/template/criteria?xtKey={exportKey}")).ConfigureAwait(false);
                var d1 = d0.ExtractSpan("<form method=\"post\" name=\"search\"", "</form>");
                var htmlForm = new HtmlFormPost(d1);
                tag = action(d1, htmlForm);
                body = htmlForm.ToString();
            }
            // submit
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/admin/export/template/run", body, timeoutInSeconds: timeoutInSeconds ?? DownloadTimeoutInSeconds)).ConfigureAwait(false);
                var d1 = d0.ExtractSpan("<form name=\"downloadForm\"", "</form>");
                if (d1 == null)
                {
                    var reportInfos = new List<string>();
                    var idx = -1;
                    while ((idx = d0.IndexOf("<p class=\"report-info\">", idx + 1)) != -1)
                        reportInfos.Add(d0.ExtractSpanInner("<p class=\"report-info\">", "</p>", idx)?.Trim());
                    if (reportInfos.Any(x => x == "No Data Found."))
                        return (true, null, false, tag);
                    var reportError = d0.ExtractSpanInner("<div class=\"report-error\"><p>", "</p>")?.Trim();
                    var exportError = d0.ExtractSpanInner("<pre class=\"export-error\">", "</pre>")?.Trim();
                    return (false, $"no-form: {reportError}{exportError}", false, tag);
                }
                var htmlForm = new HtmlFormPost(d1);
                body = htmlForm.ToString();
            }
            // download
            {
                for (var attempt = 1; attempt <= 5; attempt++)
                    try
                    {
                        Thread.Sleep(attempt * 1000);
                        await this.TryFuncAsync(() => this.DownloadFileAsync(executeFolder, HttpMethod.Get, $"{UnanetUri}/admin/export/downloadFile", body, interceptFilename: interceptFilename)).ConfigureAwait(false);
                        return (true, null, true, tag);
                    }
                    catch (WebException e)
                    {
                        if (e.Message.Contains("500"))
                            continue;
                        throw;
                    }
                return (false, $"An error has occurred while attemping downloading file {5} times.", false, tag);
            }
        }

        /// <summary>
        /// Gets the entities by list.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entitySelect">The entity select.</param>
        /// <param name="action">The action.</param>
        /// <param name="entityPrefix">The entity prefix.</param>
        /// <returns>IEnumerable&lt;Dictionary&lt;System.String, Tuple&lt;System.String, System.String&gt;[]&gt;&gt;.</returns>
        public async Task<IEnumerable<Dictionary<string, (string, string)[]>>> GetEntitiesByListAsync(string entity, string entitySelect, Action<string, HtmlFormPost> action = null, string entityPrefix = "r")
        {
            string body = null;
            // parse
            if (entitySelect == null)
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/{entity}/list")).ConfigureAwait(false);
                var d1 = d0.ExtractSpan("<form name=\"search\" method=\"post\"", "</form>");
                var htmlForm = new HtmlFormPost(d1);
                action(d1, htmlForm);
                body = htmlForm.ToString();
            }
            // submit
            {
                var d0 = await this.TryFuncAsync(() => entitySelect == null ?
                    this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}/list", body) :
                    this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}/list?{entitySelect}")).ConfigureAwait(false);
                var startIdx = 0;
                var list = new List<Dictionary<string, (string, string)[]>>();
                while (true)
                {
                    startIdx = d0.IndexOf("<table class=\"list\"", startIdx);
                    if (startIdx == -1)
                        break;
                    var d1 = d0.ExtractSpan("<table class=\"list\"", "</table>", startIdx++);
                    list.Add(ParseList(d1, entityPrefix));
                }
                return list;
            }
        }

        /// <summary>
        /// Gets the entities by sub list.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entitySelect">The entity select.</param>
        /// <param name="entityPrefix">The entity prefix.</param>
        /// <returns>IEnumerable&lt;Dictionary&lt;System.String, Tuple&lt;System.String, System.String&gt;[]&gt;&gt;.</returns>
        public async Task<IEnumerable<Dictionary<string, (string, string)[]>>> GetEntitiesBySubListAsync(string entity, string entitySelect, string entityPrefix = "k_")
        {
            // submit
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/{entity}/list?{entitySelect}")).ConfigureAwait(false);
                var startIdx = 0;
                var list = new List<Dictionary<string, (string, string)[]>>();
                while (true)
                {
                    startIdx = d0.IndexOf("<table class=\"list\"", startIdx);
                    if (startIdx == -1)
                        break;
                    var d1 = d0.ExtractSpan("<table class=\"list\"", "</table>", startIdx++);
                    list.Add(ParseList(d1, entityPrefix));
                }
                return list;
            }
        }

        #endregion

        #region Put

        /// <summary>
        /// Puts the entities by import asynchronous.
        /// </summary>
        /// <param name="importType">Type of the import.</param>
        /// <param name="action">The action.</param>
        /// <param name="executeFolder">The execute folder.</param>
        /// <param name="interceptFilename">The intercept filename.</param>
        /// <returns></returns>
        public async Task<string> PutEntitiesByImportAsync(string importType, Action<HtmlFormPost> action, string executeFolder, Func<string, string> interceptFilename = null)
        {
            HttpContent body; string url;
            // parse
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/admin/import?importType={importType}")).ConfigureAwait(false);
                var d1 = d0.ExtractSpan("<form method=\"post\" ", "</form>");
                var htmlForm = new HtmlFormPost(d1);
                action(htmlForm);
                body = htmlForm.ToContent();
                url = $"{UnanetUri}/admin/import/{importType}";
            }
            // submit
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Post, url, body)).ConfigureAwait(false);
                return d0?.ExtractSpanInner("<p class=\"error\">", "</p>")?.Trim();
            }
        }

        #endregion

        #region Manage

        /// <summary>
        /// Posts the value asynchronous.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="entitySelect">The entity select.</param>
        /// <param name="parentSelect">The parent select.</param>
        /// <param name="useSafeRead">if set to <c>true</c> [use safe read].</param>
        /// <returns></returns>
        public async Task<(string value, string last)> PostValueAsync(HttpMethod method, string entity, string entitySelect, string parentSelect, bool useSafeRead = false)
        {
            var d0 = await this.TryFuncAsync(() => method == HttpMethod.Get ?
                this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/{entity}?{parentSelect}", useSafeRead: useSafeRead) :
                this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}?{parentSelect}", entitySelect, useSafeRead: useSafeRead)).ConfigureAwait(false);
            return (d0, null);
        }

        /// <summary>
        /// Submits the manage asynchronous.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="entitySelect">The entity select.</param>
        /// <param name="action">The action.</param>
        /// <param name="marker">The marker.</param>
        /// <param name="valueFunc">The value function.</param>
        /// <returns></returns>
        public async Task<(object value, string last)> SubmitManageAsync(HttpMethod method, string entity, string entitySelect, Action<string, HtmlFormPost> action = null, string marker = null, Func<string, object> valueFunc = null)
        {
            string body, last = null, url = null;
            // parse
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/{entity}/{(method == HttpMethod.Post ? "add" : "edit?" + entitySelect)}")).ConfigureAwait(false);
                var htmlForm = new HtmlFormPost(d0);
                try { action(d0, htmlForm); }
                catch (Exception e) { last = e.Message; throw; }
                body = htmlForm.ToString();
                if (body == null)
                    return (null, last);
                url = UnanetUri + GetPostUrl(htmlForm.Action);
            }
            // submit
            {
                var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Post, url, body)).ConfigureAwait(false);
                var d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
                return d1 != null
                    ? (null, d1.ExtractSpanInner("<ul class=\"error\">", "</ul>"))
                    : (valueFunc?.Invoke(d0) ?? this, d0.ExtractSpanInner("<td class=\"right label\" style=\"padding-bottom:10px;\">", "</td>")?.Trim());
            }
        }

        /// <summary>
        /// Submits the sub manage.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="entitySelect">The entity select.</param>
        /// <param name="parentSelect">The parent select.</param>
        /// <param name="defaults">The defaults.</param>
        /// <param name="func">The function.</param>
        /// <param name="valueFunc">The value function.</param>
        /// <param name="formOptions">The form options.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">type</exception>
        public async Task<(object value, string last)> SubmitSubManageAsync(string type, HttpMethod method, string entity, string entitySelect, string parentSelect, string defaults, Func<string, HtmlFormPost, string> func = null, Func<string, object> valueFunc = null, HtmlFormOptions formOptions = null)
        {
            string body, last = null, url = null, d0 = null;
            // parse
            if (method != HttpMethod.Delete)
            {
                switch (type)
                {
                    case "0":
                        d0 = await this.TryFuncAsync(() => method == HttpMethod.Get ?
                          this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/{entity}?{parentSelect}") :
                          this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}?{parentSelect}", entitySelect)).ConfigureAwait(false); break;
                    case "A":
                        d0 = await this.TryFuncAsync(() => method == HttpMethod.Post ?
                          this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/{entity}/add?{parentSelect}") :
                          this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}/edit?{parentSelect}", entitySelect)).ConfigureAwait(false); break;
                    case "B":
                        d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}/edit", $"{entitySelect}&restore=true&isCopy=false&editAll=false&{parentSelect}")).ConfigureAwait(false); break;
                    case "C":
                        d0 = await this.TryFuncAsync(() => method == HttpMethod.Post ?
                          this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/{entity}/list?{parentSelect}") :
                          this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}/list", $"{entitySelect}&addNext=false&edit=true&copy=false&nextKey=&{parentSelect}&{defaults}")).ConfigureAwait(false); break;
                    case "D":
                        d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(method, $"{UnanetUri}/{entity}?{parentSelect}")).ConfigureAwait(false); break;
                    case "E":
                        d0 = await this.TryFuncAsync(() => method == HttpMethod.Put ?
                          this.DownloadDataAsync(HttpMethod.Get, $"{UnanetUri}/{entity}/add?{parentSelect}") :
                          this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}/add", parentSelect)).ConfigureAwait(false); break;
                    case "F":
                        d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}", $"{parentSelect}&{defaults}")).ConfigureAwait(false); break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type);
                }
                var htmlForm = new HtmlFormPost(d0, formOptions: formOptions);
                try { body = func(d0, htmlForm); }
                catch (Exception e) { last = e.Message; throw; }
                if (body == null)
                    return (null, last);
                url = UnanetUri + GetPostUrl(htmlForm.Action);
            }
            else
            {
                url = $"{UnanetUri}/{entity}/delete?{parentSelect}";
                switch (type)
                {
                    case "A": body = $"{entitySelect}"; break;
                    case "C": body = $"{entitySelect}&addNext=false&edit=false&copy=false&nextKey={entitySelect}&{parentSelect}&{defaults}"; break;
                    case "D": body = $"{entitySelect}"; break;
                    case "E": body = $"{entitySelect}"; break;
                    case "F":
                        d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Post, $"{UnanetUri}/{entity}", $"{parentSelect}&{defaults}")).ConfigureAwait(false);
                        body = func(d0, null);
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type);
                }
            }
            // submit
            {
                d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Post, url, body)).ConfigureAwait(false);
                var d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
                return d1 != null
                    ? (null, d1.ExtractSpanInner("<ul class=\"error\">", "</ul>"))
                    : (valueFunc?.Invoke(d0) ?? this, d0.ExtractSpanInner("<td class=\"right label\" style=\"padding-bottom:10px;\">", "</td>")?.Trim());
            }
        }

        #endregion
    }
}
