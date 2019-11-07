using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Automa.IO.Unanet.Records
{
    public class ApprovalModel : ModelBase
    {
        public static object ManagerApprovalByKeySheet(UnanetClient una, string personName, int keySheet, out string last) => Approve(una, "people", personName, "keySheet", keySheet, out last);
        public static object ProjectApprovalByKeySheet(UnanetClient una, string personName, int keySheet, out string last) => Approve(una, "projects", personName, "keySheet", keySheet, out last);
        static object Approve(UnanetClient una, string type, string personName, string method, int? keySheet, out string last)
        {
            GetApprovals(una, type, personName, out last);
            if (last != null)
                return null;
            return null;
        }

        public class Grid
        {
            public int Key { get; set; }
            public string Name { get; set; }
            public string PersonName { get; set; }
            public Dictionary<(int key, string keyType), GridRow> Rows { get; set; }
        }

        public class GridRow
        {
            public int Key { get; set; }
            public string KeyType { get; set; }
            public string Name { get; set; }
            public string PersonName { get; set; }
            public DateTime Week { get; set; }
            public decimal? Hours { get; set; }
            public string Status { get; set; }
            public DateTime StatusDate { get; set; }
            public string Comments { get; set; }
        }

        public static Grid GetApprovals(UnanetClient una, string type, string personName, out string last)
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
                    throw new InvalidOperationException("Unauthorized");
                var items = Parse(d0, prefix);
                var found = items.TryGetValue(personName, out var item) ? item
                    : personName == "first" ? items.First().Value
                    : null;
                if (found == null)
                    return null;
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
                items = Parse(d0, prefix);
                found = items.TryGetValue(personName, out item) ? item
                    : personName == "first" ? items.First().Value
                    : null;
                return found;
            }
            catch (Exception e) { last = e.Message; }
            return null;
        }

        public static IDictionary<string, Grid> Parse(string source, string prefix)
        {
            source = source.ExtractSpan("<form", "</form>");
            var doc = source.ToHtmlDocument();
            var divs = doc.DocumentNode.SelectNodes($"//div[@id='{prefix}']/div").ToArray();
            var r = divs.Where((e, i) => i % 2 == 0).Select((a, b) => new { a, b = divs[b * 2 + 1].InnerHtml.ToHtmlDocument().DocumentNode })
                .Select(x => new Grid
                {
                    Key = int.Parse(x.a.Attributes["id"].Value.Substring(prefix.Length + 7).Replace("-a", "")),
                    Name = x.a.InnerText.Remove(x.a.InnerText.IndexOf("(")).Trim(),
                    PersonName = x.a.InnerText.ExtractSpanInner("(", ")").ToUpperInvariant(),
                    Rows = x.b.SelectNodes($"//tr[starts-with(@id,'{prefix}')]")?.Select(y =>
                    {
                        var ats = y.Attributes["id"].Value.Substring(prefix.Length + 1).Split('_');
                        var tds = y.Descendants("td").ToArray();
                        return new GridRow
                        {
                            Key = int.Parse(ats[2].Split(',').Last()),
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
    }
}