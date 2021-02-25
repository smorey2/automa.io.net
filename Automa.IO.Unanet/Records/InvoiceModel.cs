using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class InvoiceModel : ModelBase
    {
        public string account_code { get; set; }
        public string account_type { get; set; }
        public string project_org_code { get; set; }
        public string project_org_name { get; set; }
        public string project_code { get; set; }
        public string org_code { get; set; }
        public DateTime trans_date { get; set; }
        public string ref_number { get; set; }
        public string order_number { get; set; }
        public string description { get; set; }
        public string terms { get; set; }
        public string bill_to { get; set; }
        public string bill_to_contact { get; set; }
        public string remit_to { get; set; }
        public string remit_to_contact { get; set; }
        public string ship_to { get; set; }
        public string ship_to_contact { get; set; }
        public DateTime due_date { get; set; }
        public string memo { get; set; }
        public decimal? amount { get; set; }
        public string created_by { get; set; }
        public DateTime created_date { get; set; }
        public string completed_by { get; set; }
        public DateTime? completed_date { get; set; }
        public string voided_invoice_num { get; set; }
        public string voiding_invoice_num { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder, int window, string legalEntity = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.invoice.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.invoice.key, (z, f) =>
            {
                GetWindowDates(nameof(InvoiceModel), window, out var beginDate, out var endDate);
                f.Values["filename"] = una.Options.invoice.file;
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity ?? una.Options.LegalEntity);
                f.Values["invoiceDate_bDate"] = beginDate.FromDateTime("BOT"); f.Values["invoiceDate_eDate"] = endDate.FromDateTime("EOT");
                f.FromSelectByKey("invoiceDate", "custom");
                f.Values["prange_bDate"] = "BOT"; f.Values["prange_eDate"] = "EOT";
                f.FromSelectByKey("prange", "bot_eot");
                return null;
            }, sourceFolder));
        }

        public static IEnumerable<InvoiceModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.invoice.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new InvoiceModel
                {
                    account_code = x[0],
                    account_type = x[1],
                    project_org_code = x[2],
                    project_org_name = x[3],
                    project_code = x[4],
                    org_code = x[5],
                    trans_date = x[6].ToDateTime().Value,
                    ref_number = x[7],
                    order_number = x[8],
                    description = x[9],
                    terms = x[10],
                    bill_to = x[11],
                    bill_to_contact = x[12],
                    remit_to = x[13],
                    remit_to_contact = x[14],
                    ship_to = x[15],
                    ship_to_contact = x[16],
                    due_date = x[17].ToDateTime().Value,
                    memo = x[18],
                    amount = x[19].ToDecimal(),
                    created_by = x[20],
                    created_date = x[21].ToDateTime().Value,
                    completed_by = x[22],
                    completed_date = x[23].ToDateTime(),
                    voided_invoice_num = x[24],
                    voiding_invoice_num = x[25],
                }, 0).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("ac", x.account_code), XAttribute("at", x.account_type), XAttribute("poc", x.project_org_code), XAttribute("pon", x.project_org_name), XAttribute("pc", x.project_code), XAttribute("oc", x.org_code),
                new XAttribute("td", x.trans_date), XAttribute("rn", x.ref_number), XAttribute("on", x.order_number), XAttribute("d", x.description), XAttribute("t", x.terms),
                XAttribute("bt", x.bill_to), XAttribute("btc", x.bill_to_contact), XAttribute("rt", x.remit_to), XAttribute("rtc", x.remit_to_contact), XAttribute("st", x.ship_to), XAttribute("stc", x.ship_to_contact),
                new XAttribute("dd", x.due_date), XAttribute("m", x.memo), XAttribute("a", x.amount), XAttribute("cb", x.created_by), new XAttribute("cd", x.created_date), XAttribute("cb2", x.completed_by), XAttribute("cd2", x.completed_date),
                XAttribute("vin", x.voided_invoice_num), XAttribute("vin2", x.voiding_invoice_num)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".i.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }
    }
}