using ExcelTrans.Services;
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

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = "75-00-DEG-00 - Digital Evolution Group, LLC")
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["project invoice setup"].Item2}.csv");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Exports["project invoice setup"].Item1, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity);
            }, sourceFolder));
        }

        public static IEnumerable<ProjectInvoiceSetupModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["project invoice setup"].Item2}.csv");
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

        public static ManageFlags ManageRecord(UnanetClient una, p_ProjectInvoiceSetup1 s, out string last)
        {
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.ProjectInvoiceSetupChanged;
            var r = una.SubmitSubManage("D", HttpMethod.Post, $"projects/controllers", null,
                $"projectkey={s.project_codeKey}", null,
                out last, (z, f) =>
            {
                return f.ToString();
            });
            return r != null ?
                ManageFlags.ProjectInvoiceSetupChanged :
                ManageFlags.None;
        }
    }
}