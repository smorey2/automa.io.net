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
        //
        public string allows_time { get; set; }
        public string ts_non_emp_po_required { get; set; }
        public string allows_expense { get; set; }
        public string exp_non_emp_po_required { get; set; }
        public string allows_item { get; set; }
        //
        public string user11 { get; set; }
        public string user12 { get; set; }
        public string user13 { get; set; }
        public string user14 { get; set; }
        public string user15 { get; set; }
        public string user16 { get; set; }
        public string user17 { get; set; }
        public string user18 { get; set; }
        public string user19 { get; set; }
        public string user20 { get; set; }
        // custom
        public string key { get; set; }
        public string project_codeKey { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.task.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.task.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity ?? una.Options.LegalEntity);
                return null;
            }, sourceFolder));
        }

        public static IEnumerable<TaskModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.task.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new TaskModel
                {
                    project_org_code = x[0],
                    project_code = x[1],
                    task_name = x[2].DecodeString(),
                    active = x[3],
                    //
                    original_start_date = x[4].ToDateTime(),
                    original_end_date = x[5].ToDateTime(),
                    revised_start_date = x[6].ToDateTime("BOT"),
                    revised_end_date = x[7].ToDateTime("EOT"),
                    completed_date = x[8].ToDateTime(),
                    //
                    percent_complete = x[9],
                    status = x[10],
                    output = x[11],
                    external_system_code = x[12],
                    budget_hours = x[13],
                    etc_hours = x[14],
                    est_tot_hours = x[15],
                    //
                    budget_labor_dollars_bill = x[16],
                    etc_labor_dollars_bill = x[17],
                    est_tot_labor_dollars_bill = x[18],
                    budget_expense_dollars_bill = x[19],
                    etc_expense_dollars_bill = x[20],
                    est_tot_expense_dollars_bill = x[21],
                    //
                    user01 = x[22],
                    user02 = x[23],
                    user03 = x[24],
                    user04 = x[25],
                    user05 = x[26],
                    user06 = x[27],
                    user07 = x[28],
                    user08 = x[29],
                    user09 = x[30],
                    user10 = x[31],
                    //
                    budget_labor_dollars_cost = x[32],
                    etc_labor_dollars_cost = x[33],
                    est_tot_labor_dollars_cost = x[34],
                    budget_expense_dollars_cost = x[35],
                    etc_expense_dollars_cost = x[36],
                    est_tot_expense_dollars_cost = x[37],
                    //
                    project_type = x[38],
                    delete = x[39],
                    duration = x[40],
                    enable_alerts = x[41],
                    billing_type = x[42],
                    //
                    budget_labor_dollars_cost_burdened = x[43],
                    budget_expense_dollars_cost_burdened = x[44],
                    funded_value = x[45],
                    limit_bill_to_funded = x[46],
                    limit_rev_to_funded = x[47],
                    owning_organization = x[48],
                    //
                    allows_time = x[49],
                    ts_non_emp_po_required = x[50],
                    allows_expense = x[51],
                    exp_non_emp_po_required = x[52],
                    allows_item = x[53],
                    //
                    user11 = x[54],
                    user12 = x[55],
                    user13 = x[56],
                    user14 = x[57],
                    user15 = x[58],
                    user16 = x[59],
                    user17 = x[60],
                    user18 = x[61],
                    user19 = x[62],
                    user20 = x[63],
                    //
                    key = x[64],
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
                XAttribute("bldcb", x.budget_labor_dollars_cost_burdened), XAttribute("bedcb", x.budget_expense_dollars_cost_burdened), XAttribute("fv", x.funded_value), XAttribute("lbtf", x.limit_bill_to_funded), XAttribute("lrtf", x.limit_rev_to_funded), XAttribute("oo", x.owning_organization),
                XAttribute("at", x.allows_time), XAttribute("tnepr", x.ts_non_emp_po_required), XAttribute("ae", x.allows_expense), XAttribute("enepr", x.exp_non_emp_po_required), XAttribute("ai", x.allows_item),
                XAttribute("u11", x.user11), XAttribute("u12", x.user12), XAttribute("u13", x.user13), XAttribute("u14", x.user14), XAttribute("u15", x.user15), XAttribute("u16", x.user16), XAttribute("u17", x.user17), XAttribute("u18", x.user18), XAttribute("u19", x.user19), XAttribute("u20", x.user20)
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

        public static async Task<(ChangedFields changed, string last)> ManageRecordAsync(UnanetClient una, p_Task1 s,  Action<p_Task1> bespoke = null)
        {
            var _ = new ChangedFields(ManageFlags.TaskChanged);
            bespoke?.Invoke(s);
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out var last2))
                return (_.Changed(), last2);
            var organizations = Unanet.Lookups.CostCenters.Value;
            var method = add ? HttpMethod.Post : HttpMethod.Put;
            var (r, last) = await una.SubmitSubManageAsync("B", method, "task", add ? "key=0&insertPos=1" : $"key={s.key}&insertPos=0",
                $"projectkey={s.project_codeKey}", null,
                (z, f) =>
            {
                //if (add || cf.Contains("poc")) f.Values["xxxx"] = _._(s.project_org_code, nameof(s.project_org_code));
                //if (add || cf.Contains("pc")) f.Values["xxxx"] = _._(s.project_code, nameof(s.project_code));
                if (add || cf.Contains("tn")) f.Values["taskName"] = _._(s.task_name, nameof(s.task_name));
                if (add || cf.Contains("a")) f.Checked["active"] = _._(s.active, nameof(s.active)) == "Y";
                //
                if (add || cf.Contains("osd")) f.Values["orig_start_date"] = _._(s.original_start_date, nameof(s.original_start_date)).FromDateTime();
                if (add || cf.Contains("oed")) f.Values["orig_end_date"] = _._(s.original_end_date, nameof(s.original_end_date)).FromDateTime();
                if (add || cf.Contains("rsd")) f.Values["rev_start_date"] = _._(s.revised_start_date, nameof(s.revised_start_date)).FromDateTime("BOT");
                if (add || cf.Contains("red")) f.Values["rev_end_date"] = _._(s.revised_end_date, nameof(s.revised_end_date)).FromDateTime("EOT");
                if (add || cf.Contains("cd")) f.Values["completed"] = _._(s.completed_date, nameof(s.completed_date)).FromDateTime();
                //
                if (add || cf.Contains("pc2")) f.Values["percentComplete"] = _._(s.percent_complete, nameof(s.percent_complete));
                if (add || cf.Contains("s")) f.Values["status"] = _._(s.status, nameof(s.status));
                if (add || cf.Contains("o")) f.Values["output"] = _._(s.output, nameof(s.output));
                if (add || cf.Contains("esc") || cf.Contains("bind")) f.Values["externalSystemCode"] = _._(s.external_system_code, nameof(s.external_system_code));
                if (add || cf.Contains("bh")) f.Values["hours_budget"] = _._(s.budget_hours, nameof(s.budget_hours));
                if (add || cf.Contains("eh")) f.Values["hours_etc"] = _._(s.etc_hours, nameof(s.etc_hours));
                if (add || cf.Contains("eth")) f.Values["hours_tot"] = _._(s.est_tot_hours, nameof(s.est_tot_hours));
                //
                if (add || cf.Contains("bldb")) f.Values["labor_budget_bill"] = _._(s.budget_labor_dollars_bill, nameof(s.budget_labor_dollars_bill));
                if (add || cf.Contains("eldb")) f.Values["labor_etc_bill"] = _._(s.etc_labor_dollars_bill, nameof(s.etc_labor_dollars_bill));
                if (add || cf.Contains("etldb")) f.Values["labor_tot_bill"] = _._(s.est_tot_labor_dollars_bill, nameof(s.est_tot_labor_dollars_bill));
                if (add || cf.Contains("bedb")) f.Values["expense_budget_bill"] = _._(s.budget_expense_dollars_bill, nameof(s.budget_expense_dollars_bill));
                if (add || cf.Contains("eedb")) f.Values["expense_etc_bill"] = _._(s.etc_expense_dollars_bill, nameof(s.etc_expense_dollars_bill));
                if (add || cf.Contains("etedb")) f.Values["expense_tot_bill"] = _._(s.est_tot_expense_dollars_bill, nameof(s.est_tot_expense_dollars_bill));
                //
                //if (add || cf.Contains("u1")) f.Values["udf_0"] = _._(s.user01, nameof(s.user01));
                //if (add || cf.Contains("u2")) f.Values["udf_1"] = _._(s.user02, nameof(s.user02));
                //if (add || cf.Contains("u3")) f.Values["udf_2"] = _._(s.user03, nameof(s.user03));
                //if (add || cf.Contains("u4")) f.Values["udf_3"] = _._(s.user04, nameof(s.user04));
                //if (add || cf.Contains("u5")) f.Values["udf_4"] = _._(s.user05, nameof(s.user05));
                //if (add || cf.Contains("u6")) f.Values["udf_5"] = _._(s.user06, nameof(s.user06));
                //if (add || cf.Contains("u7")) f.Values["udf_6"] = _._(s.user07, nameof(s.user07));
                if (add || cf.Contains("u8")) f.FromSelect("udf_7", _._(s.user08, nameof(s.user08)));
                //if (add || cf.Contains("u9")) f.Values["udf_8"] = _._(s.user09, nameof(s.user09));
                //if (add || cf.Contains("u10")) f.Values["udf_9"] = _._(s.user10, nameof(s.user10));
                //
                if (add || cf.Contains("bldc")) f.Values["labor_budget_cost"] = _._(s.budget_labor_dollars_cost, nameof(s.budget_labor_dollars_cost));
                if (add || cf.Contains("eldc")) f.Values["labor_etc_cost"] = _._(s.etc_labor_dollars_cost, nameof(s.etc_labor_dollars_cost));
                if (add || cf.Contains("etldc")) f.Values["labor_tot_cost"] = _._(s.est_tot_labor_dollars_cost, nameof(s.est_tot_labor_dollars_cost));
                if (add || cf.Contains("bedc")) f.Values["expense_budget_cost"] = _._(s.budget_expense_dollars_cost, nameof(s.budget_expense_dollars_cost));
                if (add || cf.Contains("eedc")) f.Values["expense_etc_cost"] = _._(s.etc_expense_dollars_cost, nameof(s.etc_expense_dollars_cost));
                if (add || cf.Contains("etedc")) f.Values["expense_tot_cost"] = _._(s.est_tot_expense_dollars_cost, nameof(s.est_tot_expense_dollars_cost));
                //
                if (add || cf.Contains("pt")) f.FromSelect("pjtType", _._(s.project_type, nameof(s.project_type)));
                if (add || cf.Contains("d")) f.Values["duration"] = _._(s.duration, nameof(s.duration));
                if (add || cf.Contains("ea")) f.Checked["alertable"] = _._(s.enable_alerts, nameof(s.enable_alerts)) == "Y";
                if (add || cf.Contains("bt")) f.FromSelect("billType", _._(s.billing_type, nameof(s.billing_type)));
                //
                if (add || cf.Contains("bldcb")) f.Values["labor_burden_cost"] = _._(s.budget_labor_dollars_cost_burdened, nameof(s.budget_labor_dollars_cost_burdened));
                if (add || cf.Contains("bedcb")) f.Values["expense_burden_cost"] = _._(s.budget_expense_dollars_cost_burdened, nameof(s.budget_expense_dollars_cost_burdened));
                if (add || cf.Contains("fv")) f.Values["funded_value"] = _._(s.funded_value, nameof(s.funded_value));
                //if (add || cf.Contains("lbtf")) f.Checked["limitBillToFunded"] = _._(s.limit_bill_to_funded, nameof(s.limit_bill_to_funded)) == "Y";
                //if (add || cf.Contains("lrtf")) f.Checked["limitRevToFunded"] = _._(s.limit_rev_to_funded, nameof(s.limit_rev_to_funded)) == "Y";
                if (add || cf.Contains("oo")) f.Values["tOOMenu"] = GetLookupValue(organizations, _._(s.owning_organization, nameof(s.owning_organization)));
                //
                if (add || cf.Contains("at")) f.Values["time_assignment_flag"] = _._(s.allows_time, nameof(s.allows_time));
                if (add || cf.Contains("tnepr")) f.Values["ts_sub_po_required"] = _._(s.ts_non_emp_po_required, nameof(s.ts_non_emp_po_required));
                if (add || cf.Contains("ae")) f.Values["expense_assignment_flag"] = _._(s.allows_expense, nameof(s.allows_expense));
                if (add || cf.Contains("enepr")) f.Values["exp_sub_po_required"] = _._(s.exp_non_emp_po_required, nameof(s.exp_non_emp_po_required));
                if (add || cf.Contains("ai")) f.Values["item_assignment_flag"] = _._(s.allows_item, nameof(s.allows_item));
                //
                //if (add || cf.Contains("u11")) f.Values["udf_10"] = _._(s.user11, nameof(s.user11));
                //if (add || cf.Contains("u12")) f.Values["udf_11"] = _._(s.user12, nameof(s.user12));
                //if (add || cf.Contains("u13")) f.Values["udf_12"] = _._(s.user13, nameof(s.user13));
                //if (add || cf.Contains("u14")) f.Values["udf_13"] = _._(s.user14, nameof(s.user14));
                //if (add || cf.Contains("u15")) f.Values["udf_14"] = _._(s.user15, nameof(s.user15));
                //if (add || cf.Contains("u16")) f.Values["udf_15"] = _._(s.user16, nameof(s.user16));
                //if (add || cf.Contains("u17")) f.Values["udf_16"] = _._(s.user17, nameof(s.user17));
                //if (add || cf.Contains("u18")) f.Values["udf_17"] = _._(s.user18, nameof(s.user18));
                //if (add || cf.Contains("u19")) f.Values["udf_18"] = _._(s.user19, nameof(s.user19));
                //if (add || cf.Contains("u20")) f.Values["udf_19"] = _._(s.user20, nameof(s.user20));
                return f.ToString();
            }).ConfigureAwait(false);
            return (_.Changed(r), last);
        }
    }
}