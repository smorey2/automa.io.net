using ExcelTrans.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class ProjectAdministratorModel : ModelBase
    {
        public string project_org_code { get; set; }
        public string project_code { get; set; }
        public string username { get; set; }
        public string role { get; set; }
        public string primary_ind { get; set; }
        public string delete { get; set; }
        //
        public string approve_time { get; set; }
        public string approve_expense { get; set; }
        public string customer_approves_first { get; set; }
        // new
        public string pm_approves_before_mgr { get; set; }
        public string approval_type { get; set; }
        // custom
        public string project_codeKey { get; set; }
        public string usernameKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.project_administrator.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.project_administrator.key, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity ?? una.Settings.LegalEntity);
            }, sourceFolder));
        }

        public static IEnumerable<ProjectAdministratorModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.project_administrator.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new ProjectAdministratorModel
                {
                    project_org_code = x[0],
                    project_code = x[1],
                    username = x[2],
                    role = x[3],
                    primary_ind = x[4],
                    delete = x[5],
                    //
                    approve_time = x[6],
                    approve_expense = x[7],
                    customer_approves_first = x[8],
                    // NEW
                    pm_approves_before_mgr = x[9],
                    approval_type = x[10],
                }, 1)
                .Where(x => x.role == "billingManager" || x.role == "projectManager" || x.role == "projectApprover")
                .ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("u", x.username), XAttribute("r", x.role), XAttribute("pi", x.primary_ind), XAttribute("at", x.approve_time), XAttribute("ae", x.approve_expense), XAttribute("caf", x.customer_approves_first),
                XAttribute("pabm", x.pm_approves_before_mgr), XAttribute("at2", x.approval_type)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".j_pa.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_ProjectAdministrator1 : ProjectAdministratorModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_ProjectAdministrator1 s, out string last)
        {
            var canDelete = s.role != "projectManager";
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last, canDelete: canDelete))
                return ManageFlags.ProjectAdministratorChanged;
            var method = !cf.Contains("delete") ? add ? HttpMethod.Post : HttpMethod.Put : HttpMethod.Delete;
            var r = una.SubmitSubManage("D", HttpMethod.Get, $"projects/controllers/{s.role}", null, //: POST
                $"projectkey={s.project_codeKey}", null,
                out last, (z, f) =>
            {
                if (s.role == "projectApprover")
                {
                    f.Checked["approverApproveFirst"] = s.pm_approves_before_mgr == "Y";
                    if (method != HttpMethod.Delete) f.Values["primaryAssigned"] = $"*{s.usernameKey};2";
                    else if (method == HttpMethod.Delete) f.Values["primaryNotAssigned"] = "-1";
                }
                else
                {
                    if (method != HttpMethod.Delete) f.Values["primary"] = s.usernameKey;
                    else if (method == HttpMethod.Delete) f.Values["primary"] = "-1";
                }
                return f.ToString();
            });
            return r != null ?
                ManageFlags.ProjectAdministratorChanged :
                ManageFlags.None;
        }
    }
}