using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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

        public static ILookup<string, PeopleTimeStatusReport> Get(UnanetClient una, DateTime weekDate)
        {
            weekDate = EnsureBeginOfWeek(weekDate);
            var source = una.RunReport("/people/status/time", f =>
            {
                var contains = $"Weekly ({weekDate:M/d/yyyy}";
                foreach (var x in f.Checked.Keys.ToArray())
                    if (x.StartsWith("tsStatus") && !f.Checked[x])
                        f.Checked[x] = true;
                f.FromSelectByPredicate("time_period", contains, x => x.Value.StartsWith(contains));
                return f.ToString();
            });
            return Parse(source);
        }

        public static ILookup<string, PeopleTimeStatusReport> Parse(string source)
        {
            source = source.ExtractSpan("<tr class=\"dl-2 dl-header\">", "<tr class=\"dt-list\">")
                .Replace("\"></a></td>", "\" /></a></td>")
                .Replace("&nbsp;", " ");
            source = $"<tbody>{source}</tr></tbody>";
            var doc = XElement.Parse(source);
            var rows = new List<PeopleTimeStatusReport>();
            foreach (var tr in doc.Elements("tr"))
            {
                if (!tr.Attribute("class").Value.Contains("dl-3"))
                    continue;
                var tds = tr.Elements("td").ToArray();
                rows.Add(new PeopleTimeStatusReport
                {
                    PersonName = tds[1].Value.Remove(tds[1].Value.IndexOf("(")).Trim(),
                    PersonUsername = tds[1].Value.ExtractSpanInner("(", ")").ToUpperInvariant(),
                    Hours = decimal.Parse(tds[2].Value),
                    OutOf = decimal.Parse(tds[3].Value),
                    Status = tds[5].Value,
                    Date = string.IsNullOrEmpty(tds[6].Value) ? null : (DateTime?)DateTime.Parse($"{tds[6].Value} {tds[7].Value}"),
                    Managers = string.IsNullOrEmpty(tds[8].Value) ? null : tds[8].Elements("div").Select(x => x.Value).ToArray(),
                    ProjApprs = string.IsNullOrEmpty(tds[9].Value) ? null : tds[9].Elements("div").Select(x => x.Value.Replace("** ", "")).ToArray(),
                    Customers = string.IsNullOrEmpty(tds[10].Value) ? null : tds[10].Elements("div").Select(x => x.Value.Replace("* ", "")).ToArray(),
                });
            }
            return rows.ToLookup(x => x.PersonName);
        }
    }
}