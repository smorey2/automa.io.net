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
        public string key { get; set; }
        //
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
        // NEW
        public string person_purchase_approval_amt { get; set; }
        public string person_purchase_email { get; set; }
        public string person_approval_grp_timesheet { get; set; }
        public string person_approval_grp_leave { get; set; }
        public string person_approval_grp_exp_rep { get; set; }
        public string person_approval_grp_exp_req { get; set; }
        public string person_approval_grp_po { get; set; }
        public string person_approval_grp_pr { get; set; }
        public string person_approval_grp_vi { get; set; }
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
        public string vendor_invoice_person { get; set; }
        public string po_form_title { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.person.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.person.key, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity ?? una.Settings.LegalEntity);
                f.Checked["exempt"] = true; f.Checked["nonExempt"] = true; f.Checked["nonEmployee"] = true;
            }, sourceFolder));
        }

        public static IEnumerable<PersonModel> Read(UnanetClient una, string sourceFolder)
        {
            string CleanRoles(string roles) => ("," + roles)
                .Replace(",projectApprover", string.Empty)
                .Replace(",projectLead", string.Empty)
                .Replace(",poOwner", string.Empty)
                .Substring(1);
            var filePath = Path.Combine(sourceFolder, una.Settings.person.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new PersonModel
                {
                    key = x[0],
                    //
                    username = x[1],
                    first_name = x[2],
                    last_name = x[3],
                    middle_initial = x[4],
                    suffix = x[5],
                    nickname = x[6],
                    //
                    exempt_status = x[7],
                    roles = CleanRoles(x[8]),
                    time_period = x[9],
                    pay_code = x[10],
                    hour_increment = x[11].ToDecimal(),
                    //expense_approval_group = x[12],
                    //
                    person_code = x[13],
                    id_code_1 = x[14],
                    id_code_2 = x[15],
                    password = x[16],
                    ivr_password = x[17],
                    email = x[18],
                    person_org_code = x[19],
                    bill_rate = x[20].ToDecimal(),
                    cost_rate = x[21].ToDecimal(),
                    time_approval_group = x[22],
                    //
                    active = x[23],
                    timesheet_emails = x[24],
                    expense_emails = x[25],
                    autofill_timesheet = x[26],
                    expense_approval_amount = x[27],
                    effective_date = x[28].ToDateTime(),
                    dilution_period = x[29],
                    default_project_org = x[30],
                    default_project = x[31],
                    default_task = x[32],
                    //
                    default_labor_category = x[33],
                    default_payment_method = x[34],
                    tito_required = x[35],
                    business_week = x[36],
                    assignment_emails = x[37],
                    //
                    user01 = x[38],
                    user02 = x[39],
                    user03 = x[40],
                    user04 = x[41],
                    user05 = x[42],
                    user06 = x[43],
                    user07 = x[44],
                    user08 = x[45],
                    user09 = x[46],
                    user10 = x[47],
                    //
                    hire_date = x[48].ToDateTime(),
                    payment_currency = x[49],
                    delete = x[50],
                    cost_structure = x[51],
                    cost_element = x[52],
                    unlock = x[53],
                    location = x[54],
                    //
                    employee_type = x[55],
                    hide_vat = x[56],
                    leave_request_emails = x[57],
                    tbd_user = x[58],
                    time_vendor = x[59],
                    expense_vendor = x[60],
                    //
                    payroll_hire_date = x[61].ToDateTime(),
                    payroll_marital_status = x[62],
                    payroll_federal_exemptions = x[63],
                    payroll_sui_tax_code = x[64],
                    payroll_state_worked_in = x[65],
                    payroll_immigration_status = x[66],
                    payroll_eeo_code = x[67],
                    payroll_medical_plan = x[68],
                    payroll_last_rate_change_date = x[69],
                    payroll_last_rate_change = x[70],
                    // NEW
                    person_purchase_approval_amt = x[71],
                    person_purchase_email = x[72],
                    person_approval_grp_timesheet = x[73],
                    person_approval_grp_leave = x[74],
                    person_approval_grp_exp_rep = x[75], // expense_approval_group
                    person_approval_grp_exp_req = x[76], // expense_approval_group
                    person_approval_grp_po = x[77],
                    person_approval_grp_pr = x[78],
                    person_approval_grp_vi = x[79],
                    //
                    user11 = x[80],
                    user12 = x[81],
                    user13 = x[82],
                    user14 = x[83],
                    user15 = x[84],
                    user16 = x[85],
                    user17 = x[86],
                    user18 = x[87],
                    user19 = x[88],
                    user20 = x[89],
                    //
                    vendor_invoice_person = x[90],
                    po_form_title = x[91],
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
                XAttribute("phd", x.payroll_hire_date), XAttribute("pm", x.payroll_marital_status), XAttribute("pfe", x.payroll_federal_exemptions), XAttribute("pstc", x.payroll_sui_tax_code), XAttribute("pswi", x.payroll_state_worked_in), XAttribute("pis", x.payroll_immigration_status), XAttribute("pec", x.payroll_eeo_code), XAttribute("pmp", x.payroll_medical_plan), XAttribute("plrcd", x.payroll_last_rate_change_date), XAttribute("plrc", x.payroll_last_rate_change),
                // NEW
                XAttribute("ppaa", x.person_purchase_approval_amt), XAttribute("ppe", x.person_purchase_email),
                XAttribute("pagt", x.person_approval_grp_timesheet), XAttribute("pagl", x.person_approval_grp_leave), XAttribute("pager", x.person_approval_grp_exp_rep), XAttribute("pager2", x.person_approval_grp_exp_req), XAttribute("pagp", x.person_approval_grp_po), XAttribute("pagp2", x.person_approval_grp_pr), XAttribute("pagv", x.person_approval_grp_vi),
                XAttribute("u11", x.user11), XAttribute("u12", x.user12), XAttribute("u13", x.user13), XAttribute("u14", x.user14), XAttribute("u15", x.user15), XAttribute("u16", x.user16), XAttribute("u17", x.user17), XAttribute("u18", x.user18), XAttribute("u19", x.user19), XAttribute("u20", x.user20),
                XAttribute("vip", x.vendor_invoice_person), XAttribute("pft", x.po_form_title)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".p.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Person1 : PersonModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_Person1 s, out Dictionary<string, (Type, object)> fields, out string last, string[] restrictedUsernames, Action<p_Person1> bespoke = null)
        {
            var _f = fields = new Dictionary<string, (Type, object)>();
            T _t<T>(T value, string name) { _f[name] = (typeof(T), value); return value; }
            //
            bespoke?.Invoke(s);
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out last))
                return ManageFlags.PersonChanged;
            var locations = Unanet.Lookups.Locations.Value;
            var approvalGroups = Unanet.Lookups.TimeExpense.Value;
            var laborCategories = Unanet.Lookups.LaborCategories.Value;
            var vendorProfiles = Unanet.Lookups.Vendors.Value;
            var organizations = Unanet.Lookups.CostCenters.Value;
            //
            if (s.id_code_1 == "C:AAAAAAAAAAAAAAAAAAAAAA")
                throw new InvalidOperationException($"{s.username} has a {nameof(s.id_code_1)}");
            if (!approvalGroups.ContainsKey(s.expense_approval_group))
                throw new ArgumentOutOfRangeException(nameof(s.expense_approval_group), s.expense_approval_group);
            if (restrictedUsernames != null && restrictedUsernames.Contains(s.username))
                throw new InvalidOperationException($"{s.username} restricted user, please update manually.");
            var r = una.SubmitManage(add ? HttpMethod.Post : HttpMethod.Put, "people",
                $"personkey={s.key}",
                out last, (z, f) =>
            {
                if (add || cf.Contains("u")) f.Values["username"] = _t(s.username, nameof(s.username));
                if (add || cf.Contains("fn")) f.Values["first_name"] = _t(s.first_name, nameof(s.first_name));
                if (add || cf.Contains("ln")) f.Values["last_name"] = _t(s.last_name, nameof(s.last_name));
                if (add || cf.Contains("mi")) f.Values["middleInitial"] = _t(s.middle_initial, nameof(s.middle_initial));
                if (add || cf.Contains("s")) f.Values["suffix"] = _t(s.suffix, nameof(s.suffix));
                //if (add || cf.Contains("n")) f.Values["xxxx"] = _t(s.nickname, nameof(s.nickname));
                //
                if (add || cf.Contains("es")) f.FromSelectByKey("exempt", _t(s.exempt_status, nameof(s.exempt_status)));
                if (add || cf.Contains("r")) f.FromMultiCheckbox("rolenames", _t(s.roles, nameof(s.roles)));

                if (add || cf.Contains("tp")) f.FromSelect("timePeriod", _t(s.time_period, nameof(s.time_period)));
                if (add || cf.Contains("pc")) f.FromSelect("payCode", _t(s.pay_code, nameof(s.pay_code)));
                if (add || cf.Contains("hi")) f.FromSelectByKey("hour_increment", _t(s.hour_increment, nameof(s.hour_increment))?.ToString());
                //if (add || cf.Contains("eag")) f.Values["expense_request_chain"] = GetLookupValue(approvalGroups, _t(s.expense_approval_group, nameof(s.expense_approval_group)));
                //if (add || cf.Contains("eag")) f.Values["expense_report_chain"] = GetLookupValue(approvalGroups, _t(s.expense_approval_group, nameof(s.expense_approval_group)));
                //
                if (add || cf.Contains("pc2")) f.Values["person_code"] = _t(s.person_code, nameof(s.person_code));
                if (add || cf.Contains("ic1") || cf.Contains("bind")) f.Values["empId"] = _t(s.id_code_1, nameof(s.id_code_1));
                if (add || cf.Contains("ic2")) f.Values["ssn"] = _t(s.id_code_2, nameof(s.id_code_2));
                //if (add || cf.Contains("p")) f.Values["password1"] = _t(s.password, nameof(s.password));
                //if (add || cf.Contains("ip")) f.Values["password2"] = _t(s.ivr_password, nameof(s.xxivr_passwordxx));
                if (add || cf.Contains("e")) f.Values["email"] = _t(s.email, nameof(s.email));
                if (add || cf.Contains("poc")) f.Values["personOrg"] = GetLookupValue(organizations, _t(s.person_org_code, nameof(s.person_org_code)));
                if (add || cf.Contains("br")) f.Values["bill_rate"] = _t(s.bill_rate, nameof(s.bill_rate))?.ToString();
                if (add || cf.Contains("cr")) f.Values["cost_rate"] = _t(s.cost_rate, nameof(s.cost_rate))?.ToString();
                //if (add || cf.Contains("tag")) f.Values["leave_request"] = GetLookupValue(approvalGroups, _t(s.time_approval_group, nameof(s.time_approval_group)));
                //if (add || cf.Contains("tag")) f.Values["time_chain"] = GetLookupValue(approvalGroups, _t(s.time_approval_group, nameof(s.time_approval_group)));
                //
                if (add || cf.Contains("a")) f.Checked["active"] = _t(s.active, nameof(s.active)) == "Y";
                if (add || cf.Contains("te")) f.Checked["timesheet_email"] = _t(s.timesheet_emails, nameof(s.timesheet_emails)) == "Y";
                if (add || cf.Contains("ee")) f.Checked["expense_email"] = _t(s.expense_emails, nameof(s.expense_emails)) == "Y";
                if (add || cf.Contains("at")) f.Checked["timesheet_lines"] = _t(s.autofill_timesheet, nameof(s.autofill_timesheet)) == "Y";
                if (add || cf.Contains("eaa")) f.Values["expense_approval_amount"] = _t(s.expense_approval_amount, nameof(s.expense_approval_amount));
                if (add || cf.Contains("ed")) f.Values["rate_begin_date"] = _t(s.effective_date, nameof(s.effective_date))?.ToString("M/d/yyyy");
                if (f.Values.ContainsKey("end_date")) f.Values["end_date"] = "0";
                //if (add || cf.Contains("dp")) f.Values["xxxx"] = _t(s.dilution_period, nameof(s.dilution_period));
                //if (add || cf.Contains("dpo")) f.Values["xxxx"] = _t(s.default_project_org, nameof(s.default_project_org));
                //if (add || cf.Contains("dp2")) f.Values["xxxx"] = _t(s.default_project, nameof(s.default_project));
                //if (add || cf.Contains("dt")) f.Values["xxxx"] = _t(s.default_task, nameof(s.default_task));
                //
                if (add || cf.Contains("dlc")) f.Values["labor_category"] = GetLookupValue(laborCategories, _t(s.default_labor_category, nameof(s.default_labor_category)));
                if (add || cf.Contains("dpm")) f.FromSelect("payment_method_key", _t(s.default_payment_method, nameof(s.default_payment_method)));
                if (add || cf.Contains("tr")) f.FromSelectByKey("titoRequired", _t(s.tito_required, nameof(s.tito_required)));
                if (add || cf.Contains("bw")) f.FromSelect("businessWeek", _t(s.business_week, nameof(s.business_week)));
                if (add || cf.Contains("ae")) f.Checked["assignment_email"] = _t(s.assignment_emails, nameof(s.assignment_emails)) == "Y";
                //
                if (add || cf.Contains("u1")) f.Values["udf_0"] = _t(s.user01, nameof(s.user01));
                if (add || cf.Contains("u2")) f.Values["udf_1"] = _t(s.user02, nameof(s.user02));
                //if (add || cf.Contains("u3")) f.Values["udf_2"] = _t(s.user03, nameof(s.user03));
                if (add || cf.Contains("u4")) f.Values["udf_3"] = _t(s.user04, nameof(s.user04));
                if (add || cf.Contains("u5")) f.Values["udf_4"] = _t(s.user05, nameof(s.user05));
                //if (add || cf.Contains("u6")) f.Values["udf_5"] = _t(s.user06, nameof(s.user06));
                //if (add || cf.Contains("u7")) f.Values["udf_6"] = _t(s.user07, nameof(s.user07));
                //if (add || cf.Contains("u8")) f.Values["udf_7"] = _t(s.user08, nameof(s.user08));
                if (add || cf.Contains("u9")) f.Values["udf_8"] = _t(s.user09, nameof(s.user09));
                if (add || cf.Contains("u10")) f.Values["udf_9"] = _t(s.user10, nameof(s.user10));
                //
                if (add || cf.Contains("hd")) f.Values["hire_date"] = _t(s.hire_date, nameof(s.hire_date))?.ToString("M/d/yyyy");
                if (add || cf.Contains("pc3")) f.FromSelectByPredicate("paymentCurrency", _t(s.payment_currency, nameof(s.payment_currency)), x => x.Value.StartsWith(s.payment_currency));
                if (add || cf.Contains("cs")) f.FromSelectByPredicate("costStructLabor", _t(s.cost_structure, nameof(s.cost_structure)), x => x.Value.StartsWith(s.cost_structure));
                //if (add || cf.Contains("ce")) f.Values["xxxx"] = _t(s.cost_element, nameof(s.cost_element));
                //if (add || cf.Contains("ul")) f.Values["xxxx"] = _t(s.unlock, nameof(s.unlock));
                if (add || cf.Contains("l")) f.Values["location"] = GetLookupValue(locations, _t(s.location, nameof(s.location)));
                //
                if (add || cf.Contains("et")) f.FromSelect("employeeType", _t(s.employee_type, nameof(s.employee_type)));
                //if (add || cf.Contains("hv")) f.Values["hide_vat"] = _t(s.hide_vat, nameof(s.hide_vat));
                //if (add || cf.Contains("lre")) f.Values["xxxx"] = _t(s.leave_request_emails, nameof(s.leave_request_emails));
                //if (add || cf.Contains("tu")) f.Values["xxxx"] = _t(s.tbd_user, nameof(s.tbd_user));
                if (add || cf.Contains("tv")) f.Values["timeVendor"] = GetLookupValue(vendorProfiles, _t(s.time_vendor, nameof(s.time_vendor)));
                if (add || cf.Contains("ev")) f.Values["expenseVendor"] = GetLookupValue(vendorProfiles, _t(s.expense_vendor, nameof(s.expense_vendor)));

                //if (add || cf.Contains("phd")) f.Values["xxxx"] = _t(s.payroll_hire_date, nameof(s.payroll_hire_date))?.ToString("M/d/yyyy");
                //if (add || cf.Contains("pms")) f.Values["xxxx"] = _t(s.payroll_marital_status, nameof(s.payroll_marital_status));
                //if (add || cf.Contains("pfe")) f.Values["xxxx"] = _t(s.payroll_federal_exemptions, nameof(s.payroll_federal_exemptions));
                //if (add || cf.Contains("pstc")) f.Values["xxxx"] = _t(s.payroll_sui_tax_code, nameof(s.payroll_sui_tax_code));
                //if (add || cf.Contains("pswi")) f.Values["xxxx"] = _t(s.payroll_state_worked_in, nameof(s.payroll_state_worked_in));
                //if (add || cf.Contains("pis")) f.Values["xxxx"] = _t(s.payroll_immigration_status, nameof(s.payroll_immigration_status));
                //if (add || cf.Contains("pec")) f.Values["xxxx"] = _t(s.payroll_eeo_code, nameof(s.payroll_eeo_code));
                //if (add || cf.Contains("pmp")) f.Values["xxxx"] = _t(s.payroll_medical_plan, nameof(s.payroll_medical_plan));
                //if (add || cf.Contains("plrcd")) f.Values["xxxx"] = _t(s.payroll_last_rate_change_date, nameof(s.payroll_last_rate_change_date));
                //if (add || cf.Contains("plrc")) f.Values["xxxx"] = _t(s.payroll_last_rate_change, nameof(s.payroll_last_rate_change));

                // NEW
                //if (add || cf.Contains("ppaa")) f.Values["xxxx"] = _t(s.person_purchase_approval_amt, nameof(s.person_purchase_approval_amt));
                //if (add || cf.Contains("ppe")) f.Checked["xxxx"] = _t(s.person_purchase_email, nameof(s.person_purchase_email)) == "Y";
                //
                if (add || cf.Contains("pagt")) f.Values["time_chain"] = GetLookupValue(approvalGroups, _t(s.person_approval_grp_timesheet, nameof(s.person_approval_grp_timesheet)), missingThrows: true);
                if (add || cf.Contains("pagl")) f.Values["leave_request"] = GetLookupValue(approvalGroups, _t(s.person_approval_grp_leave, nameof(s.person_approval_grp_leave)), missingThrows: true);
                if (add || cf.Contains("pager")) f.Values["expense_report_chain"] = GetLookupValue(approvalGroups, _t(s.person_approval_grp_exp_rep, nameof(s.person_approval_grp_exp_rep)), missingThrows: true);
                if (add || cf.Contains("pager2")) f.Values["expense_request_chain"] = GetLookupValue(approvalGroups, _t(s.person_approval_grp_exp_req, nameof(s.person_approval_grp_exp_req)), missingThrows: true);
                //if (add || cf.Contains("pagp")) f.Values["xxxx"] = _t(s.person_approval_grp_po, nameof(s.person_approval_grp_po));
                //if (add || cf.Contains("pagp2")) f.Values["xxxx"] = _t(s.person_approval_grp_pr, nameof(s.person_approval_grp_pr));
                //if (add || cf.Contains("pagv")) f.Values["xxxx"] = _t(s.person_approval_grp_vi, nameof(s.person_approval_grp_vi));
                //
                //if (add || cf.Contains("u11")) f.Values["udf_10"] = _t(s.user11, nameof(s.user11));
                //if (add || cf.Contains("u12")) f.Values["udf_11"] = _t(s.user12, nameof(s.user12));
                //if (add || cf.Contains("u13")) f.Values["udf_12"] = _t(s.user13, nameof(s.user13));
                //if (add || cf.Contains("u14")) f.Values["udf_13"] = _t(s.user14, nameof(s.user14));
                //if (add || cf.Contains("u15")) f.Values["udf_14"] = _t(s.user15, nameof(s.user15));
                //if (add || cf.Contains("u16")) f.Values["udf_15"] = _t(s.user16, nameof(s.user16));
                //if (add || cf.Contains("u17")) f.Values["udf_16"] = _t(s.user17, nameof(s.user17));
                //if (add || cf.Contains("u18")) f.Values["udf_17"] = _t(s.user18, nameof(s.user18));
                //if (add || cf.Contains("u19")) f.Values["udf_18"] = _t(s.user19, nameof(s.user19));
                //if (add || cf.Contains("u20")) f.Values["udf_19"] = _t(s.user20, nameof(s.user20));
                //
                //if (add || cf.Contains("vip")) f.Checked["xxxx"] = _t(s.vendor_invoice_person, nameof(s.vendor_invoice_person)) == "Y";
                //if (add || cf.Contains("pft")) f.Values["xxxx"] = _t(s.po_form_title, nameof(s.po_form_title));

                f.Add("button_save", "action", null);
                // edit rate row for effective_date|exempt_status|costStructLabor|bill_rate|cost_rate
                if (!add && cf.Contains("ed") || cf.Contains("es") || cf.Contains("cs") || cf.Contains("br") || cf.Contains("cr"))
                {
                    throw new InvalidOperationException("MANUAL: finance to change rate information");
                    //var d1 = z.ExtractSpanInner("<table id=\"rates\"", "</table>");
                    //var rows = una.ParseList(d1, "k_");
                    //if (rows.Keys.Count != 1)
                    //    throw new InvalidOperationException("MANUAL: found none/multiple rate rows");
                    //var rateKey = rows.Keys.Single();
                    //f.Values["isRateEdit"] = "true";
                    //f.Values["redisplayRateEdit"] = "true";
                    //f.Values["selectedRateKey"] = rateKey;
                }
            });
            return r != null ?
                ManageFlags.PersonChanged :
                ManageFlags.None;
        }
    }
}
