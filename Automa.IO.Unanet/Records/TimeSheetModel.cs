using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class TimeSheetModel : ModelBase
    {
        public bool IsEdit { get; set; }
        public int KeySheet { get; set; }
        public (int? key, string name) Person { get; set; }
        public DateTime Week { get; set; }
        public SheetStatus Status { get; set; }
        public Metadata Meta { get; set; }
        public List<Entry> Rows { get; set; }
        public string[] Errors { get; set; }

        const string Error_srcSlip = "Timesheet cannot be resubmitted by this process. Please ask timesheet owner to review and manually resubmit. (error code: srcSlip)";
        const string Error_dstProject = "This time cannot be moved to the requested project. Please be sure the person is assigned to the project and the project allows time entry. (error code: dstProject)";
        const string Error_dstTask = "This task is not open for time entry. Please request an adjustment from Finance and try again. (error code: dstTask)";
        const string Error_tito = "Timesheet Owner used the time In/Time Out function and the time cannot be updated with this process. Please ask timesheet owner to correct their timesheet manually. (errorcode: tito)";

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
            public Dictionary<string, (int? key, string value)> ProjectTypesByName;
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
            public string ProjectCode => Name.Remove(Name.IndexOf(' '));
            public Dictionary<int, (int? key, string value)> Labcats { get; set; }
            public Dictionary<string, (int? key, string value)> LabcatsByName { get; set; }
            public Dictionary<int, (int? key, string value)> Tasks { get; set; }
            public (int? key, string value) ProjectType { get; set; }
            public Dictionary<int, (int? key, string value)> Paycodes { get; set; }
            public Dictionary<string, (int? key, string value)> PaycodesByName { get; set; }
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

            public string GetPak(TimeSheetModel parent, string username, int slip) =>
                TimeModel.GetPak(username, parent.Week.AddDays(slip), Project.ProjectCode, Task.value, ProjectType.value, Labcat.value, Paycode.value);

            public object Clone()
            {
                var clone = (Entry)MemberwiseClone();
                clone.Slips = new Dictionary<int, Slip>();
                return clone;
            }

            public bool ContainsDate(TimeSheetModel parent, DateTime date) => Slips.ContainsKey((date - parent.Week).Days);

            public bool TryGetSlip(TimeSheetModel parent, DateTime date, out Slip slip) => Slips.TryGetValue((date - parent.Week).Days, out slip);
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

        #region Checksum

        public class Checksum
        {
            public class Row
            {
                public string Project { get; set; }
                public string Task { get; set; }
                public string ProjectType { get; set; }
                public string Paycode { get; set; }
                public string Labcat { get; set; }
                public string Loc { get; set; }
                public decimal Hours { get; set; }
            }

            public ILookup<string, string> CommandByPak { get; set; }
            public decimal Hours { get; set; }
            public string Body { get; set; }
            public IList<Row> Rows { get; set; }

            public Checksum(IList<(int key, string pak, string func)> command, TimeSheetModel sheet)
            {
                CommandByPak = command.ToLookup(x => x.pak, x => x.func);
                Update(sheet);
            }

            public void Update(TimeSheetModel sheet)
            {
                (Body, Hours) = GetBody(sheet);
                Rows = GetRows(sheet);
            }

            (string, decimal) GetBody(TimeSheetModel sheet)
            {
                string Trim(string source, int length, bool right = true) => source != null
                    ? source.Length < length ? right ? source.PadLeft(length) : source.PadRight(length) : source.Substring(0, length)
                    : null;
                var hours = 0M;
                var b = new StringBuilder();
                b.AppendLine($"Key: {sheet.KeySheet}, Rows: {sheet.Rows.Count}");
                b.AppendLine($"|{"PROJECT",40}|{"TASK",30}|{"LABCAT",20}|{"TYPE",8}|{"JURIS",8}|  MON|  TUE|  WED|  THU|  FRI|  SAT|  SUN|");
                var firstCommand = TimeModel.Unpak(CommandByPak.FirstOrDefault()?.Key);
                var foundPaks = new HashSet<string>();
                foreach (var row in sheet.Rows)
                {
                    b.Append($"|{Trim(row.Project.Name, 40, false)}|{Trim(row.Task.value, 30, false)}|{Trim(row.Labcat.value, 20)}|{Trim(row.ProjectType.value, 8)}|{Trim(row.Paycode.value, 8)}");
                    var slips = row.Slips;
                    for (var i = 0; i < 7; i++)
                        if (slips.TryGetValue(i, out var slip))
                        {
                            var pak = row.GetPak(sheet, firstCommand.username, i);
                            hours += slip.Hours;
                            b.Append($"|{slip.Hours,5}");
                            if (CommandByPak.Contains(pak))
                            {
                                foundPaks.Add(pak);
                                b.Append($"[{string.Join(", ", CommandByPak[pak])}]");
                            }
                        }
                        else b.Append($"|{"",5}");
                    b.AppendLine("|");
                }
                b.AppendLine($"Hours: {hours}");
                if (CommandByPak.Count != foundPaks.Count)
                {
                    b.AppendLine($"Missed Commands:");
                    foreach (var pak in CommandByPak.Where(x => !foundPaks.Contains(x.Key)))
                        b.AppendLine($"{TimeModel.Unpak(pak.Key)}[{string.Join(", ", pak)}]");
                }
                return (b.ToString(), hours);
            }

            static List<Row> GetRows(TimeSheetModel sheet) => sheet.Rows.Select(row => new Row
            {
                Project = row.Project?.Name,
                Task = row.Task.value,
                ProjectType = row.ProjectType.value,
                Paycode = row.Paycode.value,
                Labcat = row.Labcat.value,
                Loc = row.Loc.value,
                Hours = row.Slips.Sum(y => y.Value.Hours),
            }).OrderBy(x => x.Project).ThenBy(x => x.Task).ThenBy(x => x.ProjectType).ThenBy(x => x.Paycode).ThenBy(x => x.Labcat).ThenBy(x => x.Loc).ThenBy(x => x.Hours).ToList();

            public string Compare(Checksum checksum, string errorPattern, string last, bool rowCheck, Action<string, string, string> errorAction)
            {
                if (Hours != checksum.Hours)
                {
                    errorAction("HOURS", Body, checksum.Body);
                    return string.Format(errorPattern, Hours, checksum.Hours);
                }
                if (rowCheck && !Enumerable.SequenceEqual(Rows, checksum.Rows))
                {
                    //errorAction("ROWS", Body, checksum.Body);
                    //return string.Format(errorPattern, Hours, checksum.Hours);
                }
                return last;
            }
        }

        public Checksum GetChecksum(IList<(int key, string pak, string func)> command) => new Checksum(command, this);

        #endregion

        static Task<(string value, string last)> PreAdjustAsync(UnanetClient una, int keySheet) =>
           una.PostValueAsync(HttpMethod.Get, "people/time/preadjust", null, $"timesheetkey={keySheet}", useSafeRead: true);

        public static async Task<(TimeSheetModel value, string last)> GetAsync(UnanetClient una, int keySheet, bool preAdjust = false, bool useView = false, bool forceEdit = false)
        {
            var (d0, last) = await una.PostValueAsync(HttpMethod.Get, useView ? "people/time/view" : "people/time/edit", null, $"timesheetkey={keySheet}", useSafeRead: true).ConfigureAwait(false);
            var time = Parse(d0, keySheet);
            if (forceEdit && time.Meta == null)
                preAdjust = true;
            if (preAdjust && (time.Status != SheetStatus.Inuse || time.Status != SheetStatus.InuseAdjustment))
            {
                (d0, last) = await PreAdjustAsync(una, keySheet).ConfigureAwait(false);
                time = Parse(d0, keySheet);
            }
            return (time, last);
        }

        public static async Task<(bool approvals, string last)> SaveAsync(UnanetClient una, TimeSheetModel s, string submitComments, string approvalComments)
        {
            if (s.Meta == null)
                throw new ArgumentNullException(nameof(s.Meta));
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
            f.Add("timesheetkey", "text", $"{s.KeySheet}");
            for (var i = 0; i < 7; i++)
                f.Add($"date_{i}", "text", s.Meta.Dates[i].ToString("M/d/yyyy"));
            f.Add("timeRows", "text", s.Rows.Count.ToString());
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
            var (d0, last) = await una.PostValueAsync(HttpMethod.Post, f.Action.Substring(18), f.ToString(), null).ConfigureAwait(false);
            var d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
            if (d1 != null)
                last = d1.Replace("<BR>", null).Replace("<br>", null).Trim();
            if (last != null || !d0.Contains("Adjustments - Enter a change reason for all modified entries"))
                return (false, last);

            // adjustments
            f = new HtmlFormPost(d0);
            //var post = f.ToString();
            //f["ignore_warnings"] = "true";
            f.Values["globalComment"] = "true";
            f.Values["comments_00"] = approvalComments;
            f.Add("button_save", "text", null);
            (d0, last) = await una.PostValueAsync(HttpMethod.Post, f.Action.Substring(18), f.ToString(), null).ConfigureAwait(false);
            d1 = d0.ExtractSpanInner("<div class=\"error\">", "</div>");
            if (d1 != null)
                last = d1.Replace("<BR>", null).Replace("<br>", null).Trim();
            return (true, last);
        }

        public string KillTitoEntry(int keySheet) => null;

        public string MoveEntry(string username, string pak, int key, int project_codeKey, int? task_nameKey, string role_name, string type_name, string paycode_name, Action<TimeSheetModel, Entry, KeyValuePair<int, Slip>> custom = null)
        {
            // find slip
            //var unpak = TimeModel.Unpak(pak);
            var keySlip = Rows.SelectMany(x => x.Slips, (r, s) => new { r, s, m = s.Value.Key == key ? 1 : r.GetPak(this, username, s.Key) == pak ? 2 : 0 }).OrderBy(x => x.m).SingleOrDefault(x => x.m > 0);
            if (keySlip == null) return Error_srcSlip;
            if (!keySlip.r.Slips.Remove(keySlip.s.Key)) throw new InvalidOperationException();
            // clone row
            var clone = (Entry)keySlip.r.Clone();
            // clone : project
            if (!Meta.Projects.TryGetValue(project_codeKey, out var project)) return Error_dstProject;
            clone.Project = project;
            // clone : task
            if (task_nameKey != null)
            {
                if (!project.Tasks.TryGetValue(task_nameKey.Value, out var task)) return Error_dstTask;
                clone.Task = task;
            }
            // clone : labcat
            if (role_name != null && project.LabcatsByName.TryGetValue(role_name, out var role))
                clone.Labcat = role;
            else if (clone.Labcat.key != null && !project.Labcats.ContainsKey(clone.Labcat.key.Value))
                clone.Labcat = project.Labcats.Values.First(x => x.value == "Other");
            // clone : project-type
            if (!string.IsNullOrEmpty(type_name) && Meta.ProjectTypesByName.TryGetValue(type_name, out var projectType))
                clone.ProjectType = projectType;
            else if (clone.ProjectType.key != project.ProjectType.key)
            {
                clone.ProjectType = project.ProjectType;
                clone.Paycode = project.Paycode;
            }
            // clone : paycode
            if (clone.ProjectType.value == "ADMIN")
                clone.Paycode = project.Paycode;
            else if (!string.IsNullOrEmpty(paycode_name) && project.PaycodesByName.TryGetValue(paycode_name, out var paycode))
                clone.Paycode = paycode;
            else if (clone.Paycode.key != null && !project.Paycodes.ContainsKey(clone.Paycode.key.Value))
                clone.Paycode = project.Paycode;
            // add slip
            var slip = keySlip.s; clone.Slips.Add(slip.Key, slip.Value);
            custom?.Invoke(this, clone, slip);
            // find or insert row
            var match0 = Rows.Where(x => x.Project.Key == clone.Project.Key && x.Task.key == clone.Task.key && x.Labcat.key == clone.Labcat.key).ToList();
            if (match0.Count == 0) { Rows.Add(clone); return null; }
            var match1 = match0.Where(x => x.ProjectType.key == clone.ProjectType.key).ToList();
            if (match1.Count == 0) { Rows.Add(clone); return null; }
            var match2 = match1.Where(x => x.Paycode.key == clone.Paycode.key).ToList();
            if (match2.Count == 0) { Rows.Add(clone); return null; }
            // add slip, exit
            var firstSlips = match2.First().Slips;
            if (!firstSlips.TryGetValue(slip.Key, out var existingSlip)) { firstSlips.Add(slip.Key, slip.Value); return null; }
            // merge slip in, error if tito
            if (existingSlip.Titos.Length != 0 || slip.Value.Titos.Length != 0) return Error_tito;
            // merge slip in
            existingSlip.Hours += slip.Value.Hours;
            existingSlip.Comments += "\n" + slip.Value.Comments;
            return null;
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
            var view = source.Contains("setSubsectionTitle('");
            //if (view && source.Contains("<td class=\"status\">LOCKED</td>"))
            //    status = SheetStatus.Locked;
            return view
                ? ParseView(source, keySheet, status)
                : ParseEdit(source, keySheet, status);
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
                IsEdit = false,
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
            var personString = title.ExtractSpanInner("Timesheet for", "(").Trim();
            var person = ((int?)null, personString);
            var week = DateTime.Parse(title.ExtractSpanInner("(", " ").Trim());
            // errors
            var errors = new List<string>();
            var error = source.ExtractSpanInner("<div class=\"error\">", "</div>");
            if (error != null)
            {
                errors.Add(error.ExtractSpanInner("<p class=\"main\">", "</p>")?.Trim());
                errors.Add(error.ExtractSpanInner("<p class=\"sub-main\">", "<br></p>")?.Trim());
            }
            // meta
            var meta = new Metadata
            {
                ProjectTypes = Regex.Matches(source.ExtractSpanInner("var projecttypes = [", "];"), @"key:([\d]+).*\('([^']*)'\)", RegexOptions.Multiline).Cast<Match>()
                    .Select(m => ((int?)int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1.Value),
                Paycodes = Regex.Matches(source.ExtractSpanInner("var paycodesByKey = new KeyedSet();", "var projects = [];"), @"put\(([\d]+).*\('([^']*)'\)").Cast<Match>()
                    .Select(m => ((int?)int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1.Value),
                LaborCategories = source.Contains("mLabCats = [];")
                    ? Regex.Matches(source.ExtractSpanInner("mLabCats = [];", "labCats = mLabCats;"), @"put\(([\d]+).*\('([^']*)'\)", RegexOptions.Multiline).Cast<Match>()
                    .Select(m => ((int?)int.Parse(m.Groups[1].Value), HttpUtility.UrlDecode(m.Groups[2].Value))).ToDictionary(x => x.Item1.Value)
                    : new Dictionary<int, (int?, string)>(),
                Dates = Regex.Matches(source.ExtractSpanInner("var dates = [];", "var titoWindow = null;"), @"Date\(([\d]+),([\d]+),([\d]+)\)", RegexOptions.Multiline).Cast<Match>()
                    .Select(m => new DateTime(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value) + 1, int.Parse(m.Groups[3].Value))).ToList(),
            };
            meta.ProjectTypesByName = meta.ProjectTypes.Values.ToDictionary(x => x.value);
            // projects
            int lastIdx = 0;
            var laborCategories = meta.LaborCategories;
            meta.Projects = Regex.Matches(source, @"Project\(([\d]+).*?\('([^']*)'\).*projecttypes\[([\d]+)\].*pcBK\[([\d]+)\]", RegexOptions.Multiline).Cast<Match>()
               .Select(m =>
               {
                   var startIdx = m.Captures[0].Index;
                   Dictionary<int, (int?, string)> labcats;
                   if (source.IndexOf("labCats = mLabCats;", lastIdx, startIdx - lastIdx) != -1)
                       labcats = meta.LaborCategories;
                   else
                   {
                       var labcats0 = Regex.Matches(source.ExtractSpanInner("pLabCats = [];", "labCats = pLabCats;", lastIdx, startIdx) ?? "", @"put\(([\d]+).*\('([^']*)'\)", RegexOptions.Multiline).Cast<Match>()
                            .Select(n => ((int?)int.Parse(n.Groups[1].Value), HttpUtility.UrlDecode(n.Groups[2].Value))).ToDictionary(x => x.Item1.Value);
                       var labcats1 = Regex.Matches(source.ExtractSpanInner("pLabCats = [];", "labCats = pLabCats;", lastIdx, startIdx) ?? "", @"get\(([\d]+)\)", RegexOptions.Multiline).Cast<Match>()
                            .Select(n => int.Parse(n.Groups[1].Value)).ToList();
                       labcats = labcats0;
                       foreach (var x in labcats0)
                           laborCategories.Add(x.Key, x.Value);
                       foreach (var x in labcats1)
                           if (laborCategories.TryGetValue(x, out var value))
                               labcats.Add(x, value);
                   }
                   var tasks = Regex.Matches(source.ExtractSpanInner("tasks = [];", "paycodes = [", lastIdx, startIdx) ?? "", @"Task\(([\d]+).*\('([^']*)'\)", RegexOptions.Multiline).Cast<Match>()
                        .Select(n => ((int?)int.Parse(n.Groups[1].Value), HttpUtility.UrlDecode(n.Groups[2].Value))).ToDictionary(x => x.Item1.Value);
                   var paycodes = Regex.Matches(source.ExtractSpanInner("paycodes = [", "];", lastIdx, startIdx) ?? "", @"pcBK\[([\d]+)\]", RegexOptions.Multiline).Cast<Match>()
                       .Select(n => (int?)int.Parse(n.Groups[1].Value)).ToList()
                       .Select(x => meta.Paycodes[x.Value]).ToDictionary(x => x.Item1.Value);
                   lastIdx = startIdx;
                   return new Project
                   {
                       Key = int.Parse(m.Groups[1].Value),
                       Name = HttpUtility.UrlDecode(m.Groups[2].Value),
                       Labcats = labcats,
                       LabcatsByName = labcats.Values.ToDictionary(x => x.Item2),
                       Tasks = tasks,
                       ProjectType = meta.ProjectTypes.Values.ElementAt(int.Parse(m.Groups[3].Value)),
                       Paycodes = paycodes,
                       PaycodesByName = paycodes.Values.ToDictionary(x => x.Item2),
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
                                        Comments = !string.IsNullOrEmpty(o.Groups[17].Value) ? HttpUtility.UrlDecode(o.Groups[17].Value) : null,
                                    }).ToArray();
                             return (int.Parse(n.Groups[1].Value), new Slip
                             {
                                 Key = int.Parse(n.Groups[2].Value),
                                 Hours = decimal.Parse(n.Groups[3].Value),
                                 Comments = !string.IsNullOrEmpty(n.Groups[4].Value) ? HttpUtility.UrlDecode(n.Groups[4].Value) : null,
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
                IsEdit = true,
                KeySheet = keySheet,
                Person = person,
                Week = week,
                Status = status,
                Meta = meta,
                Rows = rows,
                Errors = errors.Where(x => !string.IsNullOrEmpty(x)).ToArray(),
            };
        }
    }
}