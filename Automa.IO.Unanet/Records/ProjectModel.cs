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
        public string budget_hours { get; set; }
        public string budget_labor_dollars_bill { get; set; }
        public string budget_expense_dollars_bill { get; set; }
        //
        public string etc_hours { get; set; }
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
        //
        public string key { get; set; }
        public string project_org_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = "75-00-DEG-00 - Digital Evolution Group, LLC") =>
            Task.Run(() => una.GetEntitiesByExport(una.Exports["project"].Item1, f =>
             {
                 f.Checked["suppressOutput"] = true;
                 f.FromSelect("legalEntity", legalEntity);
             }, sourceFolder));

        public static IEnumerable<ProjectModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["project"].Item2}.csv");
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
                    budget_hours = x[20],
                    budget_labor_dollars_bill = x[21],
                    budget_expense_dollars_bill = x[22],
                    //
                    etc_hours = x[23],
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
                    delete = x[66],
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
                    key = x.Count > 88 ? x[88] : null,
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
                XAttribute("rtc", x.require_time_comments), XAttribute("parp", x.proj_access_resource_planner), XAttribute("para", x.proj_access_resource_assigner), XAttribute("parr", x.proj_access_resource_requestor), XAttribute("dl", x.default_location), XAttribute("lr", x.location_required), XAttribute("fft", x.fee_factor_type), XAttribute("pacbm", x.proj_access_billing_manager), XAttribute("pabv", x.proj_access_billing_viewer), XAttribute("lbtf", x.limit_bill_to_funded), XAttribute("lrtf", x.limit_rev_to_funded), XAttribute("pg", x.posting_group)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".j.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Project1 : ProjectModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_Project1 s, out string last)
        {
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out last))
                return ManageFlags.ProjectChanged;
            var organizations = Unanet.OrganizationLookup.CostCenters;
            var r = una.SubmitManage(add ? HttpMethod.Post : HttpMethod.Put, "projects",
                $"projectkey={s.key}",
                out last, (z, f) =>
            {
                if (add || cf.Contains("poc")) f.Values["projectOrg"] = s.project_org_codeKey;
                if (add || cf.Contains("pc")) f.Values["pjtCode"] = s.project_code;
                if (add || cf.Contains("pt")) f.FromSelect("pjtType", s.project_type);
                if (add || cf.Contains("ps")) f.FromSelect("pjtStatus", $"{s.project_status} ({s.project_status})");
                //if (add || cf.Contains("pm")) f.Values["xxxx"] = s.project_manager;
                //if (add || cf.Contains("oe")) f.Values["xxxx"] = s.open_edit; //??
                //
                if (add || cf.Contains("ss")) f.Checked["selfSign"] = s.self_signup == "Y";
                if (add || cf.Contains("sp")) f.FromSelectByKey("selfPlan", s.self_plan);
                if (add || cf.Contains("sap")) f.Checked["assignSelfPlans"] = s.self_assign_plans == "Y";
                if (add || cf.Contains("aft")) f.Checked["future_charge"] = s.allows_future_time == "Y";
                //if (add || cf.Contains("at")) f.Checked["xxxx"] = s.approve_time == "Y";
                //if (add || cf.Contains("ae")) f.Checked["xxxx"] = s.approve_expense == "Y";
                if (add || cf.Contains("trt")) f.Checked["ts_task_required"] = s.time_requires_task == "Y";
                if (add || cf.Contains("ert")) f.Checked["er_task_required"] = s.expense_requires_task == "Y";
                if (add || cf.Contains("ae2")) f.Checked["expenseCharge"] = s.allows_expenses == "Y";
                //
                if (add || cf.Contains("osd")) f.Values["origStartDate"] = s.original_start_date.FromDateTime();
                if (add || cf.Contains("oed")) f.Values["origEndDate"] = s.original_end_date.FromDateTime();
                if (add || cf.Contains("rsd")) f.Values["revStartDate"] = s.revised_start_date.FromDateTime("BOT");
                if (add || cf.Contains("red")) f.Values["revEndDate"] = s.revised_end_date.FromDateTime("EOT");
                if (add || cf.Contains("cd")) f.Values["completedDate"] = s.completed_date.FromDateTime();
                //
                if (add || cf.Contains("bh")) f.Values["bgtHrs"] = s.budget_hours;
                if (add || cf.Contains("bldb")) f.Values["bgtLaborAmtBill"] = s.budget_labor_dollars_bill;
                if (add || cf.Contains("bedb")) f.Values["bgtExpenseAmtBill"] = s.budget_expense_dollars_bill;
                //
                if (add || cf.Contains("eh")) f.Values["etcHrs"] = s.etc_hours;
                if (add || cf.Contains("eldb")) f.Values["etcLaborAmtBill"] = s.etc_labor_dollars_bill;
                if (add || cf.Contains("eedb")) f.Values["etcExpenseAmtBill"] = s.etc_expense_dollars_bill;
                if (add || cf.Contains("eth")) f.Values["estTotalHrs"] = s.est_tot_hours;
                if (add || cf.Contains("etldb")) f.Values["estTotalLaborAmtBill"] = s.est_tot_labor_dollars_bill;
                if (add || cf.Contains("etedb")) f.Values["estTotalExpenseAmtBill"] = s.est_tot_expense_dollars_bill;
                //
                if (add || cf.Contains("esc") || cf.Contains("bind")) f.Values["externalSystemCode"] = s.external_system_code;
                if (add || cf.Contains("t")) f.Values["pjtTitle"] = s.title;
                if (add || cf.Contains("c")) f.Values["purpose"] = s.comments;
                //if (add || cf.Contains("apo")) f.Values["xxxx"] = s.assigned_person_orgs; // Multi
                //
                //if (add || cf.Contains("pcl")) f.FromSelect("xxxx", s.pay_code_list);
                if (add || cf.Contains("dpc"))
                    if (s.default_pay_code != null) f.FromSelect("default_paycode", s.default_pay_code);
                    else f.FromSelectByPredicate("default_paycode", s.default_pay_code, x => true);
                if (add || cf.Contains("tla")) f.Checked["taskLevelAssignment"] = s.task_level_assignment == "Y";
                if (add || cf.Contains("pp")) f.Values["probability"] = s.probability_percent;
                //
                if (add || cf.Contains("pc2")) f.Values["percentComplete"] = s.percent_complete;
                if (add || cf.Contains("tr")) f.FromSelectByKey("titoRequired", s.tito_required);
                if (add || cf.Contains("brs")) f.FromSelectByKey("billRateSource", s.bill_rate_source);
                if (add || cf.Contains("crs")) f.FromSelectByKey("costRateSource", s.cost_rate_source);
                if (add || cf.Contains("ulc")) f.FromSelectByKey("useLaborCategory", s.use_labor_category);
                if (add || cf.Contains("ewd")) f.Checked["enforceWBSDates"] = s.enforce_wbs_dates == "Y";
                if (add || cf.Contains("li")) f.Checked["leaveBalance"] = s.leave_ind == "Y";
                //
                if (add || cf.Contains("u1")) f.FromSelect("udf_0", s.user01);
                if (add || cf.Contains("u2")) f.FromSelect("udf_1", s.user02);
                if (add || cf.Contains("u3")) f.FromSelect("udf_2", s.user03);
                if (add || cf.Contains("u4")) f.FromSelect("udf_3", s.user04);
                if (add || cf.Contains("u5")) f.FromSelect("udf_4", s.user05);
                if (add || cf.Contains("u6")) f.FromSelect("udf_5", s.user06);
                if (add || cf.Contains("u7")) f.FromSelect("udf_6", s.user07);
                if (add || cf.Contains("u8")) f.FromSelect("udf_7", s.user08);
                if (add || cf.Contains("u9")) f.FromSelect("udf_8", s.user09);
                if (add || cf.Contains("u10")) f.FromSelect("udf_9", s.user10);
                //
                if (add || cf.Contains("bldc")) f.Values["bgtLaborAmtCost"] = s.budget_labor_dollars_cost;
                if (add || cf.Contains("bedc")) f.Values["bgtExpenseAmtCost"] = s.budget_expense_dollars_cost;
                if (add || cf.Contains("eldc")) f.Values["etcLaborAmtCost"] = s.etc_labor_dollars_cost;
                if (add || cf.Contains("eedc")) f.Values["etcExpenseAmtCost"] = s.etc_expense_dollars_cost;
                if (add || cf.Contains("etldc")) f.Values["estTotalLaborAmtCost"] = s.est_tot_labor_dollars_cost;
                if (add || cf.Contains("etedc")) f.Values["estTotalExpenseAmtCost"] = s.est_tot_expense_dollars_cost;
                //
                if (add || cf.Contains("pcr")) f.FromSelectByKey("pctComplRule", s.pct_complete_rule);
                if (add || cf.Contains("pc3")) f.FromSelectByKey("projectColor", s.project_color);
                if (add || cf.Contains("papm")) f.Checked["pmOpen"] = s.proj_access_proj_manager == "Y";
                if (add || cf.Contains("papv")) f.Checked["viewerOpen"] = s.proj_access_proj_viewer == "Y";
                //if (add || cf.Contains("parm")) f.Values["xxxx"] = s.proj_access_resource_manager;
                if (add || cf.Contains("at2")) f.Checked["timeCharge"] = s.allows_time == "Y";
                //
                if (add || cf.Contains("oo")) f.Values["owningCustomer"] = s.owning_organization != null &&
                    organizations.TryGetValue(s.owning_organization, out var organizationKey) ? organizationKey : "-1"; // LOOKUP
                if (add || cf.Contains("bt")) f.FromSelect("billingType", s.billing_type);
                if (add || cf.Contains("cs")) f.FromSelect("costStructure", s.cost_structure);
                //if (add || cf.Contains("ff")) f.Values["feeFactor_0"] = s.fee_factor;
                //if (add || cf.Contains("fcm")) f.FromSelect("feeMethod_0", s.fee_calculation_method);
                if (add || cf.Contains("tv")) f.Values["totalValue"] = s.total_value;
                if (add || cf.Contains("fv")) f.Values["fundedValue"] = s.funded_value;
                if (add || cf.Contains("bldcb")) f.Values["burdenLaborAmtCost"] = s.budget_labor_dollars_cost_burdened;
                if (add || cf.Contains("bedcb")) f.Values["burdenExpAmtCost"] = s.budget_expense_dollars_cost_burdened;
                //
                if (add || cf.Contains("rtc")) f.Checked["require_comments"] = s.require_time_comments == "Y";
                if (add || cf.Contains("parp")) f.Checked["rmOpen"] = s.proj_access_resource_planner == "Y";
                if (add || cf.Contains("para")) f.Checked["raOpen"] = s.proj_access_resource_assigner == "Y";
                if (add || cf.Contains("parr")) f.Checked["rrOpen"] = s.proj_access_resource_requestor == "Y";
                if (add || cf.Contains("dl")) f.FromSelect("location", s.default_location);
                if (add || cf.Contains("lr")) f.Checked["location_required"] = s.location_required == "Y";
                //if (add || cf.Contains("fft")) f.FromSelect("feeFactorType_0", s.fee_factor_type);
                if (add || cf.Contains("pacbm")) f.Checked["bmOpen"] = s.proj_access_billing_manager == "Y";
                if (add || cf.Contains("pabv")) f.Checked["bvOpen"] = s.proj_access_billing_viewer == "Y";
                if (add || cf.Contains("lbtf")) f.Checked["limitBillToFunded"] = s.limit_bill_to_funded == "Y";
                if (add || cf.Contains("lrtf")) f.Checked["limitRevToFunded"] = s.limit_rev_to_funded == "Y";
                if (add || cf.Contains("pg")) f.FromSelectByPredicate("postingGroup", s.posting_group, x => x.Value.StartsWith(s.posting_group));
                //
                //f.Values["lastUseLaborCategoryIndex"] = "2";
                f.Remove("email_alert", "time_elapsed_alert_pct", "funding_expended_alert_pct", "hour_alert_pct", "hour_alert_denom", "total_cost_alert_pct", "total_cost_alert_denom", "labor_cost_alert_pct", "labor_cost_alert_denom", "expense_cost_alert_pct", "expense_cost_alert_denom", "total_bill_alert_pct", "total_bill_alert_denom");
                f.Add("button_save", "action", null);
            });
            return r != null ?
                ManageFlags.ProjectChanged :
                ManageFlags.None;
        }
    }
}