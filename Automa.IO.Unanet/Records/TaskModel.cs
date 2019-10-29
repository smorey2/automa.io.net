using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class TaskModel : ModelBase
    {
        public string key { get; set; }
        //
        public string project_org_code { get; set; }
        public string project_code { get; set; }
        public string task_name { get; set; }
        public string active { get; set; }
        //
        public DateTime? original_start_date { get; set; }
        public DateTime? original_end_date { get; set; }
        public DateTime? revised_start_date { get; set; }
        public DateTime? revised_end_date { get; set; }
        public DateTime? completed_date { get; set; }
        //
        public string percent_complete { get; set; }
        public string status { get; set; }
        public string output { get; set; }
        public string external_system_code { get; set; }
        public string budget_hours { get; set; }
        public string etc_hours { get; set; }
        public string est_tot_hours { get; set; }
        //
        public string budget_labor_dollars_bill { get; set; }
        public string etc_labor_dollars_bill { get; set; }
        public string est_tot_labor_dollars_bill { get; set; }
        public string budget_expense_dollars_bill { get; set; }
        public string etc_expense_dollars_bill { get; set; }
        public string est_tot_expense_dollars_bill { get; set; }
        //
        public string user01 { get; set; }
        public string user02 { get; set; }
        public string user03 { get; set; }
        public string user04 { get; set; }
        public string user05 { get; set; }
        public string user06 { get; set; }
        public string user07 { get; set; }
        public string user08 { get; set; }
        public string user09 { get; set; }
        public string user10 { get; set; }
        //
        public string budget_labor_dollars_cost { get; set; }
        public string etc_labor_dollars_cost { get; set; }
        public string est_tot_labor_dollars_cost { get; set; }
        public string budget_expense_dollars_cost { get; set; }
        public string etc_expense_dollars_cost { get; set; }
        public string est_tot_expense_dollars_cost { get; set; }
        //
        public string project_type { get; set; }
        public string delete { get; set; }
        public string duration { get; set; }
        public string enable_alerts { get; set; }
        public string billing_type { get; set; }
        //
        public string budget_labor_dollars_cost_burdened { get; set; }
        public string budget_expense_dollars_cost_burdened { get; set; }
        public string funded_value { get; set; }
        public string limit_bill_to_funded { get; set; }
        public string limit_rev_to_funded { get; set; }
        public string owning_organization { get; set; }
        // custom
        public string project_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = "75-00-DEG-00 - Digital Evolution Group, LLC")
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["task"].Item2}.csv");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Exports["task"].Item1, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity);
            }, sourceFolder));
        }

        public static IEnumerable<TaskModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["task"].Item2}.csv");
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new TaskModel
                {
                    key = x[0],
                    //
                    project_org_code = x[1],
                    project_code = x[2],
                    task_name = x[3].DecodeString(),
                    active = x[4],
                    //
                    original_start_date = x[5].ToDateTime(),
                    original_end_date = x[6].ToDateTime(),
                    revised_start_date = x[7].ToDateTime("BOT"),
                    revised_end_date = x[8].ToDateTime("EOT"),
                    completed_date = x[9].ToDateTime(),
                    //
                    percent_complete = x[10],
                    status = x[11],
                    output = x[12],
                    external_system_code = x[13],
                    budget_hours = x[14],
                    etc_hours = x[15],
                    est_tot_hours = x[16],
                    //
                    budget_labor_dollars_bill = x[17],
                    etc_labor_dollars_bill = x[18],
                    est_tot_labor_dollars_bill = x[19],
                    budget_expense_dollars_bill = x[20],
                    etc_expense_dollars_bill = x[21],
                    est_tot_expense_dollars_bill = x[22],
                    //
                    user01 = x[23],
                    user02 = x[24],
                    user03 = x[25],
                    user04 = x[26],
                    user05 = x[27],
                    user06 = x[28],
                    user07 = x[29],
                    user08 = x[30],
                    user09 = x[31],
                    user10 = x[32],
                    //
                    budget_labor_dollars_cost = x[33],
                    etc_labor_dollars_cost = x[34],
                    est_tot_labor_dollars_cost = x[35],
                    budget_expense_dollars_cost = x[36],
                    etc_expense_dollars_cost = x[37],
                    est_tot_expense_dollars_cost = x[38],
                    //
                    project_type = x[39],
                    delete = x[40],
                    duration = x[41],
                    enable_alerts = x[42],
                    billing_type = x[43],
                    //
                    budget_labor_dollars_cost_burdened = x[44],
                    budget_expense_dollars_cost_burdened = x[45],
                    funded_value = x[46],
                    limit_bill_to_funded = x[47],
                    limit_rev_to_funded = x[48],
                    owning_organization = x[49],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("tn", x.task_name), XAttribute("a", x.active),
                XAttribute("osd", x.original_start_date), XAttribute("oed", x.original_end_date), XAttribute("rsd", x.revised_start_date), XAttribute("red", x.revised_end_date), XAttribute("cd", x.completed_date),
                XAttribute("pc2", x.percent_complete), XAttribute("s", x.status), XAttribute("o", x.output), XAttribute("esc", x.external_system_code), XAttribute("bh", x.budget_hours), XAttribute("eh", x.etc_hours), XAttribute("eth", x.est_tot_hours),
                XAttribute("bldb", x.budget_labor_dollars_bill), XAttribute("eldb", x.etc_labor_dollars_bill), XAttribute("etldb", x.est_tot_labor_dollars_bill), XAttribute("bedb", x.budget_expense_dollars_bill), XAttribute("eedb", x.etc_expense_dollars_bill), XAttribute("etedb", x.est_tot_expense_dollars_bill),
                XAttribute("u1", x.user01), XAttribute("u2", x.user02), XAttribute("u3", x.user03), XAttribute("u4", x.user04), XAttribute("u5", x.user05), XAttribute("u6", x.user06), XAttribute("u7", x.user07), XAttribute("u8", x.user08), XAttribute("u9", x.user09), XAttribute("u10", x.user10),
                XAttribute("bldc", x.budget_labor_dollars_cost), XAttribute("eldc", x.etc_labor_dollars_cost), XAttribute("etldc", x.est_tot_labor_dollars_cost), XAttribute("bedc", x.budget_expense_dollars_cost), XAttribute("eedc", x.etc_expense_dollars_cost), XAttribute("etedc", x.est_tot_expense_dollars_cost),
                XAttribute("pt", x.project_type), XAttribute("d", x.duration), XAttribute("ea", x.enable_alerts), XAttribute("bt", x.billing_type),
                XAttribute("bldcb", x.budget_labor_dollars_cost_burdened), XAttribute("bedcb", x.budget_expense_dollars_cost_burdened), XAttribute("fv", x.funded_value), XAttribute("lbtf", x.limit_bill_to_funded), XAttribute("lrtf", x.limit_rev_to_funded), XAttribute("oo", x.owning_organization)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".j_t.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Task1 : TaskModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_Task1 s, out string last)
        {
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out last))
                return ManageFlags.TaskChanged;
            var organizations = Unanet.Lookups.CostCenters.Value;
            var r = una.SubmitSubManage("B", add ? HttpMethod.Post : HttpMethod.Put, "task", add ? "key=0&insertPos=1" : $"key={s.key}&insertPos=0",
                $"projectkey={s.project_codeKey}", null,
                out last, (z, f) =>
            {
                //if (add || cf.Contains("poc")) f.Values["xxxx"] = s.project_org_code;
                //if (add || cf.Contains("pc")) f.Values["xxxx"] = s.project_code;
                if (add || cf.Contains("tn")) f.Values["taskName"] = s.task_name;
                if (add || cf.Contains("a")) f.Checked["active"] = s.active == "Y";
                //
                if (add || cf.Contains("osd")) f.Values["orig_start_date"] = s.original_start_date.FromDateTime();
                if (add || cf.Contains("oed")) f.Values["orig_end_date"] = s.original_end_date.FromDateTime();
                if (add || cf.Contains("rsd")) f.Values["rev_start_date"] = s.revised_start_date.FromDateTime("BOT");
                if (add || cf.Contains("red")) f.Values["rev_end_date"] = s.revised_end_date.FromDateTime("EOT");
                if (add || cf.Contains("cd")) f.Values["completed"] = s.completed_date.FromDateTime();
                //
                if (add || cf.Contains("pc2")) f.Values["percentComplete"] = s.percent_complete;
                if (add || cf.Contains("s")) f.Values["status"] = s.status;
                if (add || cf.Contains("o")) f.Values["output"] = s.output;
                if (add || cf.Contains("esc") || cf.Contains("bind")) f.Values["externalSystemCode"] = s.external_system_code;
                if (add || cf.Contains("bh")) f.Values["hours_budget"] = s.budget_hours;
                if (add || cf.Contains("eh")) f.Values["hours_etc"] = s.etc_hours;
                if (add || cf.Contains("eth")) f.Values["hours_tot"] = s.est_tot_hours;
                //
                if (add || cf.Contains("bldb")) f.Values["labor_budget_bill"] = s.budget_labor_dollars_bill;
                if (add || cf.Contains("eldb")) f.Values["labor_etc_bill"] = s.etc_labor_dollars_bill;
                if (add || cf.Contains("etldb")) f.Values["labor_tot_bill"] = s.est_tot_labor_dollars_bill;
                if (add || cf.Contains("bedb")) f.Values["expense_budget_bill"] = s.budget_expense_dollars_bill;
                if (add || cf.Contains("eedb")) f.Values["expense_etc_bill"] = s.etc_expense_dollars_bill;
                if (add || cf.Contains("etedb")) f.Values["expense_tot_bill"] = s.est_tot_expense_dollars_bill;
                //
                //if (add || cf.Contains("u1")) f.Values["udf_0"] = s.user01;
                //if (add || cf.Contains("u2")) f.Values["udf_1"] = s.user02;
                //if (add || cf.Contains("u3")) f.Values["udf_2"] = s.user03;
                //if (add || cf.Contains("u4")) f.Values["udf_3"] = s.user04;
                //if (add || cf.Contains("u5")) f.Values["udf_4"] = s.user05;
                //if (add || cf.Contains("u6")) f.Values["udf_5"] = s.user06;
                //if (add || cf.Contains("u7")) f.Values["udf_6"] = s.user07;
                if (add || cf.Contains("u8")) f.FromSelect("udf_7", s.user08);
                //if (add || cf.Contains("u9")) f.Values["udf_8"] = s.user09;
                //if (add || cf.Contains("u10")) f.Values["udf_9"] = s.user10;
                //
                if (add || cf.Contains("bldc")) f.Values["labor_budget_cost"] = s.budget_labor_dollars_cost;
                if (add || cf.Contains("eldc")) f.Values["labor_etc_cost"] = s.etc_labor_dollars_cost;
                if (add || cf.Contains("etldc")) f.Values["labor_tot_cost"] = s.est_tot_labor_dollars_cost;
                if (add || cf.Contains("bedc")) f.Values["expense_budget_cost"] = s.budget_expense_dollars_cost;
                if (add || cf.Contains("eedc")) f.Values["expense_etc_cost"] = s.etc_expense_dollars_cost;
                if (add || cf.Contains("etedc")) f.Values["expense_tot_cost"] = s.est_tot_expense_dollars_cost;
                //
                if (add || cf.Contains("pt")) f.FromSelect("pjtType", s.project_type);
                if (add || cf.Contains("d")) f.Values["duration"] = s.duration;
                if (add || cf.Contains("ea")) f.Checked["alertable"] = s.enable_alerts == "Y";
                if (add || cf.Contains("bt")) f.FromSelect("billType", s.billing_type);
                //
                if (add || cf.Contains("bldcb")) f.Values["labor_burden_cost"] = s.budget_labor_dollars_cost_burdened;
                if (add || cf.Contains("bedcb")) f.Values["expense_burden_cost"] = s.budget_expense_dollars_cost_burdened;
                if (add || cf.Contains("fv")) f.Values["funded_value"] = s.funded_value;
                //if (add || cf.Contains("lbtf")) f.Checked["limitBillToFunded"] = s.limit_bill_to_funded == "Y";
                //if (add || cf.Contains("lrtf")) f.Checked["limitRevToFunded"] = s.limit_rev_to_funded == "Y";
                if (add || cf.Contains("oo")) f.Values["tOOMenu"] = GetLookupValue(organizations, s.owning_organization);
                return f.ToString();
            });
            return r != null ?
                ManageFlags.TaskChanged :
                ManageFlags.None;
        }
    }
}