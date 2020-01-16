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
    public class ProjectInvoiceSetupModel : ModelBase
    {
        public string project_org_code { get; set; }
        public string project_code { get; set; }
        public string generate_invoice { get; set; }
        public string invoicing_option { get; set; }
        public string lead_project_org_code { get; set; }
        public string lead_project_code { get; set; }
        public string primary_invoice_format { get; set; }
        public string additional_invoice_formats { get; set; }
        public string invoice_number_format { get; set; }
        public string payment_terms { get; set; }
        //
        public string bill_to_contact { get; set; }
        public string bill_to_address { get; set; }
        public string ship_to_contact { get; set; }
        public string ship_to_address { get; set; }
        public string remit_to_contact { get; set; }
        public string remit_to_address { get; set; }
        public string invoice_delivery_method { get; set; }
        public string email_message_template { get; set; }
        public string to_email_list { get; set; }
        public string cc_email_list { get; set; }
        public string bcc_email_list { get; set; }
        public string req_delivery_receipt { get; set; }
        public string req_read_receipt { get; set; }
        //
        public string show_project_org_code { get; set; }
        public string show_project_code { get; set; }
        public string show_project_title { get; set; }
        public string show_project_funded_value { get; set; }
        public string show_company_logo { get; set; }
        public string company_logo { get; set; }
        public string show_contract_number { get; set; }
        public string contract_number { get; set; }
        public string show_order_number { get; set; }
        public string order_number { get; set; }
        public string description { get; set; }
        public string invoice_memo { get; set; }
        // custom
        public string project_codeKey { get; set; }
        public string usernameKey { get; set; }
        public string lead_project_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.project_invoice_setup.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.project_invoice_setup.key, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity ?? una.Settings.LegalEntity);
            }, sourceFolder));
        }

        public static IEnumerable<ProjectInvoiceSetupModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.project_invoice_setup.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new ProjectInvoiceSetupModel
                {
                    project_org_code = x[0],
                    project_code = x[1],
                    generate_invoice = x[2],
                    invoicing_option = x[3],
                    lead_project_org_code = x[4],
                    lead_project_code = x[5],
                    primary_invoice_format = x[6],
                    additional_invoice_formats = x[7],
                    invoice_number_format = x[8],
                    payment_terms = x[9],
                    //
                    bill_to_contact = x[10],
                    bill_to_address = x[11],
                    ship_to_contact = x[12],
                    ship_to_address = x[13],
                    remit_to_contact = x[14],
                    remit_to_address = x[15],
                    invoice_delivery_method = x[16],
                    email_message_template = x[17],
                    to_email_list = x[18],
                    cc_email_list = x[19],
                    bcc_email_list = x[20],
                    req_delivery_receipt = x[21],
                    req_read_receipt = x[22],
                    //
                    show_project_org_code = x[23],
                    show_project_code = x[24],
                    show_project_title = x[25],
                    show_project_funded_value = x[26],
                    show_company_logo = x[27],
                    company_logo = x[28],
                    show_contract_number = x[29],
                    contract_number = x[30],
                    show_order_number = x[31],
                    order_number = x[32],
                    description = x[33],
                    invoice_memo = x[34],
                }, 1)
                .ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("gi", x.generate_invoice), XAttribute("io", x.invoicing_option), XAttribute("lpoc", x.lead_project_org_code), XAttribute("lpc", x.lead_project_code), XAttribute("pif", x.primary_invoice_format), XAttribute("aif", x.additional_invoice_formats), XAttribute("inf", x.invoice_number_format), XAttribute("pt", x.payment_terms),
                XAttribute("btc", x.bill_to_contact), XAttribute("bta", x.bill_to_address), XAttribute("stc", x.ship_to_contact), XAttribute("sta", x.ship_to_address), XAttribute("rtc", x.remit_to_contact), XAttribute("rta", x.remit_to_address), XAttribute("idm", x.invoice_delivery_method), XAttribute("emt", x.email_message_template), XAttribute("tel", x.to_email_list), XAttribute("cel", x.cc_email_list), XAttribute("bel", x.bcc_email_list), XAttribute("rdr", x.req_delivery_receipt), XAttribute("rrr", x.req_read_receipt),
                XAttribute("spoc", x.show_project_org_code), XAttribute("spc", x.show_project_code), XAttribute("spt", x.show_project_title), XAttribute("spfv", x.show_project_funded_value), XAttribute("scl", x.show_company_logo), XAttribute("cl", x.company_logo), XAttribute("scn", x.show_contract_number), XAttribute("cn", x.contract_number), XAttribute("son", x.show_order_number), XAttribute("on", x.order_number), XAttribute("d", x.description), XAttribute("im", x.invoice_memo)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".j_pis.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_ProjectInvoiceSetup1 : ProjectInvoiceSetupModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_ProjectInvoiceSetup1 s, out Dictionary<string, (Type, object)> fields, out string last, Action<p_ProjectInvoiceSetup1> bespoke = null)
        {
            var _f = fields = new Dictionary<string, (Type, object)>();
            T _t<T>(T value, string name) { _f[name] = (typeof(T), value); return value; }
            //
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.ProjectInvoiceSetupChanged;
            var r = una.SubmitSubManage("D", HttpMethod.Get, $"projects/accounting/invoice/edit", null,
                $"projectkey={s.project_codeKey}", null,
                out last, (z, f) =>
            {
                if (add || cf.Contains("gi")) f.Checked["generateInvoice"] = _t(s.generate_invoice, nameof(s.generate_invoice)) == "Y";
                if (add || cf.Contains("io")) { f.Values["proj_invoice"] = _t(s.invoicing_option, nameof(s.invoicing_option)); f.Values["invoice_option"] = s.invoicing_option; }
                if (s.invoicing_option != "C")
                {
                    f.Values["lead_projects_mod"] = "true";
                    f.Types["lead_projects"] = "disabled";
                }
                else
                {
                    //if (add || cf.Contains("lpoc")) f.Values["xxxx"] = _t(s.lead_project_org_code, nameof(s.lead_project_org_code));
                    //if (add || cf.Contains("lpc")) f.Values["xxxx"] = _t(s.lead_project_code, nameof(s.lead_project_code));
                    f.Types["contributorsnotAssigned_orgCode_fltr"] = "disabled";
                    f.Types["contributorsnotAssigned_projCode_fltr"] = "disabled";
                    f.Values["lead_projects_mod"] = "false";
                    f.Values["lead_projects"] = _t(s.lead_project_codeKey, nameof(s.lead_project_codeKey));
                }
                //
                if (s.invoicing_option != "C")
                {
                    if (add || cf.Contains("pif")) f.FromSelect("invoiceFormat", _t(s.primary_invoice_format, nameof(s.primary_invoice_format)));
                    //if (add || cf.Contains("aif")) f.FromSelect("xxxx", _t(s.additional_invoice_formats, nameof(s.additional_invoice_formats)));
                    if (add || cf.Contains("inf")) f.FromSelectByPredicate("invoiceNumber", _t(s.invoice_number_format, nameof(s.invoice_number_format)), x => x.Value.StartsWith(s.invoice_number_format));
                    if (add || cf.Contains("pt")) f.FromSelect("paymentTerm", _t(s.payment_terms, nameof(s.payment_terms)));
                }
                else
                {
                    f.Types["invoiceFormat"] = "disabled";
                    //f.Types["xxxx"] = "disabled";
                    f.Types["invoiceNumber"] = "disabled";
                    f.Types["paymentTerm"] = "disabled";
                }
                //
                if (s.invoicing_option != "C")
                {
                    if (add || cf.Contains("btc")) f.FromSelect("bill_to_contact", _t(s.bill_to_contact, nameof(s.bill_to_contact)));
                    if (add || cf.Contains("bta")) f.FromSelect("bill_to_address", !string.IsNullOrEmpty(_t(s.bill_to_address, nameof(s.bill_to_address))) ? s.bill_to_address : f.Selects["bill_to_address"].First().Value);
                    if (add || cf.Contains("stc")) f.FromSelect("ship_to_contact", _t(s.ship_to_contact, nameof(s.ship_to_contact)));
                    if (add || cf.Contains("sta")) f.FromSelect("ship_to_address", _t(s.ship_to_address, nameof(s.ship_to_address)));
                    if (add || cf.Contains("rtc")) f.FromSelect("remit_to_contact", _t(s.remit_to_contact, nameof(s.remit_to_contact)));
                    if (add || cf.Contains("rta")) f.FromSelect("remit_to_address", !string.IsNullOrEmpty(_t(s.remit_to_address, nameof(s.remit_to_address))) ? s.remit_to_address : f.Selects["remit_to_address"].First().Value);
                    if (add || cf.Contains("idm")) f.FromSelectByKey("invoice_delivery_opt", _t(s.invoice_delivery_method, nameof(s.invoice_delivery_method)));
                    if (add || cf.Contains("emt")) f.FromSelect("emailTemplate", _t(s.email_message_template, nameof(s.email_message_template)));
                    if (add || cf.Contains("tel")) f.Values["toEmail"] = _t(s.to_email_list, nameof(s.to_email_list));
                    if (add || cf.Contains("cel")) f.Values["ccEmail"] = _t(s.cc_email_list, nameof(s.cc_email_list));
                    if (add || cf.Contains("bel")) f.Values["bccEmail"] = _t(s.bcc_email_list, nameof(s.bcc_email_list));
                    if (add || cf.Contains("rdr")) f.Checked["delivery_req"] = _t(s.req_delivery_receipt, nameof(s.req_delivery_receipt)) == "Y";
                    if (add || cf.Contains("rrr")) f.Checked["read_req"] = _t(s.req_read_receipt, nameof(s.req_read_receipt)) == "Y";
                }
                else
                {
                    f.Types["bill_to_contact"] = "disabled";
                    f.Types["bill_to_address"] = "disabled";
                    f.Types["ship_to_contact"] = "disabled";
                    f.Types["ship_to_address"] = "disabled";
                    f.Types["remit_to_contact"] = "disabled";
                    f.Types["remit_to_address"] = "disabled";
                    f.Types["invoice_delivery_opt"] = "disabled";
                    f.Types["emailTemplate"] = "disabled";
                    f.Types["toEmail"] = "disabled";
                    f.Types["ccEmail"] = "disabled";
                    f.Types["bccEmail"] = "disabled";
                    f.Types["delivery_req"] = "disabled";
                    f.Types["read_req"] = "disabled";
                }
                //
                if (s.invoicing_option != "C")
                {
                    if (add || cf.Contains("spoc")) f.Checked["showProjectOrgCode"] = _t(s.show_project_org_code, nameof(s.show_project_org_code)) == "Y";
                    if (add || cf.Contains("spc")) f.Checked["showProjectCode"] = _t(s.show_project_code, nameof(s.show_project_code)) == "Y";
                    if (add || cf.Contains("spt")) f.Checked["showProjectTitle"] = _t(s.show_project_title, nameof(s.show_project_title)) == "Y";
                    if (add || cf.Contains("spfv")) f.Checked["showProjectFundedValue"] = _t(s.show_project_funded_value, nameof(s.show_project_funded_value)) == "Y";
                    if (add || cf.Contains("scl")) f.Checked["showCompanyLogo"] = _t(s.show_company_logo, nameof(s.show_company_logo)) == "Y";
                    if (add || cf.Contains("cl")) f.FromSelect("companyLogo", _t(s.company_logo, nameof(s.company_logo))); f.Types["companyLogo"] = f.Checked["showCompanyLogo"] ? "text" : "disabled";
                    if (add || cf.Contains("scn")) f.Checked["showContractNumber"] = _t(s.show_contract_number, nameof(s.show_contract_number)) == "Y";
                    if (add || cf.Contains("cn")) f.Values["contractNumber"] = _t(s.contract_number, nameof(s.contract_number)); f.Types["contractNumber"] = f.Checked["showContractNumber"] ? "text" : "disabled";
                    if (add || cf.Contains("son")) f.Checked["showOrderNumber"] = _t(s.show_order_number, nameof(s.show_order_number)) == "Y";
                    f.Types["orderNumber"] = f.Checked["showOrderNumber"] ? "text" : "disabled";
                }
                else
                {
                    f.Types["showProjectOrgCode"] = "disabled";
                    f.Types["showProjectCode"] = "disabled";
                    f.Types["showProjectTitle"] = "disabled";
                    f.Types["showProjectFundedValue"] = "disabled";
                    f.Types["showCompanyLogo"] = "disabled";
                    f.Types["companyLogo"] = "disabled";
                    f.Types["showContractNumber"] = "disabled";
                    f.Types["contractNumber"] = "disabled";
                    f.Types["showOrderNumber"] = "disabled";
                    f.Types["orderNumber"] = "text";
                }
                //
                if (add || cf.Contains("on")) f.Values["orderNumber"] = _t(s.order_number, nameof(s.order_number));
                if (add || cf.Contains("d")) f.Values["description"] = _t(s.description, nameof(s.description));
                /*if (add || cf.Contains("im"))*/
                f.Values["memo"] = _t(s.invoice_memo, nameof(s.invoice_memo));
                f.Add("button_save", "action", null);
                return f.ToString();
            });
            return r != null ?
                ManageFlags.ProjectInvoiceSetupChanged :
                ManageFlags.None;
        }
    }
}