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
    public class ProjectModel : ModelBase
    {
        public string project_org_code { get; set; }
        public string project_code { get; set; }
        public string project_type { get; set; }
        public string project_status { get; set; }
        public string project_manager { get; set; }
        public string open_edit { get; set; }
        //
        public string self_signup { get; set; }
        public string self_plan { get; set; }
        public string self_assign_plans { get; set; }
        public string allows_future_time { get; set; }
        public string approve_time { get; set; }
        public string approve_expense { get; set; }
        public string time_requires_task { get; set; }
        public string expense_requires_task { get; set; }
        public string allows_expenses { get; set; }
        //
        public DateTime? original_start_date { get; set; }
        public DateTime? original_end_date { get; set; }
        public DateTime? revised_start_date { get; set; }
        public DateTime? revised_end_date { get; set; }
        public DateTime? completed_date { get; set; }
        //
        public decimal? budget_hours { get; set; }
        public string budget_labor_dollars_bill { get; set; }
        public string budget_expense_dollars_bill { get; set; }
        //
        public decimal? etc_hours { get; set; }
        public string etc_labor_dollars_bill { get; set; }
        public string etc_expense_dollars_bill { get; set; }
        public string est_tot_hours { get; set; }
        public string est_tot_labor_dollars_bill { get; set; }
        public string est_tot_expense_dollars_bill { get; set; }
        //
        public string external_system_code { get; set; }
        public string title { get; set; }
        public string comments { get; set; }
        public string assigned_person_orgs { get; set; }
        //
        public string pay_code_list { get; set; }
        public string default_pay_code { get; set; }
        public string task_level_assignment { get; set; }
        public string probability_percent { get; set; }
        //
        public string percent_complete { get; set; }
        public string tito_required { get; set; }
        public string bill_rate_source { get; set; }
        public string cost_rate_source { get; set; }
        public string use_labor_category { get; set; }
        public string enforce_wbs_dates { get; set; }
        public string leave_ind { get; set; }
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
        public string budget_expense_dollars_cost { get; set; }
        public string etc_labor_dollars_cost { get; set; }
        public string etc_expense_dollars_cost { get; set; }
        public string est_tot_labor_dollars_cost { get; set; }
        public string est_tot_expense_dollars_cost { get; set; }
        //
        public string pct_complete_rule { get; set; }
        public string project_color { get; set; }
        public string proj_access_proj_manager { get; set; }
        public string proj_access_proj_viewer { get; set; }
        public string proj_access_resource_manager { get; set; }
        public string allows_time { get; set; }
        public string delete { get; set; }
        //
        public string owning_organization { get; set; }
        public string billing_type { get; set; }
        public string cost_structure { get; set; }
        public string fee_factor { get; set; }
        public string fee_calculation_method { get; set; }
        public string total_value { get; set; }
        public string funded_value { get; set; }
        public string budget_labor_dollars_cost_burdened { get; set; }
        public string budget_expense_dollars_cost_burdened { get; set; }
        //
        public string require_time_comments { get; set; }
        public string proj_access_resource_planner { get; set; }
        public string proj_access_resource_assigner { get; set; }
        public string proj_access_resource_requestor { get; set; }
        public string default_location { get; set; }
        public string location_required { get; set; }
        public string fee_factor_type { get; set; }
        public string proj_access_billing_manager { get; set; }
        public string proj_access_billing_viewer { get; set; }
        public string limit_bill_to_funded { get; set; }
        public string limit_rev_to_funded { get; set; }
        public string posting_group { get; set; }
        // new
        public string ts_non_emp_po_required { get; set; }
        public string exp_non_emp_po_required { get; set; }
        public string allows_item { get; set; }
        public string item_requires_task { get; set; }
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
        //
        public string proj_access_proj_doc_viewer { get; set; }
        public string proj_access_po_viewer { get; set; }
        public string proj_access_pr_viewer { get; set; }
        // custom
        public string key { get; set; }
        public string project_org_codeKey { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.project.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.project.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity ?? una.Options.LegalEntity);
                return null;
            }, sourceFolder));
        }

        public static IEnumerable<ProjectModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.project.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new ProjectModel
                {
                    project_org_code = x[0],
                    project_code = x[1],
                    project_type = x[2],
                    project_status = x[3],
                    project_manager = x[4],
                    open_edit = x[5],
                    //
                    self_signup = x[6],
                    self_plan = x[7],
                    self_assign_plans = x[8],
                    allows_future_time = x[9],
                    approve_time = x[10],
                    approve_expense = x[11],
                    time_requires_task = x[12],
                    expense_requires_task = x[13],
                    allows_expenses = x[14],
                    //
                    original_start_date = x[15].ToDateTime(),
                    original_end_date = x[16].ToDateTime(),
                    revised_start_date = x[17].ToDateTime("BOT"),
                    revised_end_date = x[18].ToDateTime("EOT"),
                    completed_date = x[19].ToDateTime(),
                    //
                    budget_hours = x[20].ToDecimal(),
                    budget_labor_dollars_bill = x[21],
                    budget_expense_dollars_bill = x[22],
                    //
                    etc_hours = x[23].ToDecimal(),
                    etc_labor_dollars_bill = x[24],
                    etc_expense_dollars_bill = x[25],
                    est_tot_hours = x[26],
                    est_tot_labor_dollars_bill = x[27],
                    est_tot_expense_dollars_bill = x[28],
                    //
                    external_system_code = x[29],
                    title = x[30],
                    comments = x[31],
                    assigned_person_orgs = x[32],
                    //
                    pay_code_list = x[33],
                    default_pay_code = x[34],
                    task_level_assignment = x[35],
                    probability_percent = x[36],
                    //
                    percent_complete = x[37],
                    tito_required = x[38],
                    bill_rate_source = x[39],
                    cost_rate_source = x[40],
                    use_labor_category = x[41],
                    enforce_wbs_dates = x[42],
                    leave_ind = x[43],
                    //
                    user01 = x[44],
                    user02 = x[45],
                    user03 = x[46],
                    user04 = x[47],
                    user05 = x[48],
                    user06 = x[49],
                    user07 = x[50],
                    user08 = x[51],
                    user09 = x[52],
                    user10 = x[53],
                    //
                    budget_labor_dollars_cost = x[54],
                    budget_expense_dollars_cost = x[55],
                    etc_labor_dollars_cost = x[56],
                    etc_expense_dollars_cost = x[57],
                    est_tot_labor_dollars_cost = x[58],
                    est_tot_expense_dollars_cost = x[59],
                    //
                    pct_complete_rule = x[60],
                    project_color = x[61],
                    proj_access_proj_manager = x[62],
                    proj_access_proj_viewer = x[63],
                    proj_access_resource_manager = x[64],
                    allows_time = x[65],
                    delete = x[67],
                    //
                    owning_organization = x[67],
                    billing_type = x[68],
                    cost_structure = x[69],
                    fee_factor = x[70],
                    fee_calculation_method = x[71],
                    total_value = x[72],
                    funded_value = x[73],
                    budget_labor_dollars_cost_burdened = x[74],
                    budget_expense_dollars_cost_burdened = x[75],
                    //
                    require_time_comments = x[76],
                    proj_access_resource_planner = x[77],
                    proj_access_resource_assigner = x[78],
                    proj_access_resource_requestor = x[79],
                    default_location = x[80],
                    location_required = x[81],
                    fee_factor_type = x[82],
                    proj_access_billing_manager = x[83],
                    proj_access_billing_viewer = x[84],
                    limit_bill_to_funded = x[85],
                    limit_rev_to_funded = x[86],
                    posting_group = x[87],
                    //
                    ts_non_emp_po_required = x[88],
                    exp_non_emp_po_required = x[89],
                    allows_item = x[90],
                    item_requires_task = x[91],
                    //
                    user11 = x[92],
                    user12 = x[93],
                    user13 = x[94],
                    user14 = x[95],
                    user15 = x[96],
                    user16 = x[97],
                    user17 = x[98],
                    user18 = x[99],
                    user19 = x[100],
                    user20 = x[101],
                    //
                    proj_access_proj_doc_viewer = x[102],
                    proj_access_po_viewer = x[103],
                    proj_access_pr_viewer = x[104],
                    //
                    key = x[105],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("pt", x.project_type), XAttribute("ps", x.project_status), XAttribute("pm", x.project_manager), XAttribute("oe", x.open_edit),
                XAttribute("ss", x.self_signup), XAttribute("sp", x.self_plan), XAttribute("sap", x.self_assign_plans), XAttribute("aft", x.allows_future_time), XAttribute("at", x.approve_time), XAttribute("ae", x.approve_expense), XAttribute("trt", x.time_requires_task), XAttribute("ert", x.expense_requires_task), XAttribute("ae2", x.allows_expenses),
                XAttribute("osd", x.original_start_date), XAttribute("oed", x.original_end_date), XAttribute("rsd", x.revised_start_date), XAttribute("red", x.revised_end_date), XAttribute("cd", x.completed_date),
                XAttribute("bh", x.budget_hours), XAttribute("bldb", x.budget_labor_dollars_bill), XAttribute("bedb", x.budget_expense_dollars_bill),
                XAttribute("eh", x.etc_hours), XAttribute("eldb", x.etc_labor_dollars_bill), XAttribute("eedb", x.etc_expense_dollars_bill), XAttribute("eth", x.est_tot_hours), XAttribute("etldb", x.est_tot_labor_dollars_bill), XAttribute("etedb", x.est_tot_expense_dollars_bill),
                XAttribute("esc", x.external_system_code), XAttribute("t", x.title), XAttribute("c", x.comments), XAttribute("apo", x.assigned_person_orgs),
                XAttribute("pcl", x.pay_code_list), XAttribute("dpc", x.default_pay_code), XAttribute("tla", x.task_level_assignment), XAttribute("pp", x.probability_percent),
                XAttribute("pc2", x.percent_complete), XAttribute("tr", x.tito_required), XAttribute("brs", x.bill_rate_source), XAttribute("crs", x.cost_rate_source), XAttribute("ulc", x.use_labor_category), XAttribute("ewd", x.enforce_wbs_dates), XAttribute("li", x.leave_ind),
                XAttribute("u1", x.user01), XAttribute("u2", x.user02), XAttribute("u3", x.user03), XAttribute("u4", x.user04), XAttribute("u5", x.user05), XAttribute("u6", x.user06), XAttribute("u7", x.user07), XAttribute("u8", x.user08), XAttribute("u9", x.user09), XAttribute("u10", x.user10),
                XAttribute("bldc", x.budget_labor_dollars_cost), XAttribute("bedc", x.budget_expense_dollars_cost), XAttribute("eldc", x.etc_labor_dollars_cost), XAttribute("eedc", x.etc_expense_dollars_cost), XAttribute("etldc", x.est_tot_labor_dollars_cost), XAttribute("etedc", x.est_tot_expense_dollars_cost),
                XAttribute("pcr", x.pct_complete_rule), XAttribute("pc3", x.project_color), XAttribute("papm", x.proj_access_proj_manager), XAttribute("papv", x.proj_access_proj_viewer), XAttribute("parm", x.proj_access_resource_manager), XAttribute("at2", x.allows_time),
                XAttribute("oo", x.owning_organization), XAttribute("bt", x.billing_type), XAttribute("cs", x.cost_structure), XAttribute("ff", x.fee_factor), XAttribute("fcm", x.fee_calculation_method), XAttribute("tv", x.total_value), XAttribute("fv", x.funded_value), XAttribute("bldcb", x.budget_labor_dollars_cost_burdened), XAttribute("bedcb", x.budget_expense_dollars_cost_burdened),
                XAttribute("rtc", x.require_time_comments), XAttribute("parp", x.proj_access_resource_planner), XAttribute("para", x.proj_access_resource_assigner), XAttribute("parr", x.proj_access_resource_requestor), XAttribute("dl", x.default_location), XAttribute("lr", x.location_required), XAttribute("fft", x.fee_factor_type), XAttribute("pacbm", x.proj_access_billing_manager), XAttribute("pabv", x.proj_access_billing_viewer), XAttribute("lbtf", x.limit_bill_to_funded), XAttribute("lrtf", x.limit_rev_to_funded), XAttribute("pg", x.posting_group),
                // new
                XAttribute("tnepr", x.ts_non_emp_po_required), XAttribute("enepr", x.exp_non_emp_po_required), XAttribute("ai", x.allows_item), XAttribute("irt", x.item_requires_task),
                XAttribute("u11", x.user11), XAttribute("u12", x.user12), XAttribute("u13", x.user13), XAttribute("u14", x.user14), XAttribute("u15", x.user15), XAttribute("u16", x.user16), XAttribute("u17", x.user17), XAttribute("u18", x.user18), XAttribute("u19", x.user19), XAttribute("u20", x.user20),
                XAttribute("papdv", x.proj_access_proj_doc_viewer), XAttribute("papv2", x.proj_access_po_viewer), XAttribute("papv3", x.proj_access_pr_viewer)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".j.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Project1 : ProjectModel
        {
            public string XA { get; set; }
            public string XCF { get; set; }
        }

        public static async Task<(ChangedFields changed, string last)> ManageRecordAsync(UnanetClient una, p_Project1 s, Action<p_Project1> bespoke = null)
        {
            var _ = new ChangedFields(ManageFlags.ProjectChanged);
            bespoke?.Invoke(s);
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out  var last2))
                return (_.Changed(), last2);
            var organizations = Unanet.Lookups.CostCenters.Value;
            var (r, last) = await una.SubmitManageAsync(add ? HttpMethod.Post : HttpMethod.Put, "projects",
                $"projectkey={s.key}",
                (z, f) =>
            {
                if (add || cf.Contains("poc")) f.Values["projectOrg"] = _._(s.project_org_codeKey, nameof(s.project_org_codeKey));
                if (add || cf.Contains("pc")) f.Values["pjtCode"] = _._(s.project_code, nameof(s.project_code));
                if (add || cf.Contains("pt")) f.FromSelect("pjtType", _._(s.project_type, nameof(s.project_type)));
                if (add || cf.Contains("ps")) f.FromSelect("pjtStatus", $"{_._(s.project_status, nameof(s.project_status))} ({s.project_status})");
                //if (add || cf.Contains("pm")) f.Values["xxxx"] = _._(s.project_manager, nameof(s.project_manager));
                //if (add || cf.Contains("oe")) f.Values["xxxx"] = _._(s.open_edit, nameof(s.open_edit)); //??
                //
                if (add || cf.Contains("ss")) f.Checked["selfSign"] = _._(s.self_signup, nameof(s.self_signup)) == "Y";
                if (add || cf.Contains("sp")) f.FromSelectByKey("selfPlan", _._(s.self_plan, nameof(s.self_plan)));
                if (add || cf.Contains("sap")) f.Checked["assignSelfPlans"] = _._(s.self_assign_plans, nameof(s.self_assign_plans)) == "Y";
                if (add || cf.Contains("aft")) f.Checked["future_charge"] = _._(s.allows_future_time, nameof(s.allows_future_time)) == "Y";
                //if (add || cf.Contains("at")) f.Checked["xxxx"] = _._(s.approve_time, nameof(s.approve_time)) == "Y";
                //if (add || cf.Contains("ae")) f.Checked["xxxx"] = _._(s.approve_expense, nameof(s.approve_expense)) == "Y";
                if (add || cf.Contains("trt")) f.Checked["ts_task_required"] = _._(s.time_requires_task, nameof(s.time_requires_task)) == "Y";
                if (add || cf.Contains("ert")) f.Checked["er_task_required"] = _._(s.expense_requires_task, nameof(s.expense_requires_task)) == "Y";
                if (add || cf.Contains("ae2")) f.Checked["expenseCharge"] = _._(s.allows_expenses, nameof(s.allows_expenses)) == "Y";
                //
                if (add || cf.Contains("osd")) f.Values["origStartDate"] = _._(s.original_start_date, nameof(s.original_start_date)).FromDateTime();
                if (add || cf.Contains("oed")) f.Values["origEndDate"] = _._(s.original_end_date, nameof(s.original_end_date)).FromDateTime();
                if (add || cf.Contains("rsd")) f.Values["revStartDate"] = _._(s.revised_start_date, nameof(s.revised_start_date)).FromDateTime("BOT");
                if (add || cf.Contains("red")) f.Values["revEndDate"] = _._(s.revised_end_date, nameof(s.revised_end_date)).FromDateTime("EOT");
                if (add || cf.Contains("cd")) f.Values["completedDate"] = _._(s.completed_date, nameof(s.completed_date)).FromDateTime();
                //
                if (add || cf.Contains("bh")) f.Values["bgtHrs"] = _._(s.budget_hours, nameof(s.budget_hours)).ToString();
                if (add || cf.Contains("bldb")) f.Values["bgtLaborAmtBill"] = _._(s.budget_labor_dollars_bill, nameof(s.budget_labor_dollars_bill));
                if (add || cf.Contains("bedb")) f.Values["bgtExpenseAmtBill"] = _._(s.budget_expense_dollars_bill, nameof(s.budget_expense_dollars_bill));
                //
                if (add || cf.Contains("eh")) f.Values["etcHrs"] = _._(s.etc_hours, nameof(s.etc_hours)).ToString();
                if (add || cf.Contains("eldb")) f.Values["etcLaborAmtBill"] = _._(s.etc_labor_dollars_bill, nameof(s.etc_labor_dollars_bill));
                if (add || cf.Contains("eedb")) f.Values["etcExpenseAmtBill"] = _._(s.etc_expense_dollars_bill, nameof(s.etc_expense_dollars_bill));
                if (add || cf.Contains("eth")) f.Values["estTotalHrs"] = _._(s.est_tot_hours, nameof(s.est_tot_hours));
                if (add || cf.Contains("etldb")) f.Values["estTotalLaborAmtBill"] = _._(s.est_tot_labor_dollars_bill, nameof(s.est_tot_labor_dollars_bill));
                if (add || cf.Contains("etedb")) f.Values["estTotalExpenseAmtBill"] = _._(s.est_tot_expense_dollars_bill, nameof(s.est_tot_expense_dollars_bill));
                //
                if (add || cf.Contains("esc") || cf.Contains("bind")) f.Values["externalSystemCode"] = _._(s.external_system_code, nameof(s.external_system_code));
                if (add || cf.Contains("t")) f.Values["pjtTitle"] = _._(s.title, nameof(s.title));
                if (add || cf.Contains("c")) f.Values["purpose"] = _._(s.comments, nameof(s.comments));
                //if (add || cf.Contains("apo")) f.Values["xxxx"] = _._(s.assigned_person_orgs, nameof(s.assigned_person_orgs)); // Multi
                //
                //if (add || cf.Contains("pcl")) f.FromSelect("xxxx", _._(s.pay_code_list, nameof(s.pay_code_list)));
                if (add || cf.Contains("dpc"))
                    if (s.default_pay_code != null) f.FromSelect("default_paycode", _._(s.default_pay_code, nameof(s.default_pay_code)));
                    else f.FromSelectByPredicate("default_paycode", _._(s.default_pay_code, nameof(s.default_pay_code)), x => true);
                if (add || cf.Contains("tla")) f.Checked["taskLevelAssignment"] = _._(s.task_level_assignment, nameof(s.task_level_assignment)) == "Y";
                if (add || cf.Contains("pp")) f.Values["probability"] = _._(s.probability_percent, nameof(s.probability_percent));
                //
                if (add || cf.Contains("pc2")) f.Values["percentComplete"] = _._(s.percent_complete, nameof(s.percent_complete));
                if (add || cf.Contains("tr")) f.FromSelectByKey("titoRequired", _._(s.tito_required, nameof(s.tito_required)));
                if (add || cf.Contains("brs")) f.FromSelectByKey("billRateSource", _._(s.bill_rate_source, nameof(s.bill_rate_source)));
                if (add || cf.Contains("crs")) f.FromSelectByKey("costRateSource", _._(s.cost_rate_source, nameof(s.cost_rate_source)));
                if (add || cf.Contains("ulc")) f.FromSelectByKey("useLaborCategory", _._(s.use_labor_category, nameof(s.use_labor_category)));
                if (add || cf.Contains("ewd")) f.Checked["enforceWBSDates"] = _._(s.enforce_wbs_dates, nameof(s.enforce_wbs_dates)) == "Y";
                if (add || cf.Contains("li")) f.Checked["leaveBalance"] = _._(s.leave_ind, nameof(s.leave_ind)) == "Y";
                //
                if (add || cf.Contains("u1")) f.FromSelect("udf_0", _._(s.user01, nameof(s.user01)));
                if (add || cf.Contains("u2")) f.FromSelect("udf_1", _._(s.user02, nameof(s.user02)));
                if (add || cf.Contains("u3")) f.FromSelect("udf_2", _._(s.user03, nameof(s.user03)));
                if (add || cf.Contains("u4")) f.FromSelect("udf_3", _._(s.user04, nameof(s.user04)));
                if (add || cf.Contains("u5")) f.FromSelect("udf_4", _._(s.user05, nameof(s.user05)));
                if (add || cf.Contains("u6")) f.FromSelect("udf_5", _._(s.user06, nameof(s.user06)));
                if (add || cf.Contains("u7")) f.FromSelect("udf_6", _._(s.user07, nameof(s.user07)));
                if (add || cf.Contains("u8")) f.FromSelect("udf_7", _._(s.user08, nameof(s.user08)));
                if (add || cf.Contains("u9")) f.FromSelect("udf_8", _._(s.user09, nameof(s.user09)));
                if (add || cf.Contains("u10")) f.FromSelect("udf_9", _._(s.user10, nameof(s.user10)));
                //
                if (add || cf.Contains("bldc")) f.Values["bgtLaborAmtCost"] = _._(s.budget_labor_dollars_cost, nameof(s.budget_labor_dollars_cost));
                if (add || cf.Contains("bedc")) f.Values["bgtExpenseAmtCost"] = _._(s.budget_expense_dollars_cost, nameof(s.budget_expense_dollars_cost));
                if (add || cf.Contains("eldc")) f.Values["etcLaborAmtCost"] = _._(s.etc_labor_dollars_cost, nameof(s.etc_labor_dollars_cost));
                if (add || cf.Contains("eedc")) f.Values["etcExpenseAmtCost"] = _._(s.etc_expense_dollars_cost, nameof(s.etc_expense_dollars_cost));
                if (add || cf.Contains("etldc")) f.Values["estTotalLaborAmtCost"] = _._(s.est_tot_labor_dollars_cost, nameof(s.est_tot_labor_dollars_cost));
                if (add || cf.Contains("etedc")) f.Values["estTotalExpenseAmtCost"] = _._(s.est_tot_expense_dollars_cost, nameof(s.est_tot_expense_dollars_cost));
                //
                if (add || cf.Contains("pcr")) f.FromSelectByKey("pctComplRule", _._(s.pct_complete_rule, nameof(s.pct_complete_rule)));
                if (add || cf.Contains("pc3")) f.FromSelectByKey("projectColor", _._(s.project_color, nameof(s.project_color)));
                if (add || cf.Contains("papm")) f.Checked["pmOpen"] = _._(s.proj_access_proj_manager, nameof(s.proj_access_proj_manager)) == "Y";
                if (add || cf.Contains("papv")) f.Checked["viewerOpen"] = _._(s.proj_access_proj_viewer, nameof(s.proj_access_proj_viewer)) == "Y";
                //if (add || cf.Contains("parm")) f.Values["xxxx"] = _._(s.proj_access_resource_manager, nameof(s.proj_access_resource_manager));
                if (add || cf.Contains("at2")) f.Checked["timeCharge"] = _._(s.allows_time, nameof(s.allows_time)) == "Y";
                //
                if (add || cf.Contains("oo")) f.Values["owningCustomer"] = _._(s.owning_organization, nameof(s.owning_organization)) != null &&
                    organizations.TryGetValue(s.owning_organization, out var organizationKey) ? organizationKey : "-1"; // LOOKUP
                if (add || cf.Contains("bt")) f.FromSelect("billingType", _._(s.billing_type, nameof(s.billing_type)));
                if (add || cf.Contains("cs")) f.FromSelect("costStructure", _._(s.cost_structure, nameof(s.cost_structure)));
                //if (add || cf.Contains("ff")) f.Values["feeFactor_0"] = _._(s.fee_factor, nameof(s.fee_factor));
                //if (add || cf.Contains("fcm")) f.FromSelect("feeMethod_0", _._(s.fee_calculation_method, nameof(s.fee_calculation_method)));
                if (add || cf.Contains("tv")) f.Values["totalValue"] = _._(s.total_value, nameof(s.total_value));
                if (add || cf.Contains("fv")) f.Values["fundedValue"] = _._(s.funded_value, nameof(s.funded_value));
                if (add || cf.Contains("bldcb")) f.Values["burdenLaborAmtCost"] = _._(s.budget_labor_dollars_cost_burdened, nameof(s.budget_labor_dollars_cost_burdened));
                if (add || cf.Contains("bedcb")) f.Values["burdenExpAmtCost"] = _._(s.budget_expense_dollars_cost_burdened, nameof(s.budget_expense_dollars_cost_burdened));
                //
                if (add || cf.Contains("rtc")) f.Checked["require_comments"] = _._(s.require_time_comments, nameof(s.require_time_comments)) == "Y";
                if (add || cf.Contains("parp")) f.Checked["rmOpen"] = _._(s.proj_access_resource_planner, nameof(s.proj_access_resource_planner)) == "Y";
                if (add || cf.Contains("para")) f.Checked["raOpen"] = _._(s.proj_access_resource_assigner, nameof(s.proj_access_resource_assigner)) == "Y";
                if (add || cf.Contains("parr")) f.Checked["rrOpen"] = _._(s.proj_access_resource_requestor, nameof(s.proj_access_resource_requestor)) == "Y";
                if (add || cf.Contains("dl")) f.FromSelect("location", _._(s.default_location, nameof(s.default_location)));
                if (add || cf.Contains("lr")) f.Checked["location_required"] = _._(s.location_required, nameof(s.location_required)) == "Y";
                //if (add || cf.Contains("fft")) f.FromSelect("feeFactorType_0", _._(s.fee_factor_type, nameof(s.fee_factor_type)));
                if (add || cf.Contains("pacbm")) f.Checked["bmOpen"] = _._(s.proj_access_billing_manager, nameof(s.proj_access_billing_manager)) == "Y";
                if (add || cf.Contains("pabv")) f.Checked["bvOpen"] = _._(s.proj_access_billing_viewer, nameof(s.proj_access_billing_viewer)) == "Y";
                if (add || cf.Contains("lbtf")) f.Checked["limitBillToFunded"] = _._(s.limit_bill_to_funded, nameof(s.limit_bill_to_funded)) == "Y";
                if (add || cf.Contains("lrtf")) f.Checked["limitRevToFunded"] = _._(s.limit_rev_to_funded, nameof(s.limit_rev_to_funded)) == "Y";
                if (add || cf.Contains("pg")) f.FromSelectByPredicate("postingGroup", _._(s.posting_group, nameof(s.posting_group)), x => x.Value.StartsWith(s.posting_group));
                //
                //if (add || cf.Contains("tnepr")) f.Values["xxx"] = _._(s.ts_non_emp_po_required, nameof(s.ts_non_emp_po_required));
                //if (add || cf.Contains("enepr")) f.Values["xxx"] = _._(s.exp_non_emp_po_required, nameof(s.exp_non_emp_po_required));
                //if (add || cf.Contains("ai")) f.Checked["xxx"] = _._(s.allows_item, nameof(s.allows_item)) == "Y";
                //if (add || cf.Contains("irt")) f.Checked["xxx"] = _._(s.item_requires_task, nameof(s.item_requires_task)) == "Y";
                //
                if (add || cf.Contains("u11")) f.FromSelect("udf_10", _._(s.user11, nameof(s.user11)));
                //if (add || cf.Contains("u12")) f.Values["udf_11"] = _._(s.user12, nameof(s.user12));
                if (add || cf.Contains("u13")) f.Values["udf_12"] = _._(s.user13, nameof(s.user13));
                //if (add || cf.Contains("u14")) f.Values["udf_13"] = _._(s.user14, nameof(s.user14));
                //if (add || cf.Contains("u15")) f.Values["udf_14"] = _._(s.user15, nameof(s.user15));
                //if (add || cf.Contains("u16")) f.Values["udf_15"] = _._(s.user16, nameof(s.user16));
                //if (add || cf.Contains("u17")) f.Values["udf_16"] = _._(s.user17, nameof(s.user17));
                //if (add || cf.Contains("u18")) f.Values["udf_17"] = _._(s.user18, nameof(s.user18));
                if (add || cf.Contains("u19")) f.Values["udf_18"] = _._(s.user19, nameof(s.user19));
                if (add || cf.Contains("u20")) f.FromSelect("udf_19", _._(s.user20, nameof(s.user20)));
                //
                //if (add || cf.Contains("papdv")) f.Values["xxx"] = _tset(s.proj_access_proj_doc_viewer, nameof(s.proj_access_proj_doc_viewer));
                //if (add || cf.Contains("papv")) f.Values["xxx"] = _._(s.proj_access_po_viewer, nameof(s.proj_access_po_viewer));
                //if (add || cf.Contains("papv2")) f.Values["xxx"] = _._(s.proj_access_pr_viewer, nameof(s.proj_access_pr_viewer));
                //f.Values["lastUseLaborCategoryIndex"] = "2";
                f.Remove("email_alert", "time_elapsed_alert_pct", "funding_expended_alert_pct", "hour_alert_pct", "hour_alert_denom", "total_cost_alert_pct", "total_cost_alert_denom", "labor_cost_alert_pct", "labor_cost_alert_denom", "expense_cost_alert_pct", "expense_cost_alert_denom", "total_bill_alert_pct", "total_bill_alert_denom");
                f.Add("button_save", "action", null);
            }).ConfigureAwait(false);
            return (_.Changed(r), last);
        }
    }
}