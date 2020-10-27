using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Unanet.Reports
{
    public class PeopleTimeStatusReport : ReportBase
    {
        public string PersonName { get; set; }
        public string PersonUsername { get; set; }
        public decimal Hours { get; set; }
        public decimal OutOf { get; set; }
        public string Status { get; set; }
        public DateTime? Date { get; set; }
        public string[] Managers { get; set; }
        public string[] ProjApprs { get; set; }
        public string[] Customers { get; set; }

        public static async Task<ILookup<string, PeopleTimeStatusReport>> GetAsync(UnanetClient una, DateTime weekDate)
        {
            weekDate = EnsureBeginOfWeek(weekDate);
            var source = await una.RunReportAsync("/people/status/time", (z, f) =>
            {
                var contains = $"Weekly ({weekDate:M/d/yyyy}";
                foreach (var x in f.Checked.Keys.ToArray())
                    if (x.StartsWith("tsStatus") && !f.Checked[x])
                        f.Checked[x] = true;
                f.FromSelectStartsWith("time_period", contains);
                return f.ToString();
            }).ConfigureAwait(false);
            return !string.IsNullOrEmpty(source) ? Parse(source) : null;
        }


        public static ILookup<string, PeopleTimeStatusReport> Parse(string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            source = source.ExtractSpanInner("var tableData = [", "];");
            if (source == null)
                throw new InvalidOperationException("Parse marker not found");
            source = $"[{source}]";
            var doc = JArray.Parse(source);
            var rows = new List<PeopleTimeStatusReport>();
            foreach (var tr in doc)
            {
                if (tr["person"] == null)
                    continue;

                string[] AppToMulti(string x) => !x.StartsWith("<span class=\"multi-line-span\">") ? new[] { x } : x.Substring(30, x.Length - 30 - 7).Split(new[] { "</span><span class=\"multi-line-span\">" }, StringSplitOptions.RemoveEmptyEntries);
                rows.Add(new PeopleTimeStatusReport
                {
                    PersonName = tr["person"].Value<string>().Remove(tr["person"].Value<string>().IndexOf("(")).Trim(),
                    PersonUsername = tr["person"].Value<string>().ExtractSpanInner("(", ")").ToUpperInvariant(),
                    Hours = tr["outOf"] != null ? tr["hours"].Value<decimal>() : 0M,
                    OutOf = tr["outOf"] != null ? tr["outOf"].Value<decimal>() : 0M,
                    //percentOf = tr["percentOf"] != null ? tr["percentOf"].Value<string>() : null,
                    Status = tr["tStatus"] != null ? tr["tStatus"].Value<string>().ExtractSpanInner("<span class=float-left>", "</span>") : null,
                    Date = tr["tStatus"] != null && DateTime.TryParse(tr["tStatus"].Value<string>().ExtractSpanInner("<span class=float-right>", "</span>"), out var z) ? (DateTime?)z : null,
                    Managers = tr["appMgr"] != null ? AppToMulti(tr["appMgr"].Value<string>()).Select(x => x.Replace("*", string.Empty).Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray() : null,
                    ProjApprs = tr["appPA"] != null ? AppToMulti(tr["appPA"].Value<string>()).Select(x => x.Replace("*", string.Empty).Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray() : null,
                    Customers = tr["appCust"] != null ? AppToMulti(tr["appCust"].Value<string>()).Select(x => x.Replace("*", string.Empty).Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray() : null,
                });
            }
            return rows.ToLookup(x => x.PersonName);
        }

        //[Obsolete]
        //public static ILookup<string, PeopleTimeStatusReport> ParseObsolete(string source)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException(nameof(source));
        //    source = source.ExtractSpan("<tr class=\"dl-2 dl-header\">", "<tr class=\"dt-list\">")
        //        ?.Replace("\"></a></td>", "\" /></a></td>")
        //        .Replace("&nbsp;", " ");
        //    if (source == null)
        //        throw new InvalidOperationException("Parse marker not found");
        //    source = $"<tbody>{source}</tr></tbody>";
        //    var doc = XElement.Parse(source);
        //    var rows = new List<PeopleTimeStatusReport>();
        //    foreach (var tr in doc.Elements("tr"))
        //    {
        //        if (!tr.Attribute("class").Value.Contains("dl-3"))
        //            continue;
        //        var tds = tr.Elements("td").ToArray();
        //        rows.Add(new PeopleTimeStatusReport
        //        {
        //            PersonName = tds[1].Value.Remove(tds[1].Value.IndexOf("(")).Trim(),
        //            PersonUsername = tds[1].Value.ExtractSpanInner("(", ")").ToUpperInvariant(),
        //            Hours = decimal.Parse(tds[2].Value),
        //            OutOf = decimal.Parse(tds[3].Value),
        //            Status = tds[5].Value,
        //            Date = string.IsNullOrEmpty(tds[6].Value) ? null : (DateTime?)DateTime.Parse($"{tds[6].Value} {tds[7].Value}"),
        //            Managers = string.IsNullOrEmpty(tds[8].Value) ? null : tds[8].Elements("div").Select(x => x.Value).ToArray(),
        //            ProjApprs = string.IsNullOrEmpty(tds[9].Value) ? null : tds[9].Elements("div").Select(x => x.Value.Replace("** ", "")).ToArray(),
        //            Customers = string.IsNullOrEmpty(tds[10].Value) ? null : tds[10].Elements("div").Select(x => x.Value.Replace("* ", "")).ToArray(),
        //        });
        //    }
        //    return rows.ToLookup(x => x.PersonName);
        //}
    }
}