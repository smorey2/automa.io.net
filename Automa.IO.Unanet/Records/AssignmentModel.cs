using ExcelTrans.Services;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class AssignmentModel : ModelBase
    {
        public string project_org_code { get; set; }
        public string project_code { get; set; }
        public string task_name { get; set; }
        public string username { get; set; }
        public string begin_date { get; set; }
        public string end_date { get; set; }
        public string delete { get; set; }
        //
        public string budget_hours { get; set; }
        public string exceed_budget { get; set; }
        public string bill_rate { get; set; }
        public string cost_rate { get; set; }
        public string project_org_override { get; set; }
        public string person_org_override { get; set; }
        //
        public string labor_category { get; set; }
        public string location { get; set; }
        public string etc_hours { get; set; }
        public string use_wbs_dates { get; set; }
        public string cost_structure { get; set; }
        public string cost_element { get; set; }
        public string edc { get; set; }
        // custom
        public string assignment_type { get; set; }
        public string assign_code { get; set; }
        public string assign { get; set; }
        public string project_codeKey { get; set; }
        public string usernameKey { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.assignment.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.assignment.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.Checked["person_outputActive"] = true; f.Checked["person_outputInactive"] = true;
                f.Values["drange_bDate"] = "BOT"; f.Values["drRange_eDate"] = "EOT";
                f.Values["drange"] = "bot_eot";
                f.Checked["organizationAssignment"] = true;
                f.Checked["projectAssignment"] = true;
                f.Checked["taskAssignment"] = false;
                return null;
            }, sourceFolder));
        }

        public static IEnumerable<AssignmentModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.assignment.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new AssignmentModel
                {
                    project_org_code = x[0],
                    project_code = x[1],
                    task_name = x[2].DecodeString(),
                    username = x[3],
                    begin_date = x[4],
                    end_date = x[5],
                    delete = x[6],
                    //
                    budget_hours = x[7],
                    exceed_budget = x[8],
                    bill_rate = x[9],
                    cost_rate = x[10],
                    project_org_override = x[11],
                    person_org_override = x[12],
                    //
                    labor_category = x[13],
                    location = x[14],
                    etc_hours = x[15],
                    use_wbs_dates = x[16],
                    cost_structure = x[17],
                    cost_element = x[18],
                    edc = x[19],
                    //
                    assignment_type = x[20],
                    assign_code = x[21],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("tn", x.task_name), XAttribute("u", x.username), XAttribute("bd", x.begin_date), XAttribute("ed", x.end_date),
                XAttribute("bh", x.budget_hours), XAttribute("eb", x.exceed_budget), XAttribute("br", x.bill_rate), XAttribute("cr", x.cost_rate), XAttribute("joo", x.project_org_override), XAttribute("poo", x.person_org_override),
                XAttribute("lc", x.labor_category), XAttribute("l", x.location), XAttribute("eh", x.etc_hours), XAttribute("uwd", x.use_wbs_dates), XAttribute("cs", x.cost_structure), XAttribute("ce", x.cost_element), XAttribute("e", x.edc),
                XAttribute("at", x.assignment_type), XAttribute("ac", x.assign_code)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".j_a.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Assignment1 : AssignmentModel
        {
            public string XA { get; set; }
            public string XCF { get; set; }
        }

        public static async Task<(ChangedFields changed, string last)> ManageRecordAsync(UnanetClient una, p_Assignment1 s, Action<p_Assignment1> bespoke = null)
        {
            var _ = new ChangedFields(ManageFlags.AssignmentChanged);
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out var last2, canDelete: true))
                return (_.Changed(), last2);
            var method = !cf.Contains("delete") ? add ? HttpMethod.Post : HttpMethod.Put : HttpMethod.Delete;
            switch (s.assignment_type)
            {
                case "1":
                    {
                        if (!Unanet.Lookups.TryGetCostCentersAndDefault(s.assign, out var assignKey))
                            throw new InvalidOperationException($"unable to find org {s.assign}");
                        var (r, last) = await una.SubmitSubManageAsync("E", method, $"projects/orgs", $"key={assignKey}&nextKey=0",
                            $"projectkey={s.project_codeKey}", null,
                            (z, f) =>
                            {
                                if (add)
                                {
                                    f.Values["assign"] = assignKey ?? throw new ArgumentOutOfRangeException(nameof(s.assign), s.assign); // LOOKUP
                                    f.Add("button_save", "action", null);
                                }
                                return f.ToString();
                            }, formOptions: new HtmlFormOptions { ParseOptions = false }).ConfigureAwait(false);
                        return (_.Changed(r), last);
                    }
                case "2":
                    {
                        var (r, last) = await una.SubmitSubManageAsync(add ? "E" : "F", method, $"projects/assignment", null,
                            $"projectkey={s.project_codeKey}", "savedCriteria=&person_mod=false&personClass=com.unanet.page.projects.ScheduledPeopleMenu%24ScheduleListPeopleMenu&person_dbValue=&person_personOrgCode_fltr=&person_lastname_fltr=&person_outputActive=true&location_mod=false&locationClass=com.unanet.page.criteria.FilteredLocationMenu&location_dbValue=&location_location_fltr=&dateRange_bDate=BOT&dateRange_eDate=EOT&dateRange=bot_eot&unit=HOUR&showEstimates=true&savedListName=&criteriaClass=com.unanet.page.projects.AssignmentListCriteria&loadValues=true&restore=false&list=true",
                            (z, f) =>
                            {
                                if (add)
                                {
                                    f.Values["assign"] = s.usernameKey ?? throw new ArgumentNullException(nameof(s.usernameKey));
                                    f.Values["costStructLabor"] = "-1";
                                    f.Values["beginDate"] = "BOT"; f.Types["beginDate"] = "text";
                                    f.Values["endDate"] = "EOT"; f.Types["endDate"] = "text";
                                    f.Checked["useWbsDates"] = false;
                                    f.Add("button_save", "action", null);
                                    return f.ToString();
                                }
                                else if (cf.Contains("delete"))
                                {
                                    var doc = z.ToHtmlDocument();
                                    var rows = doc.DocumentNode.Descendants("tr")
                                        .Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value.StartsWith("r"))
                                        .ToDictionary(
                                            x => x.Attributes["id"].Value.Substring(1).Trim(),
                                            x => x.Descendants("td").ToArray());
                                    var row = rows.SingleOrDefault(x => x.Value[3].InnerText.Contains($"({s.assign.ToLowerInvariant()})"));
                                    var key = row.Key ?? throw new ArgumentNullException(nameof(row.Key));
                                    var keysListed = string.Join(",", rows.Keys);
                                    return $"projectkey={s.project_codeKey}&restore=true&key={key}&keysListed={keysListed}";
                                }
                                return null;
                            }).ConfigureAwait(false);
                        return (_.Changed(r), last);
                    }
                default: throw new ArgumentOutOfRangeException(nameof(s.assignment_type), s.assignment_type);
            }
        }
    }
}
