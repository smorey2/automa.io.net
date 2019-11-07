using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class TimeSheetModel : ModelBase
    {
        public int KeySheet { get; set; }
        public (int? key, string name) Person { get; set; }
        public DateTime Week { get; set; }
        public SheetStatus Status { get; set; }
        public Metadata Meta { get; set; }
        public List<Entry> Rows { get; set; }

        #region Classes

        public class p_TimeSheet1 : TimeSheetModel
        {
            public string XCF { get; set; }
        }

        public enum SheetStatus
        {
            Missing,
            Inuse,
            Submitted,
            Approving,
            Disapproved,
            Completed,
            Locked,
            InuseAdjustment,
            SubmittedAdjustment,
            ApprovingAdjustment,
            DisapprovedAdjustment,
        }

        public class Metadata
        {
            public Dictionary<int, (int? key, string value)> ProjectTypes;
            public Dictionary<int, (int? key, string value)> Paycodes;
            public Dictionary<int, (int? key, string value)> LaborCategories;
            public List<DateTime> Dates;
            public Dictionary<int, Project> Projects;
        }

        [DebuggerDisplay("{Name}")]
        public class Project
        {
            public int Key { get; set; }
            public string Name { get; set; }
            public Dictionary<int, (int? key, string value)> Labcats { get; set; }
            public Dictionary<int, (int? key, string value)> Tasks { get; set; }
            public (int? key, string value) ProjectType { get; set; }
            public List<(int? key, string value)> Paycodes { get; set; }
            public (int? key, string value) Paycode { get; set; }
        }

        [DebuggerDisplay("{Project}: {Task},{Labcat},{ProjectType}")]
        public class Entry : ICloneable
        {
            public Project Project { get; set; }
            public (int? key, string value) Task { get; set; }
            public (int? key, string value) ProjectType { get; set; }
            public (int? key, string value) Paycode { get; set; }
            public (int? key, string value) Labcat { get; set; }
            public (int? key, string value) Loc { get; set; }
            public Dictionary<int, Slip> Slips { get; set; }

            public object Clone()
            {
                var clone = (Entry)MemberwiseClone();
                clone.Slips = new Dictionary<int, Slip>();
                return clone;
            }
        }

        [DebuggerDisplay("{Key}: {Hours}")]
        public class Slip
        {
            public int? Key { get; set; }
            public decimal Hours { get; set; }
            public string Comments { get; set; }
            public SlipTito[] Titos { get; set; }
        }

        [DebuggerDisplay("{Key}")]
        public class SlipTito
        {
            public int? Key { get; set; }
            public DateTime Start { get; set; }
            public DateTime Stop { get; set; }
            public decimal Nonwork { get; set; }
            public string Comments { get; set; }
        }

        #endregion

        public (decimal Hours, string Body) GetChecksum()
        {
            var hours = 0M;
            var b = new StringBuilder();
            b.AppendLine($"KeySheet: {KeySheet}, Rows: {Rows.Count}");
            b.AppendLine($"|{"PROJECT",40}|{"TASK",40}|{"LABCAT",15}|{"TYPE",8}|{"JURIS",8}| MON| TUE| WED| THU| FRI| SAT| SUN|");
            foreach (var row in Rows)
            {
                b.Append($"|{row.Project.Name,4}|{row.Task.value,30}|{row.Labcat.value,15}|{row.ProjectType.value,8}|{row.Paycode.value,8}");
                var slips = row.Slips;
                for (var i = 0; i < 7; i++)
                    if (slips.TryGetValue(i, out var slip))
                    {
                        hours += slip.Hours;
                        b.Append($"|{slip.Hours,4}");
                    }
                    else b.Append($"|{"",4}");
                b.AppendLine("|");
            }
            b.AppendLine($"Hours: {hours}");
            return (hours, b.ToString());
        }

        static string PreAdjust(UnanetClient una, int keySheet, out string last) =>
           una.PostValue(HttpMethod.Get, "people/time/preadjust", null, $"timesheetkey={keySheet}", out last);

        public static TimeSheetModel Get(UnanetClient una, int keySheet, out string last, bool preAdjust = false)
        {
            var d0 = una.PostValue(HttpMethod.Get, "people/time/edit", null, $"timesheetkey={keySheet}", out last);
            var time = Parse(d0, keySheet);
            if (preAdjust && (time.Status != SheetStatus.Inuse || time.Status != SheetStatus.InuseAdjustment))
            {
                d0 = PreAdjust(una, keySheet, out last);
                time = Parse(d0, keySheet);
            }
            return time;
        }

        public static bool Save(UnanetClient una, TimeSheetModel s, string submitComments, string approvalComments, out string last)
        {
            var f = new HtmlFormPost { Action = "/roundarch/action/people/time/save" };
            if (s.Status == SheetStatus.Inuse)
            {
                f.Add("submitButton", "action", "save");
                f.Add("submitComments", "text", null);
            }
            else
            {
                f.Add("submitButton", "action", "submit");
                f.Add("submitComments", "text", submitComments);
            }
            f.Add("timesheetkey", "text", s.KeySheet.ToString());
            for (var i = 0; i < 7; i++)
                f.Add($"date_{i}", "text", s.Meta.Dates[i].ToString("M/d/yyyy"));
            f.Add("rows", "text", s.Rows.Count.ToString());
            f.Add("columns", "text", "7");
            // rows
            for (var i = 0; i < s.Rows.Count; i++)
            {
                var row = s.Rows[i];
                f.Add($"project_{i}", "text", $"{row.Project.Key}");
                f.Add($"task_{i}", "text", $"{row.Task.key}");
                f.Add($"labcat_{i}", "text", $"{row.Labcat.key}");
                f.Add($"loc_{i}", "text", $"{row.Loc.key}");
                f.Add($"projecttype_{i}", "text", $"{row.ProjectType.key}");
                f.Add($"paycode_{i}", "text", $"{row.Paycode.key}");
                // slips
                foreach (var j in row.Slips)
                {
                    var slip = j.Value; var jk = j.Key;
                    f.Add($"k_{i}_{jk}", "text", $"{slip.Key}");
                    f.Add($"d_{i}_{jk}", "text", $"{slip.Hours}");
                    f.Add($"c_{i}_{jk}", "text", slip.Comments);
                    f.Add($"t_{i}_{jk}_count", "text", $"{slip.Titos.Length}");
                    // titos
                    for (var k = 0; k < slip.Titos.Length; k++)
                    {
                        var tito = slip.Titos[k];
                        f.Add($"t_{i}_{jk}_key_{k}", "text", $"{tito.Key}");
                        f.Add($"t_{i}_{jk}_start_{k}", "text", tito.Start.ToString("yyyy-dd-MM HH:MM:ss"));
                        f.Add($"t_{i}_{jk}_stop_{k}", "text", tito.Stop.ToString("yyyy-dd-MM HH:MM:ss"));
                        f.Add($"t_{i}_{jk}_nonwork_{k}", "text", $"{tito.Nonwork}");
                        f.Add($"t_{i}_{jk}_comments_{k}", "text", tito.Comments);
                    }
                }
            }
            f.Add("tito_count", "text", "0");
            var d0 = una.PostValue(HttpMethod.Post, f.Action.Substring(18), f.ToString(), null, out last);
            var d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
            if (d1 != null)
                last = d0.ExtractSpanInner("ERROR:");
            if (last != null || !d0.Contains("Adjustments - Enter a change reason for all modified entries"))
                return false;

            // adjustments
            f = new HtmlFormPost(d0);
            var post = f.ToString();
            //f["ignore_warnings"] = "true";
            f.Values["globalComment"] = "true";
            f.Values["comments_00"] = approvalComments;
            f.Add("button_save", "text", null);
            d0 = una.PostValue(HttpMethod.Post, f.Action.Substring(18), f.ToString(), null, out last);
            d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
            if (d1 != null)
                last = d0.ExtractSpanInner("ERROR:");
            return true;
        }

        public void MoveEntry(int key, int project_codeKey, int task_nameKey, out string last, Action<TimeSheetModel, Entry, KeyValuePair<int, Slip>> customClone = null)
        {
            last = null;
            // find slip
            var keySlip = Rows.SelectMany(x => x.Slips, (r, s) => new { r, s }).FirstOrDefault(x => x.s.Value.Key == key);
            if (keySlip == null) { last = "unable to find src Slip"; return; }
            if (!keySlip.r.Slips.Remove(keySlip.s.Key)) throw new InvalidOperationException();
            if (!Meta.Projects.TryGetValue(project_codeKey, out var project)) { last = "unable to find dst Project"; return; }
            if (!project.Tasks.TryGetValue(task_nameKey, out var task)) { last = "unable to find dst Task"; return; }
            // clone row
            var clone = (Entry)keySlip.r.Clone();
            clone.Project = project;
            clone.Task = task;
            if (clone.Labcat.key != null && !project.Labcats.ContainsKey(clone.Labcat.key.Value))
                clone.Labcat = project.Labcats.Values.First(x => x.value == "Other");
            //clone.ProjectType = project.ProjectType;
            //clone.Paycode = project.Paycode;
            var slip = keySlip.s; clone.Slips.Add(slip.Key, slip.Value); // slip.Value.Key = null; 
            customClone?.Invoke(this, clone, slip);
            // find or insert row
            var match0 = Rows.Where(x => x.Project.Key == clone.Project.Key && x.Task.key == clone.Task.key && x.Labcat.key == clone.Labcat.key).ToList();
            if (match0.Count == 0) { Rows.Add(clone); return; }
            var match1 = match0.Where(x => x.ProjectType.key == clone.ProjectType.key).ToList();
            if (match1.Count == 0) { Rows.Add(clone); return; }
            var match2 = match1.Where(x => x.Paycode.key == clone.Paycode.key).ToList();
            if (match2.Count == 0) { Rows.Add(clone); return; }
            // add slip, exit
            var firstSlips = match2.First().Slips;
            if (!firstSlips.TryGetValue(slip.Key, out var existingSlip)) { firstSlips.Add(slip.Key, slip.Value); return; }
            // merge slip in, error if tito
            if (existingSlip.Titos.Length != 0 || slip.Value.Titos.Length != 0) { last = "move requires merge, but tito exists"; return; }
            // merge slip in
            existingSlip.Hours += slip.Value.Hours;
            existingSlip.Comments += "\n" + slip.Value.Comments;
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
            var statusString = source.ExtractSpanInner("<label>Status:</label>", "</div>").Replace("&nbsp;", " ");
            var status =
                statusString.Contains("INUSE (Adjustments)") ? SheetStatus.InuseAdjustment
                : statusString.Contains("SUBMITTED (Adjustments)") ? SheetStatus.SubmittedAdjustment
                : statusString.Contains("APPROVING (Adjustments)") ? SheetStatus.ApprovingAdjustment
                : statusString.Contains("DISAPPROVED (Adjustments)") ? SheetStatus.DisapprovedAdjustment
                //
                : statusString.Contains("INUSE") ? SheetStatus.Inuse
                : statusString.Contains("SUBMITTED") ? SheetStatus.Submitted
                : statusString.Contains("APPROVING") ? SheetStatus.Approving
                : statusString.Contains("DISAPPROVED") ? SheetStatus.Disapproved
                : statusString.Contains("COMPLETED") ? SheetStatus.Completed
                : statusString.Contains("LOCKED") ? SheetStatus.Locked
                : SheetStatus.Missing;
            return status == SheetStatus.Inuse || status == SheetStatus.InuseAdjustment
                ? ParseEdit(source, keySheet, status)
                : ParseView(source, keySheet, status);
        }

        static TimeSheetModel ParseView(string source, int keySheet, SheetStatus status)
        {
            var title = source.ExtractSpanInner("setSubsectionTitle('", "');");
            var personString = title.ExtractSpanInner("Timesheet for", "(").Trim();
            var person = ((int?)null, personString);
            var week = DateTime.Parse(title.ExtractSpanInner(") (", " ").Trim());
            //
            source = source.ExtractSpanInner("<table class=\"timesheet\">", "</table>");
            source = source.ExtractSpan("<tbody>", "</tbody>");
            var doc = XElement.Parse(source);
            var rows = new List<Entry>();
            string project = null;
            foreach (var tr in doc.Elements("tr"))
            {
                if (tr.Attribute("class").Value.Contains("row-change"))
                {
                    project = tr.Elements("td").ToArray()[0].Value;
                    continue;
                }
                var tds = tr.Elements("td").ToArray();
                rows.Add(new Entry
                {
                    Project = new Project { Name = project },
                    Task = (0, tds[0].Value),
                    Labcat = (0, tds[1].Value),
                    Loc = (0, tds[2].Value),
                    ProjectType = (0, tds[3].Value),
                    Paycode = (0, tds[4].Value),
                    Slips = new[] {
                        (0, !string.IsNullOrEmpty(tds[5].Value) ? new Slip { Key = 0, Hours = decimal.Parse(tds[5].Value)} : null),
                        (1, !string.IsNullOrEmpty(tds[6].Value) ? new Slip { Key = 0, Hours = decimal.Parse(tds[6].Value)} : null),
                        (2, !string.IsNullOrEmpty(tds[7].Value) ? new Slip { Key = 0, Hours = decimal.Parse(tds[7].Value)} : null),
                        (3, !string.IsNullOrEmpty(tds[8].Value) ? new Slip { Key = 0, Hours = decimal.Parse(tds[8].Value)} : null),
                        (4, !string.IsNullOrEmpty(tds[9].Value) ? new Slip { Key = 0, Hours = decimal.Parse(tds[9].Value)} : null),
                        (5, !string.IsNullOrEmpty(tds[10].Value) ? new Slip { Key = 0, Hours = decimal.Parse(tds[10].Value)} : null),
                        (6, !string.IsNullOrEmpty(tds[11].Value) ? new Slip { Key = 0, Hours = decimal.Parse(tds[11].Value)} : null),
                    }.Where(x => x.Item2 != null).ToDictionary(x => x.Item1, x => x.Item2),
                });
            }
            return new TimeSheetModel
            {
                KeySheet = keySheet,
                Person = person,
                Week = week,
                Status = status,
                Rows = rows,
            };
        }

        static TimeSheetModel ParseEdit(string source, int keySheet, SheetStatus status)
        {
            var title = source.ExtractSpanInner("<title>", "</title>");
            //var personString = string.Join(" ", title.ExtractSpanInner("Timesheet for", "(").Trim().Split(',').Reverse().ToArray());
            var personString = title.ExtractSpanInner("Timesheet for", "(").Trim();
            var personGroups = Regex.Match(source.ExtractSpanInner("new Person(", ");"), @"([\d]+),([\d]+),([\d]+)").Groups;
            var person = ((int?)int.Parse(personGroups[1].Value), personString);
            var week = DateTime.Parse(title.ExtractSpanInner("(", " ").Trim());
            //
            var meta = new Metadata
            {
                ProjectTypes = Regex.Matches(source.ExtractSpanInner("var projecttypes = [", "];"), @"key:([\d]+).*\('([^']*)'\)", RegexOptions.Multiline).Cast<Match>()
                    .Select(m => ((int?)int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1.Value),
                Paycodes = Regex.Matches(source.ExtractSpanInner("var paycodesByKey = new KeyedSet();", "var projects = [];"), @"put\(([\d]+).*\('([^']*)'\)").Cast<Match>()
                    .Select(m => ((int?)int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1.Value),
                LaborCategories = Regex.Matches(source.ExtractSpanInner("mLabCats = [];", "labCats = mLabCats;"), @"put\(([\d]+).*\('([^']*)'\)", RegexOptions.Multiline).Cast<Match>()
                    .Select(m => ((int?)int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1.Value),
                Dates = Regex.Matches(source.ExtractSpanInner("var dates = [];", "var titoWindow = null;"), @"Date\(([\d]+),([\d]+),([\d]+)\)", RegexOptions.Multiline).Cast<Match>()
                    .Select(m => new DateTime(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value) + 1, int.Parse(m.Groups[3].Value))).ToList(),
            };
            // projects
            int lastIdx = 0;
            meta.Projects = Regex.Matches(source, @"Project\(([\d]+).*?\('([^']*)'\).*projecttypes\[([\d]+)\].*pcBK\[([\d]+)\]", RegexOptions.Multiline).Cast<Match>()
               .Select(m =>
               {
                   var startIdx = m.Captures[0].Index;
                   var labcats = Regex.Matches(source.ExtractSpanInner("pLabCats = [];", "labCats = pLabCats;", lastIdx, startIdx) ?? "", @"get.([\d]+)", RegexOptions.Multiline).Cast<Match>()
                        .Select(n => (int?)int.Parse(n.Groups[1].Value)).ToList()
                        .Select(x => meta.LaborCategories[x.Value]).ToDictionary(x => x.key.Value);
                   var tasks = Regex.Matches(source.ExtractSpanInner("tasks = [];", "paycodes = [", lastIdx, startIdx) ?? "", @"Task\(([\d]+).*\('([^']*)'\)", RegexOptions.Multiline).Cast<Match>()
                        .Select(n => ((int?)int.Parse(n.Groups[1].Value), HttpUtility.UrlDecode(n.Groups[2].Value))).ToDictionary(x => x.Item1.Value);
                   var paycodes = Regex.Matches(source.ExtractSpanInner("paycodes = [", "];", lastIdx, startIdx) ?? "", @"pcBK\[([\d]+)\]", RegexOptions.Multiline).Cast<Match>()
                       .Select(n => (int?)int.Parse(n.Groups[1].Value)).ToList()
                       .Select(x => meta.Paycodes[x.Value]).ToList();
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
            var rows = Regex.Matches(source, @"projects\[([\d]+)\].*tasks\[([\d]+)\].*projecttypes\[([\d]+)\].*paycodesByKey.get\(([\d]+)\).*labCatByKey.get\(([\d]+)\)", RegexOptions.Multiline).Cast<Match>()
                .Select(m =>
                {
                    var startIdx = m.Captures[0].Index;
                    var slips = Regex.Matches(source.ExtractSpanInner("timeslips = [];", "rows.push(", lastIdx, startIdx) ?? "", @"timeslips\[([\d]+)\].*Timeslip\(([\d]+),([\d\.]+),[^']*'([^']*)'[^,]*,(null|\[[^\]]*\])").Cast<Match>()
                         .Select(n =>
                         {
                             var titos = n.Groups[5].Value == "null"
                                ? new SlipTito[0]
                                : Regex.Matches(n.Groups[5].Value.ExtractSpanInner("[", "]"), @"Tito\(([\d]+).*?Date\(([\d]+),([\d]+),([\d]+),([\d]+),([\d]+),([\d]+),([\d]+)\).*?Date\(([\d]+),([\d]+),([\d]+),([\d]+),([\d]+),([\d]+),([\d]+)\),([\d\.]+),.*?'([^']*)'").Cast<Match>()
                                    .Select(o => new SlipTito
                                    {
                                        Key = int.Parse(o.Groups[1].Value),
                                        Start = new DateTime(int.Parse(o.Groups[2].Value), int.Parse(o.Groups[3].Value) + 1, int.Parse(o.Groups[4].Value), int.Parse(o.Groups[5].Value), int.Parse(o.Groups[6].Value), int.Parse(o.Groups[7].Value)).AddMilliseconds(int.Parse(o.Groups[8].Value)),
                                        Stop = new DateTime(int.Parse(o.Groups[9].Value), int.Parse(o.Groups[10].Value) + 1, int.Parse(o.Groups[11].Value), int.Parse(o.Groups[12].Value), int.Parse(o.Groups[13].Value), int.Parse(o.Groups[14].Value)).AddMilliseconds(int.Parse(o.Groups[15].Value)),
                                        Nonwork = decimal.Parse(o.Groups[16].Value),
                                        Comments = o.Groups[17].Value,
                                    }).ToArray();
                             return (int.Parse(n.Groups[1].Value), new Slip
                             {
                                 Key = int.Parse(n.Groups[2].Value),
                                 Hours = decimal.Parse(n.Groups[3].Value),
                                 Comments = n.Groups[4].Value,
                                 Titos = titos,
                             });
                         }).ToDictionary(x => x.Item1, x => x.Item2);
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
                KeySheet = keySheet,
                Person = person,
                Week = week,
                Status = status,
                Meta = meta,
                Rows = rows,
            };
        }
    }
}