using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace Automa.IO.Unanet.Records
{
    public class ApprovalModel : ModelBase
    {
        #region Classes

        public class Grid
        {
            public string Label { get; set; }
            public int Key { get; set; }
            public string Name { get; set; }
            public string PersonName { get; set; }
            public Dictionary<(string key, string keyType), GridRow> Rows { get; set; }
        }

        public class GridRow
        {
            public string Label { get; set; }
            public string Key { get; set; }
            public string KeyType { get; set; }
            public string Name { get; set; }
            public string PersonName { get; set; }
            public DateTime Week { get; set; }
            public decimal? Hours { get; set; }
            public string Status { get; set; }
            public DateTime StatusDate { get; set; }
            public string Comments { get; set; }
        }

        #endregion

        public static bool ManagerApprovalByKey(UnanetClient una, (string key, string keyType) key, string comments, string personName, out string last) => Approve(una, "people", "key", key, comments, personName, out last);
        public static bool ManagerApprovalByKeyMatch(UnanetClient una, (string key, string keyType) key, string comments, string personName, out string last) => Approve(una, "people", "keyMatch", key, comments, personName, out last);
        public static bool ProjectApprovalByKey(UnanetClient una, (string key, string keyType) key, string comments, string personName, out string last) => Approve(una, "projects", "key", key, comments, personName, out last);
        public static bool ProjectApprovalByKeyMatch(UnanetClient una, (string key, string keyType) key, string comments, string personName, out string last) => Approve(una, "projects", "keyMatch", key, comments, personName, out last);
        public static bool CustomerApprovalByKey(UnanetClient una, (string key, string keyType) key, string comments, string personName, out string last) => Approve(una, "customers", "key", key, comments, personName, out last);
        public static bool CustomerApprovalByKeyMatch(UnanetClient una, (string key, string keyType) key, string comments, string personName, out string last) => Approve(una, "customers", "keyMatch", key, comments, personName, out last);
        public static bool Approve(UnanetClient una, string type, string method, (string key, string keyType)? key, string comments, string personName, out string last)
        {
            var found = FindApproval(una, type, personName, out last);
            if (last != null || found.grid.Rows == null)
                return false;
            GridRow row;
            switch (method)
            {
                case "all":
                    foreach (var row0 in found.grid.Rows.Values)
                        Approve(una, type, found, row0, comments, out last);
                    return true;
                case "first":
                    row = found.grid.Rows.FirstOrDefault().Value;
                    if (row == null)
                        return false;
                    Approve(una, type, found, row, comments, out last);
                    return true;
                case "firstTime":
                    row = found.grid.Rows.FirstOrDefault(x => x.Key.keyType == "Time").Value;
                    if (row == null)
                        return false;
                    Approve(una, type, found, row, comments, out last);
                    return true;
                case "key":
                    if (!found.grid.Rows.TryGetValue(key.Value, out row))
                    {
                        last = $"Unable to find {key} for {personName}";
                        return false;
                    }
                    Approve(una, type, found, row, comments, out last);
                    return true;
                case "keyMatch":
                    foreach (var row0 in found.grid.Rows.Values.Where(x => x.Key.EndsWith($",{key.Value.key}") && x.KeyType == key.Value.keyType))
                        Approve(una, type, found, row0, comments, out last);
                    return true;
                default: throw new ArgumentOutOfRangeException(nameof(method), method);
            }
        }

        public static void ApproveSheet(UnanetClient una, (string key, string keyType) key, string comments, string[] projApprs, string[] managers, string[] customers, out string last)
        {
            last = null;
            if (projApprs != null)
                foreach (var personName in projApprs)
                    ProjectApprovalByKeyMatch(una, key, comments, personName, out last);
            if (managers != null)
                foreach (var personName in managers)
                {
                    // delay to propagate
                    Thread.Sleep(100);
                    ManagerApprovalByKeyMatch(una, key, comments, personName, out last);
                }
            if (customers != null)
                foreach (var personName in customers)
                    CustomerApprovalByKeyMatch(una, key, comments, personName, out last);
        }

        public static (Grid grid, HtmlFormPost form) FindApproval(UnanetClient una, string type, string personName, out string last)
        {
            last = null;
            string prefix;
            switch (type)
            {
                case "people": prefix = "queueMgrAlt"; break;
                case "projects": prefix = "queuePmAlt"; break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type);
            }
            try
            {
                var d0 = una.PostValue(HttpMethod.Get, $"{type}/approvals/alternate", null, null, out last);
                if (d0.Contains("Unauthorized"))
                    throw new InvalidOperationException($"'{type}/approvals' was not authorized, please contact the administrator");
                var items = ParseApproval(d0, prefix);
                var found = items.TryGetValue(personName, out var item) ? item
                    : personName == "first" ? items.Where(x => x.Key != "Benson, Tim").First().Value
                    : null;
                if (found == null)
                    return (null, null);
                // first post
                var f = new HtmlFormPost(d0);
                var key = found.Key;
                f.Values["scrollToLabel"] = $"label_{prefix}_{key}";
                f.Values[prefix] = "true";
                f.Add($"{prefix}_{key}", "value", "true");
                f.Add($"{prefix}_{key}_Time", "value", "true");
                f.Add($"{prefix}_{key}_Leave", "value", "true");
                f.Add($"{prefix}_{key}_Expense", "value", "true");
                f.Add($"{prefix}_{key}_ExpenseRequest", "value", "true");
                f.Add($"{prefix}_{key}_PR", "value", "true");
                f.Add($"{prefix}_{key}_PO", "value", "true");
                f.Add($"{prefix}_{key}_VI", "value", "true");
                f.Add($"{prefix}_{key}_POVI", "value", "true");
                var body = f.ToString();
                var url = una.GetPostUrl(f.Action);
                d0 = una.PostValue(HttpMethod.Post, url, body, null, out last);
                // second post
                f = new HtmlFormPost(d0);
                url = una.GetPostUrl(f.Action);
                d0 = una.PostValue(HttpMethod.Post, url, body, null, out last);
                items = ParseApproval(d0, prefix);
                found = items.TryGetValue(personName, out item) ? item
                    : personName == "first" ? items.Where(x => x.Key != "Benson, Tim").First().Value
                    : null;
                return (found, f);
            }
            catch (Exception e) { last = e.Message; }
            return (null, null);
        }

        public static IDictionary<string, Grid> ParseApproval(string source, string prefix)
        {
            source = source.ExtractSpan("<form", "</form>");
            var doc = source.ToHtmlDocument();
            var divs = doc.DocumentNode.SelectNodes($"//div[@id='{prefix}']/div").ToArray();
            var r = divs.Where((e, i) => i % 2 == 0).Select((a, b) => new { a, b = divs[b * 2 + 1].InnerHtml.ToHtmlDocument().DocumentNode })
                .Select(x => new Grid
                {
                    Label = x.a.Attributes["id"].Value.Substring(6),
                    Key = int.Parse(x.a.Attributes["id"].Value.Substring(prefix.Length + 7).Replace("-a", "")),
                    Name = x.a.InnerText.Remove(x.a.InnerText.IndexOf("(")).Trim(),
                    PersonName = x.a.InnerText.ExtractSpanInner("(", ")").ToUpperInvariant(),
                    Rows = x.b.SelectNodes($"//tr[starts-with(@id,'{prefix}')]")?.Select(y =>
                    {
                        var ats = y.Attributes["id"].Value.Substring(prefix.Length + 1).Split('_');
                        var tds = y.Descendants("td").ToArray();
                        return new GridRow
                        {
                            Label = y.Attributes["id"].Value,
                            Key = ats[2],
                            KeyType = ats[1],
                            Name = tds[3].InnerText.Remove(tds[3].InnerText.IndexOf("(")).Trim(),
                            PersonName = tds[3].InnerText.ExtractSpanInner("(", ")").ToUpperInvariant(),
                            Week = DateTime.Parse(tds[4].InnerText.Remove(tds[4].InnerText.IndexOf("&"))),
                            Hours = decimal.Parse(tds[5].InnerText),
                            Status = tds[6].InnerText,
                            StatusDate = DateTime.Parse(tds[7].InnerText),
                            Comments = tds.Length > 8 ? tds[8].InnerText.Replace("&nbsp;", " ").Trim() : string.Empty,
                        };
                    }).ToDictionary(z => (z.Key, z.KeyType)),
                }).ToDictionary(x => x.Name);
            return r;
        }

        public static void Approve(UnanetClient una, string type, (Grid grid, HtmlFormPost form) found, GridRow row, string comments, out string last)
        {
            last = null;
            string approvalType;
            switch (type)
            {
                case "people": approvalType = "MANAGER"; break;
                case "projects": approvalType = "PROJECT"; break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type);
            }
            try
            {
                var f = found.form;
                if (found.grid.Rows != null)
                    foreach (var group in found.grid.Rows.Keys.GroupBy(x => x.keyType))
                        f.Add($"keys_{found.grid.Label}_{group.Key}", "text", $"{string.Join(";", group.Select(x => x.key).ToArray())};");
                f.Remove("displaySet");
                f.Values["approvalType"] = approvalType;
                f.Values["fromQueue"] = "true";
                f.Values["referralURL"] = $"/{type}/approvals/refresh";
                string url;
                switch (row.KeyType)
                {
                    case "Leave":
                        url = "approve/leave";
                        f.Values["leaveKey"] = row.Key;
                        break;
                    case "Time":
                        url = "approve/time";
                        var keys = row.Key.Split(',');
                        f.Values["projectkey"] = keys[0];
                        f.Values["timesheetkey"] = keys[1];
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(row.KeyType), row.KeyType);
                }
                f.Values["scrollToLabel"] = row.Label;
                f.Add("comments", "text", comments);
                var body = f.ToString();
                var d0 = una.PostValue(HttpMethod.Post, url, body, null, out last);
            }
            catch (Exception e) { last = e.Message; }
        }
    }
}