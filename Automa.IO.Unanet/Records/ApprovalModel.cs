using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        }

        public class GridExpenseRow : GridRow
        {
            public string SomeId { get; set; }
            public decimal? Amount { get; set; }
            public string Expense { get; set; }
            public string Comments { get; set; }
        }

        public class GridTimeRow : GridRow
        {
            public DateTime Week { get; set; }
            public decimal? Hours { get; set; }
            public string Status { get; set; }
            public DateTime StatusDate { get; set; }
            public string Comments { get; set; }
        }

        #endregion

        public static Task<(bool value, string last)> ManagerApprovalByKeyAsync(UnanetClient una, (string key, string keyType) key, string comments, string personName) => ApproveAsync(una, "people", "key", key, comments, personName);
        public static Task<(bool value, string last)> ManagerApprovalByKeyMatchAsync(UnanetClient una, (string key, string keyType) key, string comments, string personName) => ApproveAsync(una, "people", "keyMatch", key, comments, personName);
        public static Task<(bool value, string last)> ProjectApprovalByKeyAsync(UnanetClient una, (string key, string keyType) key, string comments, string personName) => ApproveAsync(una, "projects", "key", key, comments, personName);
        public static Task<(bool value, string last)> ProjectApprovalByKeyMatchAsync(UnanetClient una, (string key, string keyType) key, string comments, string personName) => ApproveAsync(una, "projects", "keyMatch", key, comments, personName);
        public static Task<(bool value, string last)> CustomerApprovalByKeyAsync(UnanetClient una, (string key, string keyType) key, string comments, string personName) => ApproveAsync(una, "customers", "key", key, comments, personName);
        public static Task<(bool value, string last)> CustomerApprovalByKeyMatchAsync(UnanetClient una, (string key, string keyType) key, string comments, string personName) => ApproveAsync(una, "customers", "keyMatch", key, comments, personName);
        public static async Task<(bool value, string last)> ApproveAsync(UnanetClient una, string type, string method, (string key, string keyType)? key, string comments, string personName)
        {
            var (grid, form, last) = await FindApprovalAsync(una, type, personName).ConfigureAwait(false);
            if (last != null || grid?.Rows == null)
                return (false, last);
            GridRow row;
            switch (method)
            {
                case "all":
                    foreach (var row0 in grid.Rows.Values)
                        last = await ApproveAsync(una, type, (grid, form), row0, comments).ConfigureAwait(false);
                    return (true, last);
                case "first":
                    row = grid.Rows.FirstOrDefault().Value;
                    if (row == null)
                        return (false, last);
                    last = await ApproveAsync(una, type, (grid, form), row, comments).ConfigureAwait(false);
                    return (true, last);
                case "firstTime":
                    row = grid.Rows.FirstOrDefault(x => x.Key.keyType == "Time").Value;
                    if (row == null)
                        return (false, last);
                    last = await ApproveAsync(una, type, (grid, form), row, comments).ConfigureAwait(false);
                    return (true, last);
                case "key":
                    if (!grid.Rows.TryGetValue(key.Value, out row))
                    {
                        last = $"Unable to find {key} for {personName}";
                        return (false, last);
                    }
                    last = await ApproveAsync(una, type, (grid, form), row, comments).ConfigureAwait(false);
                    return (true, last);
                case "keyMatch":
                    foreach (var row0 in grid.Rows.Values.Where(x => x.Key.EndsWith($",{key.Value.key}") && x.KeyType == key.Value.keyType))
                        last = await ApproveAsync(una, type, (grid, form), row0, comments).ConfigureAwait(false);
                    return (true, last);
                default: throw new ArgumentOutOfRangeException(nameof(method), method);
            }
        }

        public static async Task<(bool value, string last)> ApproveSheetAsync(UnanetClient una, (string key, string keyType) key, string comments, string[] projApprs, string[] managers, string[] customers)
        {
            bool value, changed = false; string last = null;
            if (projApprs != null)
                foreach (var personName in projApprs)
                {
                    (value, last) = await ProjectApprovalByKeyMatchAsync(una, key, comments, personName).ConfigureAwait(false);
                    changed |= value;
                }
            if (managers != null)
                foreach (var personName in managers)
                {
                    Thread.Sleep(100); // delay to propagate
                    (value, last) = await ManagerApprovalByKeyMatchAsync(una, key, comments, personName).ConfigureAwait(false);
                    changed |= value;
                }
            if (customers != null)
                foreach (var personName in customers)
                {
                    (value, last) = await CustomerApprovalByKeyMatchAsync(una, key, comments, personName).ConfigureAwait(false);
                    changed |= value;
                }
            return (changed, last);
        }

        public static async Task<(Grid grid, HtmlFormPost form, string last)> FindApprovalAsync(UnanetClient una, string type, string personName)
        {
            string prefix;
            switch (type)
            {
                case "people": prefix = "queueMgrAlt"; break;
                case "projects": prefix = "queuePmAlt"; break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type);
            }
            try
            {
                var (d0, last) = await una.PostValueAsync(HttpMethod.Get, $"{type}/approvals/alternate", null, null).ConfigureAwait(false);
                if (d0.Contains("Unauthorized"))
                    throw new InvalidOperationException($"'{type}/approvals' was not authorized, please contact the administrator");
                var items = ParseApproval(d0, prefix);
                var found = items.TryGetValue(personName, out var z) ? z
                    : personName == "first" ? items.Where(x => x.Key != "Benson, Tim").First().Value
                    : null;
                if (found == null)
                    return (null, null, last);
                // first post
                var f = new HtmlFormPost(d0);
                var key = found.Key;
                f.Values["scrollToLabel"] = $"label_{prefix}_{key}";
                f.Values[prefix] = "true";
                f.Add($"{prefix}_{key}", "value", "true");
                f.Add($"{prefix}_{key}_Time", "value", "true");
                f.Add($"{prefix}_{key}_Leave", "value", "false");
                f.Add($"{prefix}_{key}_Expense", "value", "false");
                f.Add($"{prefix}_{key}_ExpenseRequest", "value", "false");
                f.Add($"{prefix}_{key}_PR", "value", "false");
                f.Add($"{prefix}_{key}_PO", "value", "false");
                f.Add($"{prefix}_{key}_VI", "value", "false");
                f.Add($"{prefix}_{key}_POVI", "value", "false");
                var body = f.ToString();
                var url = una.GetPostUrl(f.Action);
                (d0, last) = await una.PostValueAsync(HttpMethod.Post, url, body, null).ConfigureAwait(false);
                // second post
                f = new HtmlFormPost(d0);
                url = una.GetPostUrl(f.Action);
                (d0, last) = await una.PostValueAsync(HttpMethod.Post, url, body, null).ConfigureAwait(false);
                items = ParseApproval(d0, prefix);
                found = items.TryGetValue(personName, out z) ? z
                    : personName == "first" ? items.Where(x => x.Key != "Benson, Tim").First().Value
                    : null;
                return (found, f, last);
            }
            catch (Exception e) { return (null, null, e.Message); }
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
                    Key = int.Parse(x.a.Attributes["id"].Value.Substring(prefix.Length + 7).Replace("-a", string.Empty)),
                    Name = x.a.InnerText.Remove(x.a.InnerText.IndexOf("(")).Trim(),
                    PersonName = x.a.InnerText.ExtractSpanInner("(", ")").ToUpperInvariant(),
                    Rows = x.b.SelectNodes($"//tr[starts-with(@id,'{prefix}')]")?.Select(y =>
                    {
                        var ats = y.Attributes["id"].Value.Substring(prefix.Length + 1).Split('_');
                        var tds = y.Descendants("td").ToArray();
                        switch (ats[1])
                        {
                            case "Expense":
                                return (GridRow)new GridExpenseRow
                                {
                                    Label = y.Attributes["id"].Value,
                                    Key = ats[2],
                                    KeyType = ats[1],
                                    Name = tds[4].InnerText.Remove(tds[4].InnerText.IndexOf("(")).Trim(),
                                    PersonName = tds[4].InnerText.ExtractSpanInner("(", ")").ToUpperInvariant(),
                                    //
                                    SomeId = tds[5].InnerText,
                                    Amount = decimal.Parse(tds[6].InnerText.Replace("$", string.Empty)),
                                    Expense = tds[7].InnerText,
                                    Comments = tds.Length > 8 ? tds[8].InnerText.Replace("&nbsp;", " ").Trim() : string.Empty,
                                };
                            case "Time":
                                return (GridRow)new GridTimeRow
                                {
                                    Label = y.Attributes["id"].Value,
                                    Key = ats[2],
                                    KeyType = ats[1],
                                    Name = tds[3].InnerText.Remove(tds[3].InnerText.IndexOf("(")).Trim(),
                                    PersonName = tds[3].InnerText.ExtractSpanInner("(", ")").ToUpperInvariant(),
                                    //
                                    Week = DateTime.Parse(tds[4].InnerText.Remove(tds[4].InnerText.IndexOf("&"))),
                                    Hours = decimal.Parse(tds[5].InnerText),
                                    Status = tds[6].InnerText,
                                    StatusDate = DateTime.Parse(tds[7].InnerText),
                                    Comments = tds.Length > 8 ? tds[8].InnerText.Replace("&nbsp;", " ").Trim() : string.Empty,
                                };
                            default: return null;
                        }
                    }).Where(z => z != null).ToDictionary(z => (z.Key, z.KeyType)),
                }).ToDictionary(x => x.Name);
            return r;
        }

        public static async Task<string> ApproveAsync(UnanetClient una, string type, (Grid grid, HtmlFormPost form) found, GridRow row, string comments)
        {
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
                var (d0, last) = await una.PostValueAsync(HttpMethod.Post, url, body, null).ConfigureAwait(false);
                return last;
            }
            catch (Exception e) { return e.Message; }
        }
    }
}