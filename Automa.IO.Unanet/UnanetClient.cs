using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Automa.IO.Unanet
{
    /// <summary>
    /// UnanetClient
    /// </summary>
    public partial class UnanetClient : AutomaClient
    {
        public static readonly DateTime BOT = new DateTime(0);

        string UnanetUri => !_sandbox ? $"https://{_unanetId}.unanet.biz/{_unanetId}/action" : $"https://{_unanetId}-sand.unanet.biz/{_unanetId}-sand/action";
        public IDictionary<string, Tuple<string, string>> Exports => !_sandbox ? Config.Exports : Config.ExportsSandbox;
        public IDictionary<string, string> Imports => Config.Imports;

        readonly string _unanetId;
        readonly bool _sandbox;

        public UnanetClient(string unanetId, bool sandbox)
            : base(x => new Automa(x, (ctx, driver) => new UnanetAutomation(x, ctx, driver, unanetId, sandbox)))
        {
            _unanetId = unanetId;
            _sandbox = sandbox;
        }

        public override bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null)
        {
            switch (mode)
            {
                case AccessMode.Request: return ((string)value).IndexOf("<form name=\"login\"") != -1;
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
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{UnanetUri}/reports/{report}/search"));
                var d1 = d0.ExtractSpan("<form method=\"post\" name=\"search\"", "</form>");
                var htmlForm = new HtmlFormPost(d1);
                action(htmlForm);
                body = htmlForm.ToString();
                if (!htmlForm.Action.StartsWith("/roundarch/action"))
                    throw new InvalidOperationException("unexpected form action returned from unanet");
                url = UnanetUri + htmlForm.Action.Substring(17);
            }
            // download
            {
                var d0 = this.TryFunc(() => this.DownloadFile(executeFolder, HttpMethod.Post, $"{url}/csv", body, interceptFilename: interceptFilename));
                return true;
            }
        }

        #endregion

        #region Parse

        /// <summary>
        /// Parses the list.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="entityPrefix">The entity prefix.</param>
        /// <returns>Dictionary&lt;System.String, Tuple&lt;System.String, System.String&gt;[]&gt;.</returns>
        public Dictionary<string, Tuple<string, string>[]> ParseList(string source, string entityPrefix)
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
                    .Select(y => new Tuple<string, string>(y.value, y.klass))
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
                public class Value { public string Key; public string Label; }
                public string Error;
                public bool Exceeded;
                public int Limit;
                public Value[] Results;
            }
            public DataType Data;
        }

        /// <summary>
        /// Gets the automatic complete.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="matchStr">The match string.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        /// <exception cref="InvalidOperationException">Autocomplete Exceeded</exception>
        public Dictionary<string, string> GetAutoComplete(string field, string matchStr = null, bool startsWith = true, string legalEntityKey = null)
        {
            var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{UnanetUri}/autocomplete?field={field}&matchStr={HttpUtility.UrlEncode(matchStr)}&leKey={legalEntityKey}"));
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
        public Dictionary<string, string> GetOptions(string menuClass, string menuName, string value)
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
            var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Post, $"{UnanetUri}/options", body));
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
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool GetEntitiesByExport(string exportKey, Action<HtmlFormPost> action, string executeFolder, Func<string, string> interceptFilename = null)
        {
            string body;
            // parse
            {
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{UnanetUri}/admin/export/template/criteria?xtKey={exportKey}"));
                var d1 = d0.ExtractSpan("<form method=\"post\" name=\"search\"", "</form>");
                var htmlForm = new HtmlFormPost(d1);
                action(htmlForm);
                body = htmlForm.ToString();
            }
            // submit
            {
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Post, $"{UnanetUri}/admin/export/template/run", body));
                var d1 = d0.ExtractSpan("<form name=\"downloadForm\"", "</form>");
                if (d1 == null)
                {
                    var reportError = d0.ExtractSpanInner("<div class=\"report-error\"><p>", "</p>")?.Trim();
                    var exportError = d0.ExtractSpanInner("<pre class=\"export-error\">", "</pre>")?.Trim();
                    return false;
                }
                var htmlForm = new HtmlFormPost(d1);
                body = htmlForm.ToString();
            }
            // download
            {
                var d0 = this.TryFunc(() => this.DownloadFile(executeFolder, HttpMethod.Get, $"{UnanetUri}/admin/export/downloadFile", body, interceptFilename: interceptFilename));
                return true;
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
        public IEnumerable<Dictionary<string, Tuple<string, string>[]>> GetEntitiesByList(string entity, string entitySelect, Action<HtmlFormPost> action = null, string entityPrefix = "r")
        {
            string body = null;
            // parse
            if (entitySelect == null)
            {
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{UnanetUri}/{entity}/list"));
                var d1 = d0.ExtractSpan("<form name=\"search\" method=\"post\"", "</form>");
                var htmlForm = new HtmlFormPost(d1);
                action(htmlForm);
                body = htmlForm.ToString();
            }
            // submit
            {
                var d0 = this.TryFunc(() => entitySelect == null ?
                    this.DownloadData(HttpMethod.Post, $"{UnanetUri}/{entity}/list", body) :
                    this.DownloadData(HttpMethod.Post, $"{UnanetUri}/{entity}/list?{entitySelect}"));
                var startIdx = 0;
                while (true)
                {
                    startIdx = d0.IndexOf("<table class=\"list\"", startIdx);
                    if (startIdx == -1)
                        yield break;
                    var d1 = d0.ExtractSpan("<table class=\"list\"", "</table>", startIdx++);
                    yield return ParseList(d1, entityPrefix);
                }
            }
        }

        /// <summary>
        /// Gets the entities by sub list.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entitySelect">The entity select.</param>
        /// <param name="entityPrefix">The entity prefix.</param>
        /// <returns>IEnumerable&lt;Dictionary&lt;System.String, Tuple&lt;System.String, System.String&gt;[]&gt;&gt;.</returns>
        public IEnumerable<Dictionary<string, Tuple<string, string>[]>> GetEntitiesBySubList(string entity, string entitySelect, string entityPrefix = "k_")
        {
            // submit
            {
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{UnanetUri}/{entity}/list?{entitySelect}"));
                var startIdx = 0;
                while (true)
                {
                    startIdx = d0.IndexOf("<table class=\"list\"", startIdx);
                    if (startIdx == -1)
                        yield break;
                    var d1 = d0.ExtractSpan("<table class=\"list\"", "</table>", startIdx++);
                    yield return ParseList(d1, entityPrefix);
                }
            }
        }

        #endregion

        #region Put

        public string PutEntitiesByImport(string importType, Action<HtmlFormPost> action, string executeFolder, Func<string, string> interceptFilename = null)
        {
            HttpContent body; string url;
            // parse
            {
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{UnanetUri}/admin/import?importType={importType}"));
                var d1 = d0.ExtractSpan("<form method=\"post\" ", "</form>");
                var htmlForm = new HtmlFormPost(d1);
                action(htmlForm);
                body = htmlForm.ToContent();
                url = $"{UnanetUri}/admin/import/{importType}";
            }
            // submit
            {
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Post, url, body));
                return d0 != null ? d0.ExtractSpanInner("<p class=\"error\">", "</p>")?.Trim() : null;
            }
        }

        #endregion

        #region Manage

        public object SubmitManage(HttpMethod method, string entity, string entitySelect, out string last, Action<string, HtmlFormPost> action = null, string marker = null, Func<string, object> valueFunc = null)
        {
            string body, url = null;
            // parse
            {
                var d0 = this.TryFunc(() =>
                    this.DownloadData(HttpMethod.Get, $"{UnanetUri}/{entity}/{(method == HttpMethod.Post ? "add" : "edit?" + entitySelect)}"));
                var htmlForm = new HtmlFormPost(d0);
                try { action(d0, htmlForm); }
                catch (Exception e) { last = e.Message; throw; }
                body = htmlForm.ToString();
                if (htmlForm.Action == null || !htmlForm.Action.StartsWith("/roundarch/action"))
                    throw new InvalidOperationException("unexpected form action returned from unanet");
                url = UnanetUri + htmlForm.Action.Substring(17);
            }
            // submit
            {
                var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Post, url, body));
                var d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
                if (d1 != null)
                {
                    last = d1.ExtractSpanInner("<ul class=\"error\">", "</ul>");
                    return null;
                }
                last = d0.ExtractSpanInner("<td class=\"right label\" style=\"padding-bottom:10px;\">", "</td>")?.Trim();
                return valueFunc?.Invoke(d0) ?? this;
            }
        }

        public object SubmitSubManage(string type, HttpMethod method, string entity, string entitySelect, string parentSelect, string defaults, out string last, Func<string, HtmlFormPost, string> func = null, Func<string, object> valueFunc = null, HtmlFormSettings formSettings = null)
        {
            string body, url = null, d0 = null;
            // parse
            if (method != HttpMethod.Delete)
            {
                switch (type)
                {
                    case "A":
                        d0 = this.TryFunc(() => method == HttpMethod.Post ?
                            this.DownloadData(HttpMethod.Get, $"{UnanetUri}/{entity}/add?{parentSelect}") :
                            this.DownloadData(HttpMethod.Post, $"{UnanetUri}/{entity}/edit?{parentSelect}", $"{entitySelect}"));
                        break;
                    case "B":
                        d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Post, $"{UnanetUri}/{entity}/edit", $"{entitySelect}&restore=true&isCopy=false&editAll=false&{parentSelect}"));
                        break;
                    case "C":
                        d0 = this.TryFunc(() => method == HttpMethod.Post ?
                            this.DownloadData(HttpMethod.Get, $"{UnanetUri}/{entity}/list?{parentSelect}") :
                            this.DownloadData(HttpMethod.Post, $"{UnanetUri}/{entity}/list", $"{entitySelect}&addNext=false&edit=true&copy=false&nextKey=&{parentSelect}&{defaults}"));
                        break;
                    case "D": d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, $"{UnanetUri}/{entity}?{parentSelect}")); break;
                    case "E":
                        d0 = this.TryFunc(() => method == HttpMethod.Put ?
                            this.DownloadData(HttpMethod.Get, $"{UnanetUri}/{entity}/add?{parentSelect}") :
                            this.DownloadData(HttpMethod.Post, $"{UnanetUri}/{entity}/add", parentSelect)); break;
                    case "F": d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Post, $"{UnanetUri}/{entity}", $"{parentSelect}&{defaults}")); break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type);
                }
                var htmlForm = new HtmlFormPost(d0, formOptions: formSettings);
                try { body = func(d0, htmlForm); }
                catch (Exception e) { last = e.Message; throw; }
                if (htmlForm.Action == null || !htmlForm.Action.StartsWith("/roundarch/action"))
                    throw new InvalidOperationException("unexpected form action returned from unanet");
                url = UnanetUri + htmlForm.Action.Substring(17);
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
                        d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Post, $"{UnanetUri}/{entity}", $"{parentSelect}&{defaults}"));
                        body = func(d0, null);
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type);
                }
            }
            // submit
            {
                d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Post, url, body));
                var d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
                if (d1 != null)
                {
                    last = d1.ExtractSpanInner("<ul class=\"error\">", "</ul>");
                    return null;
                }
                last = d0.ExtractSpanInner("<td class=\"right label\" style=\"padding-bottom:10px;\">", "</td>")?.Trim();
                return valueFunc?.Invoke(d0) ?? this;
            }
        }

        #endregion
    }
}
