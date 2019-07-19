using ExcelTrans.Services;
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
        public string project_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder) =>
            Task.Run(() => una.GetEntitiesByExport(una.Exports["assignment"].Item1, f =>
              {
                  f.Checked["suppressOutput"] = true;
                  f.Checked["person_outputActive"] = true; f.Checked["person_outputInactive"] = true;
                  f.Values["drange_bDate"] = "BOT"; f.Values["drRange_eDate"] = "EOT";
                  f.Values["drange"] = "bot_eot";
                  f.Checked["organizationAssignment"] = true;
                  f.Checked["projectAssignment"] = false;
                  f.Checked["taskAssignment"] = false;
              }, sourceFolder));

        public static IEnumerable<AssignmentModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["assignment"].Item2}.csv");
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new AssignmentModel
                {
                    project_org_code = x[0],
                    project_code = x[1],
                    task_name = x[2],
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
                    // custom
                    assignment_type = x.Count > 20 ? x[20] : null,
                    assign_code = x.Count > 21 ? x[21] : null,
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
            Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Assignment1 : AssignmentModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_Assignment1 s, out string last)
        {
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.AssignmentChanged;
            Unanet.OrganizationLookup.CostCenters.TryGetValue(s.assign_code, out var assign_codeKey);
            var method = !cf.Contains("delete") ? add ? HttpMethod.Post : HttpMethod.Put : HttpMethod.Delete;
            var r = una.SubmitSubManage("E", add ? HttpMethod.Post : HttpMethod.Put, $"projects/orgs",
                $"key={assign_codeKey}", $"projectkey={s.project_codeKey}", null,
                out last, (z, f) =>
                {
                    if (cf.Contains("insert")) f.Values["assign"] = assign_codeKey ?? throw new System.ArgumentOutOfRangeException(nameof(s.assign_code), s.assign_code); // LOOKUP
                    f.Add("button_save", "action", null);
                });
            return r != null ?
                ManageFlags.AssignmentChanged :
                ManageFlags.None;
        }
    }
}
