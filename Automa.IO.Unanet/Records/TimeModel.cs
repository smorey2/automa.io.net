using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class TimeModel : ModelBase
    {
        public string username { get; set; }
        public DateTime work_date { get; set; }
        public string project_org_code { get; set; }
        public string project_code { get; set; }
        public string task_name { get; set; }
        public string project_type { get; set; }
        public string pay_code { get; set; }
        //
        public decimal? hours { get; set; }
        public decimal? bill_rate { get; set; }
        public decimal? cost_rate { get; set; }
        public string project_org_override { get; set; }
        public string person_org_override { get; set; }
        public string labor_category { get; set; }
        public string location { get; set; }
        public string comments { get; set; }
        //
        public string change_reason { get; set; }
        public string cost_structure { get; set; }
        public string cost_element { get; set; }
        public string time_period_begin_date { get; set; }
        public string post_date { get; set; }
        public decimal? additional_pay_rate { get; set; }
        //
        public string key { get; set; }
        public string keySheet { get; set; }
        public string enumSheet { get; set; }
        public decimal? labor_category_bill_rate { get; set; }
        public string keyInvoice { get; set; }
        public string invoice_number { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string windowEntity, string sourceFolder, int window, DateTime? begin = null, DateTime? end = null, string legalEntity = null, Action<HtmlFormPost> func = null, string tempPath = null)
        {
            var filePath = tempPath ?? Path.Combine(sourceFolder, una.Settings.time.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.time.key, (z, f) =>
            {
                GetWindowDates(windowEntity ?? nameof(TimeModel), window, out var beginDate, out var endDate);
                f.Checked["suppressOutput"] = true;
                f.Values["dateType"] = "range";
                f.Values["beginDate"] = begin != null ? begin.FromDateTime("BOT") : beginDate.FromDateTime("BOT"); f.Values["endDate"] = end != null ? end.FromDateTime("EOT") : endDate.FromDateTime("EOT");
                f.FromSelect("legalEntity", legalEntity ?? una.Settings.LegalEntity);
                f.Checked["exempt"] = true; f.Checked["nonExempt"] = true; f.Checked["nonEmployee"] = true; f.Checked["subcontractor"] = true;
                f.Checked["INUSE"] = true; f.Checked["SUBMITTED"] = true; f.Checked["APPROVING"] = true;
                f.Checked["DISAPPROVED"] = true; f.Checked["COMPLETED"] = true; f.Checked["LOCKED"] = true;
                f.Checked["EXTRACTED"] = true;
                f.Checked["inclReg"] = true;
                f.FromSelectByKey("adjustmentStatus", "ENTERED");
                f.Checked["incPrevExt"] = true;
                f.Checked["suppIntAdj"] = true;
                func?.Invoke(f);
                return (begin ?? beginDate, end ?? endDate);
            }, sourceFolder, interceptFilename: x => filePath));
        }

        public static IEnumerable<TimeModel> Read(UnanetClient una, string sourceFolder, string tempPath = null)
        {
            var filePath = tempPath ?? Path.Combine(sourceFolder, una.Settings.time.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new TimeModel
                {
                    username = x[0],
                    work_date = x[1].ToDateTime().Value,
                    project_org_code = x[2],
                    project_code = x[3],
                    task_name = x[4].DecodeString(),
                    project_type = x[5],
                    pay_code = x[6],
                    //
                    hours = x[7].ToDecimal(),
                    bill_rate = x[8].ToDecimal(),
                    cost_rate = x[9].ToDecimal(),
                    project_org_override = x[10],
                    person_org_override = x[11],
                    labor_category = x[12],
                    location = x[13],
                    comments = x[14],
                    //
                    change_reason = x[15],
                    cost_structure = x[16],
                    cost_element = x[17],
                    time_period_begin_date = x[18],
                    post_date = x[19],
                    additional_pay_rate = x[20].ToDecimal(),
                    //
                    key = x[21],
                    keySheet = x[22],
                    enumSheet = x[23],
                    keyInvoice = x[24],
                    labor_category_bill_rate = x[25].ToDecimal(),
                    invoice_number = x[26],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null, string tempPath = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder, tempPath).Select(x => new XElement("p", XAttribute("k", x.key), XAttribute("k2", x.keySheet), XAttribute("k3", x.keyInvoice),
                XAttribute("u", x.username), new XAttribute("wd", x.work_date), XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("tn", x.task_name), XAttribute("pt", x.project_type), XAttribute("pc2", x.pay_code),
                XAttribute("h", x.hours), XAttribute("br", x.bill_rate), XAttribute("cr", x.cost_rate), XAttribute("poo", x.project_org_override), XAttribute("poo2", x.person_org_override), XAttribute("lc", x.labor_category), XAttribute("l", x.location), XAttribute("c", x.comments),
                XAttribute("cr2", x.change_reason), XAttribute("cs", x.cost_structure), XAttribute("ce", x.cost_element), XAttribute("tpbd", x.time_period_begin_date), XAttribute("pd", x.post_date), XAttribute("apr", x.additional_pay_rate),
                XAttribute("es", x.enumSheet), XAttribute("lcbr", x.labor_category_bill_rate), XAttribute("in", x.invoice_number)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".t.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }
    }
}