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
    public class PersonModel : ModelBase
    {
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string middle_initial { get; set; }
        public string suffix { get; set; }
        public string nickname { get; set; }
        //
        public string exempt_status { get; set; }
        public string roles { get; set; }
        public string time_period { get; set; }
        public string pay_code { get; set; }
        public decimal? hour_increment { get; set; }
        public string expense_approval_group { get; set; }
        //
        public string person_code { get; set; }
        public string id_code_1 { get; set; }
        public string id_code_2 { get; set; }
        public string password { get; set; }
        public string ivr_password { get; set; }
        public string email { get; set; }
        public string person_org_code { get; set; }
        public decimal? bill_rate { get; set; }
        public decimal? cost_rate { get; set; }
        public string time_approval_group { get; set; }
        //
        public string active { get; set; }
        public string timesheet_emails { get; set; }
        public string expense_emails { get; set; }
        public string autofill_timesheet { get; set; }
        public string expense_approval_amount { get; set; }
        public DateTime? effective_date { get; set; }
        public string dilution_period { get; set; }
        public string default_project_org { get; set; }
        public string default_project { get; set; }
        public string default_task { get; set; }
        //
        public string default_labor_category { get; set; }
        public string default_payment_method { get; set; }
        public string tito_required { get; set; }
        public string business_week { get; set; }
        public string assignment_emails { get; set; }
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
        public DateTime? hire_date { get; set; }
        public string payment_currency { get; set; }
        public string delete { get; set; }
        public string cost_structure { get; set; }
        public string cost_element { get; set; }
        public string unlock { get; set; }
        public string location { get; set; }
        //
        public string employee_type { get; set; }
        public string hide_vat { get; set; }
        public string leave_request_emails { get; set; }
        public string tbd_user { get; set; }
        public string time_vendor { get; set; }
        public string expense_vendor { get; set; }
        //
        public DateTime? payroll_hire_date { get; set; }
        public string payroll_marital_status { get; set; }
        public string payroll_federal_exemptions { get; set; }
        public string payroll_sui_tax_code { get; set; }
        public string payroll_state_worked_in { get; set; }
        public string payroll_immigration_status { get; set; }
        public string payroll_eeo_code { get; set; }
        public string payroll_medical_plan { get; set; }
        public string payroll_last_rate_change_date { get; set; }
        public string payroll_last_rate_change { get; set; }
        //
        public string key { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = "75-00-DEG-00 - Digital Evolution Group, LLC")
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["person"].Item2}.csv");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Exports["person"].Item1, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity);
                f.Checked["exempt"] = true; f.Checked["nonExempt"] = true; f.Checked["nonEmployee"] = true;
            }, sourceFolder));
        }

        public static IEnumerable<PersonModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["person"].Item2}.csv");
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new PersonModel
                {
                    username = x[0],
                    first_name = x[1],
                    last_name = x[2],
                    middle_initial = x[3],
                    suffix = x[4],
                    nickname = x[5],
                    //
                    exempt_status = x[6],
                    roles = x[7],
                    time_period = x[8],
                    pay_code = x[9],
                    hour_increment = x[10].ToDecimal(),
                    expense_approval_group = x[11],
                    //
                    person_code = x[12],
                    id_code_1 = x[13],
                    id_code_2 = x[14],
                    password = x[15],
                    ivr_password = x[16],
                    email = x[17],
                    person_org_code = x[18],
                    bill_rate = x[19].ToDecimal(),
                    cost_rate = x[20].ToDecimal(),
                    time_approval_group = x[21],
                    //
                    active = x[22],
                    timesheet_emails = x[23],
                    expense_emails = x[24],
                    autofill_timesheet = x[25],
                    expense_approval_amount = x[26],
                    effective_date = x[27].ToDateTime(),
                    dilution_period = x[28],
                    default_project_org = x[29],
                    default_project = x[30],
                    default_task = x[31],
                    //
                    default_labor_category = x[32],
                    default_payment_method = x[33],
                    tito_required = x[34],
                    business_week = x[35],
                    assignment_emails = x[36],
                    //
                    user01 = x[37],
                    user02 = x[38],
                    user03 = x[39],
                    user04 = x[40],
                    user05 = x[41],
                    user06 = x[42],
                    user07 = x[43],
                    user08 = x[44],
                    user09 = x[45],
                    user10 = x[46],
                    //
                    hire_date = x[47].ToDateTime(),
                    payment_currency = x[48],
                    delete = x[49],
                    cost_structure = x[50],
                    cost_element = x[51],
                    unlock = x[52],
                    location = x[53],
                    //
                    employee_type = x[54],
                    hide_vat = x[55],
                    leave_request_emails = x[56],
                    tbd_user = x[57],
                    time_vendor = x[58],
                    expense_vendor = x[59],
                    //
                    payroll_hire_date = x[60].ToDateTime(),
                    payroll_marital_status = x[61],
                    payroll_federal_exemptions = x[62],
                    payroll_sui_tax_code = x[63],
                    payroll_state_worked_in = x[64],
                    payroll_immigration_status = x[65],
                    payroll_eeo_code = x[66],
                    payroll_medical_plan = x[67],
                    payroll_last_rate_change_date = x[68],
                    payroll_last_rate_change = x[69],
                    //
                    key = x.Count > 70 ? x[70] : null,
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("u", x.username), XAttribute("fn", x.first_name), XAttribute("ln", x.last_name), XAttribute("mi", x.middle_initial), XAttribute("s", x.suffix), XAttribute("n", x.nickname),
                XAttribute("es", x.exempt_status), XAttribute("r", x.roles), XAttribute("tp", x.time_period), XAttribute("pc", x.pay_code), XAttribute("hi", x.hour_increment), XAttribute("eag", x.expense_approval_group),
                XAttribute("pc2", x.person_code), XAttribute("ic1", x.id_code_1), XAttribute("ic2", x.id_code_2), XAttribute("p", x.password), XAttribute("ip", x.ivr_password), XAttribute("e", x.email), XAttribute("poc", x.person_org_code), XAttribute("br", x.bill_rate), XAttribute("cr", x.cost_rate), XAttribute("tag", x.time_approval_group),
                XAttribute("a", x.active), XAttribute("te", x.timesheet_emails), XAttribute("ee", x.expense_emails), XAttribute("at", x.autofill_timesheet), XAttribute("eaa", x.expense_approval_amount), XAttribute("ed", x.effective_date), XAttribute("dp", x.dilution_period), XAttribute("dpo", x.default_project_org), XAttribute("dp2", x.default_project), XAttribute("dt", x.default_task),
                XAttribute("dlc", x.default_labor_category), XAttribute("dpm", x.default_payment_method), XAttribute("tr", x.tito_required), XAttribute("bw", x.business_week), XAttribute("ae", x.assignment_emails),
                XAttribute("u1", x.user01), XAttribute("u2", x.user02), XAttribute("u3", x.user03), XAttribute("u4", x.user04), XAttribute("u5", x.user05), XAttribute("u6", x.user06), XAttribute("u7", x.user07), XAttribute("u8", x.user08), XAttribute("u9", x.user09), XAttribute("u10", x.user10),
                XAttribute("hd", x.hire_date), XAttribute("pc3", x.payment_currency), XAttribute("cs", x.cost_structure), XAttribute("ce", x.cost_element), XAttribute("ul", x.unlock), XAttribute("l", x.location),
                XAttribute("et", x.employee_type), XAttribute("hv", x.hide_vat), XAttribute("lre", x.leave_request_emails), XAttribute("tu", x.tbd_user), XAttribute("tv", x.time_vendor), XAttribute("ev", x.expense_vendor),
                XAttribute("phd", x.payroll_hire_date), XAttribute("pm", x.payroll_marital_status), XAttribute("pfe", x.payroll_federal_exemptions), XAttribute("pstc", x.payroll_sui_tax_code), XAttribute("pswi", x.payroll_state_worked_in), XAttribute("pis", x.payroll_immigration_status), XAttribute("pec", x.payroll_eeo_code), XAttribute("pmp", x.payroll_medical_plan), XAttribute("plrcd", x.payroll_last_rate_change_date), XAttribute("plrc", x.payroll_last_rate_change)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".p.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Person1 : PersonModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_Person1 s, out string last)
        {
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out last))
                return ManageFlags.PersonChanged;
            var locations = Unanet.LocationLookup.Locations;
            var approvalGroups = Unanet.ApprovalGroupLookup.TimeExpense;
            var laborCategories = Unanet.LaborCategoryLookup.LaborCategories;
            var vendorProfiles = Unanet.OrganizationLookup.Vendors;
            var organizations = Unanet.OrganizationLookup.CostCenters;
            //
            if (s.id_code_1 == "C:AAAAAAAAAAAAAAAAAAAAAA")
                throw new InvalidOperationException($"{s.username} has a {nameof(s.id_code_1)}");
            if (!approvalGroups.ContainsKey(s.expense_approval_group))
                throw new ArgumentOutOfRangeException(nameof(s.expense_approval_group), s.expense_approval_group);
            if (s.username == "DDULLEA" || s.username == "SMOREY" || s.username == "TBENSON")
                throw new InvalidOperationException($"{s.username} restricted user, please update manually.");
            var r = una.SubmitManage(add ? HttpMethod.Post : HttpMethod.Put, "people",
                $"personkey={s.key}",
                out last, (z, f) =>
            {
                if (add || cf.Contains("u")) f.Values["username"] = s.username;
                if (add || cf.Contains("fn")) f.Values["first_name"] = s.first_name;
                if (add || cf.Contains("ln")) f.Values["last_name"] = s.last_name;
                if (add || cf.Contains("mi")) f.Values["middleInitial"] = s.middle_initial;
                if (add || cf.Contains("s")) f.Values["suffix"] = s.suffix;
                //if (add || cf.Contains("n")) f.Values["xxxx"] = s.nickname;
                //
                if (add || cf.Contains("es")) f.FromSelectByKey("exempt", s.exempt_status);
                if (add || cf.Contains("r")) f.FromMultiCheckbox("rolenames", s.roles);

                if (add || cf.Contains("tp")) f.FromSelect("timePeriod", s.time_period);
                if (add || cf.Contains("pc")) f.FromSelect("payCode", s.pay_code);
                if (add || cf.Contains("hi")) f.FromSelectByKey("hour_increment", s.hour_increment?.ToString());
                if (add || cf.Contains("eag")) f.Values["expense_request_chain"] = s.expense_approval_group != null && // FIX SOURCE
                    approvalGroups.TryGetValue(s.expense_approval_group, out var approvalGroupKey) ? approvalGroupKey : "-1"; // LOOKUP
                if (add || cf.Contains("eag")) f.Values["expense_report_chain"] = s.expense_approval_group != null && // FIX SOURCE
                    approvalGroups.TryGetValue(s.expense_approval_group, out var approvalGroupKey) ? approvalGroupKey : "-1"; // LOOKUP
                //
                if (add || cf.Contains("pc2")) f.Values["person_code"] = s.person_code;
                if (add || cf.Contains("ic1") || cf.Contains("bind")) f.Values["empId"] = s.id_code_1;
                if (add || cf.Contains("ic2")) f.Values["ssn"] = s.id_code_2;
                //if (add || cf.Contains("p")) f.Values["password1"] = s.password;
                //if (add || cf.Contains("ip")) f.Values["password2"] = s.ivr_password;
                if (add || cf.Contains("e")) f.Values["email"] = s.email;
                if (add || cf.Contains("poc")) f.Values["personOrg"] = s.person_org_code != null &&
                    organizations.TryGetValue(s.person_org_code, out var organizationKey) ? organizationKey : "-1"; // LOOKUP
                if (add || cf.Contains("br")) f.Values["bill_rate"] = s.bill_rate?.ToString();
                if (add || cf.Contains("cr")) f.Values["cost_rate"] = s.cost_rate?.ToString();
                if (add || cf.Contains("tag")) f.Values["leave_request"] = s.time_approval_group != null && // FIX SOURCE
                    approvalGroups.TryGetValue(s.time_approval_group, out var approvalGroupKey) ? approvalGroupKey : "-1"; // LOOKUP
                if (add || cf.Contains("tag")) f.Values["time_chain"] = s.time_approval_group != null &&
                    approvalGroups.TryGetValue(s.time_approval_group, out var approvalGroupKey) ? approvalGroupKey : "-1"; // LOOKUP
                //
                if (add || cf.Contains("a")) f.Checked["active"] = s.active == "Y";
                if (add || cf.Contains("te")) f.Checked["timesheet_email"] = s.timesheet_emails == "Y";
                if (add || cf.Contains("ee")) f.Checked["expense_email"] = s.expense_emails == "Y";
                if (add || cf.Contains("at")) f.Checked["timesheet_lines"] = s.autofill_timesheet == "Y";
                if (add || cf.Contains("eaa")) f.Values["expense_approval_amount"] = s.expense_approval_amount;
                if (add || cf.Contains("ed")) f.Values["rate_begin_date"] = s.effective_date?.ToString("M/d/yyyy");
                if (f.Values.ContainsKey("end_date")) f.Values["end_date"] = "0";
                //if (add || cf.Contains("dp")) f.Values["xxxx"] = s.dilution_period;
                //if (add || cf.Contains("dpo")) f.Values["xxxx"] = s.default_project_org;
                //if (add || cf.Contains("dp2")) f.Values["xxxx"] = s.default_project;
                //if (add || cf.Contains("dt")) f.Values["xxxx"] = s.default_task;
                //
                if (add || cf.Contains("dlc")) f.Values["labor_category"] = s.default_labor_category != null &&
                    laborCategories.TryGetValue(s.default_labor_category, out var laborCategoryKey) ? laborCategoryKey : "-1"; // LOOKUP
                if (add || cf.Contains("dpm")) f.FromSelect("payment_method_key", s.default_payment_method);
                if (add || cf.Contains("tr")) f.FromSelectByKey("titoRequired", s.tito_required);
                if (add || cf.Contains("bw")) f.FromSelect("businessWeek", s.business_week);
                if (add || cf.Contains("ae")) f.Checked["assignment_email"] = s.assignment_emails == "Y";
                //
                if (add || cf.Contains("u1")) f.Values["udf_0"] = s.user01;
                if (add || cf.Contains("u2")) f.Values["udf_1"] = s.user02;
                //if (add || cf.Contains("u3")) f.Values["udf_2"] = s.user03;
                if (add || cf.Contains("u4")) f.Values["udf_3"] = s.user04;
                if (add || cf.Contains("u5")) f.Values["udf_4"] = s.user05;
                //if (add || cf.Contains("u6")) f.Values["udf_5"] = s.user06;
                //if (add || cf.Contains("u7")) f.Values["udf_6"] = s.user07;
                //if (add || cf.Contains("u8")) f.Values["udf_7"] = s.user08;
                if (add || cf.Contains("u9")) f.Values["udf_8"] = s.user09;
                if (add || cf.Contains("u10")) f.Values["udf_9"] = s.user10;
                //
                if (add || cf.Contains("hd")) f.Values["hire_date"] = s.hire_date?.ToString("M/d/yyyy");
                if (add || cf.Contains("pc3")) f.FromSelectByPredicate("paymentCurrency", s.payment_currency, x => x.Value.StartsWith(s.payment_currency));
                if (add || cf.Contains("cs")) f.FromSelectByPredicate("costStructLabor", s.cost_structure, x => x.Value.StartsWith(s.cost_structure));
                //if (add || cf.Contains("ce")) f.Values["xxxx"] = s.cost_element;
                //if (add || cf.Contains("ul")) f.Values["xxxx"] = s.unlock;
                if (add || cf.Contains("l")) f.Values["location"] = s.location != null &&
                    locations.TryGetValue(s.location, out var locationKey) ? locationKey : "-1"; // LOOKUP
                //
                if (add || cf.Contains("et")) f.FromSelect("employeeType", s.employee_type);
                //if (add || cf.Contains("hv")) f.Values["hide_vat"] = s.hide_vat;
                //if (add || cf.Contains("lre")) f.Values["xxxx"] = s.leave_request_emails;
                //if (add || cf.Contains("tu")) f.Values["xxxx"] = s.tbd_user;
                if (add || cf.Contains("tv")) f.Values["timeVendor"] = s.time_vendor != null &&
                    vendorProfiles.TryGetValue(s.time_vendor, out var vendorKey) ? vendorKey : null; // LOOKUP
                if (add || cf.Contains("ev")) f.Values["expenseVendor"] = s.expense_vendor != null &&
                    vendorProfiles.TryGetValue(s.expense_vendor, out var vendorKey) ? vendorKey : null; // LOOKUP
                //
                //if (add || cf.Contains("phd")) f.Values["xxxx"] = s.payroll_hire_date?.ToString("M/d/yyyy");
                //if (add || cf.Contains("pms")) f.Values["xxxx"] = s.payroll_marital_status;
                //if (add || cf.Contains("pfe")) f.Values["xxxx"] = s.payroll_federal_exemptions;
                //if (add || cf.Contains("pstc")) f.Values["xxxx"] = s.payroll_sui_tax_code;
                //if (add || cf.Contains("pswi")) f.Values["xxxx"] = s.payroll_state_worked_in;
                //if (add || cf.Contains("pis")) f.Values["xxxx"] = s.payroll_immigration_status;
                //if (add || cf.Contains("pec")) f.Values["xxxx"] = s.payroll_eeo_code;
                //if (add || cf.Contains("pmp")) f.Values["xxxx"] = s.payroll_medical_plan;
                //if (add || cf.Contains("plrcd")) f.Values["xxxx"] = s.payroll_last_rate_change_date;
                //if (add || cf.Contains("plrc")) f.Values["xxxx"] = s.payroll_last_rate_change;
                //
                f.Add("button_save", "action", null);
                // edit rate row for effective_date|exempt_status|costStructLabor|bill_rate|cost_rate
                if (!add && cf.Contains("ed") || cf.Contains("es") || cf.Contains("cs") || cf.Contains("br") || cf.Contains("cr"))
                {
                    var d1 = z.ExtractSpanInner("<table id=\"rates\"", "</table>");
                    var rows = una.ParseList(d1, "k_");
                    if (rows.Keys.Count != 1)
                        throw new InvalidOperationException("MANUAL: found none/multiple rate rows");
                    var rateKey = rows.Keys.Single();
                    f.Values["isRateEdit"] = "true";
                    f.Values["redisplayRateEdit"] = "true";
                    f.Values["selectedRateKey"] = rateKey;
                }
            });
            return r != null ?
                ManageFlags.PersonChanged :
                ManageFlags.None;
        }
    }
}
