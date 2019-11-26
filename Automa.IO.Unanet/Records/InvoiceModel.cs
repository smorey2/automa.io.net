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
        public string customer { get; set; }
        public DateTime trans_date { get; set; }
        public string ref_number { get; set; }
        public string terms { get; set; }
        public string bill_to { get; set; }
        public DateTime due_date { get; set; }
        public string memo { get; set; }
        public decimal? quantity { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, int window, string legalEntity = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.invoice.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.invoice.key, f =>
            {
                GetWindowDates(nameof(InvoiceModel), window, out var beginDate, out var endDate);
                f.Values["filename"] = una.Settings.invoice.file;
                f.Checked["suppressOutput"] = true;
                f.FromSelect("legalEntity", legalEntity ?? una.Settings.LegalEntity);
                f.Values["invoiceDate_bDate"] = beginDate.FromDateTime("BOT"); f.Values["invoiceDate_eDate"] = endDate.FromDateTime("EOT");
                f.FromSelectByKey("invoiceDate", "custom");
                f.Values["prange_bDate"] = "BOT"; f.Values["prange_eDate"] = "EOT";
                f.FromSelectByKey("prange", "bot_eot");
            }, sourceFolder));
        }

        public static IEnumerable<InvoiceModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.invoice.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new InvoiceModel
                {
                    customer = x[0],
                    trans_date = x[1].ToDateTime().Value,
                    ref_number = x[2],
                    terms = x[5],
                    bill_to = x[8],
                    due_date = x[10].ToDateTime().Value,
                    //memo = x[12],
                    quantity = x[14].ToDecimal(),
                }, 0).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("c", x.customer), new XAttribute("td", x.trans_date), XAttribute("rn", x.ref_number), XAttribute("t", x.terms), XAttribute("bt", x.bill_to), new XAttribute("dd", x.due_date), XAttribute("m", x.memo), XAttribute("q", x.quantity)
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