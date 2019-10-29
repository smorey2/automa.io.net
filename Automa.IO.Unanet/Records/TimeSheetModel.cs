using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace Automa.IO.Unanet.Records
{
    public class TimeSheetModel : ModelBase
    {
        public int keySheet { get; set; }
        public Metadata Meta { get; set; }
        public List<Entry> Rows { get; set; }

        public class p_TimeSheet1 : TimeSheetModel
        {
            public string XCF { get; set; }
        }

        public static void PreAdjust(UnanetClient una, p_TimeSheet1 s, out string last)
        {
            var r = una.PostValue(HttpMethod.Get, "people/time/preadjust", null,
                $"timesheetkey={s.keySheet}",
                out last);
        }

        public static TimeSheetModel Get(UnanetClient una, int keySheet, out string last)
        {
            var d0 = una.PostValue(HttpMethod.Get, "people/time/edit", null,
                $"timesheetkey={keySheet}",
                out last);
            return Parse(d0, keySheet);
        }

        public static void Save(UnanetClient una, TimeSheetModel s, out string last)
        {
            var f = new HtmlFormPost();
            f.Add("submitButton", "action", "save");
            f.Add("submitComments", "text", null);
            f.Add("timesheetkey", "text", s.keySheet.ToString());
            for (var i = 0; i < 7; i++)
                f.Add($"date_{i}", "text", s.Meta.Dates[i].ToString("M/d/yyyy"));
            f.Add("rows", "text", s.Rows.Count.ToString());
            f.Add("columns", "text", "7");
            //
            for (var i = 0; i < s.Rows.Count; i++)
            {
                var row = s.Rows[i];
                f.Add($"project_{i}", "text", $"{row.Project.Key}");
                f.Add($"task_{i}", "text", $"{row.Task.key}");
                f.Add($"labcat_{i}", "text", $"{row.Labcat.key}");
                f.Add($"loc_{i}", "text", $"{row.Loc.key}");
                f.Add($"projecttype_{i}", "text", $"{row.ProjectType.key}");
                f.Add($"paycode_{i}", "text", $"{row.Paycode.key}");
                foreach (var j in row.Slips)
                {
                    var slip = j.Value;
                    f.Add($"k_{i}_{j.Key}", "text", $"{slip.Key}");
                    f.Add($"d_{i}_{j.Key}", "text", $"{slip.Hours}");
                    f.Add($"c_{i}_{j.Key}", "text", slip.Comments);
                    f.Add($"t_{i}_{j.Key}_count", "text", "0");
                }
            }
            f.Add("tito_count", "text", "0");
            var post = f.ToString();
            var d0 = una.PostValue(HttpMethod.Post, "people/time/save", post,
                null,
                out last);
            var d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
        }

        public class Metadata
        {
            public Dictionary<int, (int key, string value)> ProjectTypes;
            public Dictionary<int, (int key, string value)> Paycodes;
            public Dictionary<int, (int key, string value)> LaborCategories;
            public List<DateTime> Dates;
            public Dictionary<int, Project> Projects;
        }

        [DebuggerDisplay("{Name}")]
        public class Project
        {
            public int Key { get; set; }
            public string Name { get; set; }
            public Dictionary<int, (int key, string value)> Labcats { get; set; }
            public Dictionary<int, (int key, string value)> Tasks { get; set; }
            public (int key, string value) ProjectType { get; set; }
            public List<(int key, string value)> Paycodes { get; set; }
            public (int key, string value) Paycode { get; set; }
        }

        [DebuggerDisplay("{Project},{Task},{LaborCategory},{ProjectType}")]
        public class Entry : ICloneable
        {
            public Project Project { get; set; }
            public (int key, string value) Task { get; set; }
            public (int key, string value) ProjectType { get; set; }
            public (int key, string value) Paycode { get; set; }
            public (int key, string value) Labcat { get; set; }
            public (int key, string value) Loc { get; set; }
            public Dictionary<int, Slip> Slips { get; set; }

            public object Clone() => MemberwiseClone();
        }

        [DebuggerDisplay("{Key},{Hours}")]
        public class Slip
        {
            public int Key { get; set; }
            public decimal Hours { get; set; }
            public string Comments { get; set; }
        }

        public void MoveEntry(int key, int project_codeKey, int task_nameKey, out string last)
        {
            last = null;
            // find slip
            var keySlip = Rows.SelectMany(x => x.Slips, (row, slip) => new { row, slip }).FirstOrDefault(x => x.slip.Value.Key == key);
            if (keySlip == null)
            {
                last = "unable to find src Slip";
                return;
            }
            keySlip.row.Slips.Remove(keySlip.slip.Key);
            if (!Meta.Projects.TryGetValue(project_codeKey, out var project))
            {
                last = "unable to find dst Project";
                return;
            }
            if (!project.Tasks.TryGetValue(task_nameKey, out var task))
            {
                last = "unable to find dst Task";
                return;
            }
            // clone row
            var clone = (Entry)keySlip.row.Clone();
            clone.Project = project;
            clone.Task = task;
            if (!project.Labcats.ContainsKey(clone.Labcat.key))
                clone.Labcat = project.Labcats.Values.First(x => x.value == "Other");
            clone.ProjectType = project.ProjectType;
            clone.Paycode = project.Paycode;
            var cloneSlip = keySlip.slip.Value;
            clone.Slips.Add(cloneSlip.Key, cloneSlip);
            // find or insert row
            var match0 = Rows.Where(x => x.Project.Key == clone.Project.Key && x.Task.key == clone.Task.key && x.Labcat.key == clone.Labcat.key).ToList();
            if (match0.Count == 0)
            {
                Rows.Add(clone);
                return;
            }
            var match1 = match0.Where(x => x.ProjectType.key == clone.ProjectType.key).ToList();
            if (match1.Count == 0)
            {
                Rows.Add(clone);
                return;
            }
            var match2 = match1.Where(x => x.Paycode.key == clone.Paycode.key).ToList();
            if (match2.Count == 0)
            {
                Rows.Add(clone);
                return;
            }
            // add slip, exit
            var first = match2.First();
            if (!first.Slips.TryGetValue(cloneSlip.Key, out var slipValue))
            {
                first.Slips.Add(cloneSlip.Key, cloneSlip);
                return;
            }
            // merge in
            slipValue.Hours += cloneSlip.Hours;
            slipValue.Comments += "\n" + cloneSlip.Comments;
        }

        public void TrimAll()
        {
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                var row = Rows[i];
                if (row.Slips.Count == 0)
                    Rows.RemoveAt(i);
            }
        }

        public static TimeSheetModel Parse(string source, int keySheet)
        {
            var meta = new Metadata
            {
                ProjectTypes = Regex.Match(source.ExtractSpanInner("var projecttypes = [", "];"), @"key:([\d]+).*\('([^']+)'\)")
                    .RegExSelect(m => (int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1),
                Paycodes = Regex.Match(source.ExtractSpanInner("var paycodesByKey = new KeyedSet();", "var projects = [];"), @"put\(([\d]+).*\('([^']+)'\)")
                    .RegExSelect(m => (int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1),
                LaborCategories = Regex.Match(source.ExtractSpanInner("mLabCats = [];", "labCats = mLabCats;"), @"put\(([\d]+).*\('([^']+)'\)")
                    .RegExSelect(m => (int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1),
                Dates = Regex.Match(source.ExtractSpanInner("var dates = [];", "var titoWindow = null;"), @"Date\(([\d]+),([\d]+),([\d]+)\)")
                    .RegExSelect(m => new DateTime(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value) + 1, int.Parse(m.Groups[3].Value))).ToList(),
            };
            // projects
            int lastIdx = 0;
            meta.Projects = Regex.Match(source, @"Project\(([\d]+).*?\('([^']+)'\).*projecttypes\[([\d]+)\].*pcBK\[([\d]+)\]")
               .RegExSelect(m =>
               {
                   var startIdx = m.Captures[0].Index;
                   var labcats = Regex.Match(source.ExtractSpanInner("pLabCats = [];", "labCats = pLabCats;", lastIdx, startIdx) ?? "", @"get.([\d]+)")
                        .RegExSelect(n => int.Parse(n.Groups[1].Value)).ToList()
                        .Select(x => meta.LaborCategories[x]).ToDictionary(x => x.Item1);
                   var tasks = Regex.Match(source.ExtractSpanInner("tasks = [];", "paycodes = [", lastIdx, startIdx) ?? "", @"Task\(([\d]+).*\('([^']+)'\)")
                        .RegExSelect(n => (int.Parse(n.Groups[1].Value), HttpUtility.UrlDecode(n.Groups[2].Value))).ToDictionary(x => x.Item1);
                   var paycodes = Regex.Match(source.ExtractSpanInner("paycodes = [", "];", lastIdx, startIdx) ?? "", @"pcBK\[([\d]+)\]")
                       .RegExSelect(n => int.Parse(n.Groups[1].Value)).ToList()
                       .Select(x => meta.Paycodes[x]).ToList();
                   lastIdx = startIdx;
                   return new Project
                   {
                       Key = int.Parse(m.Groups[1].Value),
                       Name = HttpUtility.UrlDecode(m.Groups[2].Value),
                       Labcats = labcats,
                       Tasks = tasks,
                       ProjectType = meta.ProjectTypes.Values.ElementAt(int.Parse(m.Groups[3].Value)),
                       Paycodes = paycodes,
                       Paycode = meta.Paycodes[int.Parse(m.Groups[4].Value)],
                   };
               }).ToDictionary(x => x.Key);

            // timeslips
            lastIdx = 0;
            source = source.ExtractSpan("timeslips = [];", "var dates = [];");
            var rows = Regex.Match(source, @"projects\[([\d]+)\].*tasks\[([\d]+)\].*projecttypes\[([\d]+)\].*paycodesByKey.get\(([\d]+)\).*labCatByKey.get\(([\d]+)\)")
                .RegExSelect(m =>
                {
                    var startIdx = m.Captures[0].Index;
                    var slips = Regex.Match(source.ExtractSpanInner("timeslips = [];", "rows.push(", lastIdx, startIdx) ?? "", @"timeslips\[([\d]+)\].*Timeslip\(([\d]+),([\d]+).*\('([^']+)'\)")
                         .RegExSelect(n => (int.Parse(n.Groups[1].Value), new Slip
                         {
                             Key = int.Parse(n.Groups[2].Value),
                             Hours = decimal.Parse(n.Groups[3].Value),
                             Comments = n.Groups[4].Value,
                         })).ToDictionary(x => x.Item1, x => x.Item2);
                    lastIdx = startIdx;
                    return new Entry
                    {
                        Project = meta.Projects.Values.ElementAt(int.Parse(m.Groups[1].Value)),
                        Task = meta.Projects.Values.ElementAt(int.Parse(m.Groups[1].Value)).Tasks.Values.ElementAt(int.Parse(m.Groups[2].Value)),
                        ProjectType = meta.ProjectTypes.Values.ElementAt(int.Parse(m.Groups[3].Value)),
                        Paycode = meta.Paycodes[int.Parse(m.Groups[4].Value)],
                        Labcat = meta.LaborCategories[int.Parse(m.Groups[5].Value)],
                        Slips = slips,
                    };
                }).ToList();
            return new TimeSheetModel
            {
                keySheet = keySheet,
                Meta = meta,
                Rows = rows,
            };
        }
    }
}