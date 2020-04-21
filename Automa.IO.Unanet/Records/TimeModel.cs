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
        public string key { get; set; }
        public string keySheet { get; set; }
        //
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
        public string additional_pay_rate { get; set; }
        //
        public string keyInvoice { get; set; }
        public string invoice_number { get; set; }

        public static Task<(bool success, bool hasFile)> ExportFileAsync(UnanetClient una, string windowEntity, string sourceFolder, int window, DateTime? cutoff = null, string legalEntity = null, Action<HtmlFormPost> func = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.time.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.time.key, f =>
            {
                GetWindowDates(windowEntity ?? nameof(TimeModel), window, out var beginDate, out var endDate);
                f.Checked["suppressOutput"] = true;
                f.Values["dateType"] = "range";
                f.Values["beginDate"] = beginDate.FromDateTime("BOT"); f.Values["endDate"] = cutoff != null ? cutoff.FromDateTime("EOT") : endDate.FromDateTime("EOT");
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
            }, sourceFolder));
        }

        public static IEnumerable<TimeModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.time.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new TimeModel
                {
                    key = x[0],
                    keySheet = x[1],
                    //
                    username = x[2],
                    work_date = x[3].ToDateTime().Value,
                    project_org_code = x[4],
                    project_code = x[5],
                    task_name = x[6].DecodeString(),
                    project_type = x[7],
                    pay_code = x[8],
                    //
                    hours = x[9].ToDecimal(),
                    bill_rate = x[10].ToDecimal(),
                    cost_rate = x[11].ToDecimal(),
                    project_org_override = x[12],
                    person_org_override = x[13],
                    labor_category = x[14],
                    location = x[15],
                    comments = x[16],
                    //
                    change_reason = x[17],
                    cost_structure = x[18],
                    cost_element = x[19],
                    time_period_begin_date = x[20],
                    post_date = x[21],
                    additional_pay_rate = x[22],
                    //
                    keyInvoice = x[23],
                    invoice_number = x[24],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key), XAttribute("k2", x.keySheet), XAttribute("k3", x.keyInvoice),
                XAttribute("u", x.username), new XAttribute("wd", x.work_date), XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("tn", x.task_name), XAttribute("pt", x.project_type), XAttribute("pc2", x.pay_code),
                XAttribute("h", x.hours), XAttribute("br", x.bill_rate), XAttribute("cr", x.cost_rate), XAttribute("poo", x.project_org_override), XAttribute("poo2", x.person_org_override), XAttribute("lc", x.labor_category), XAttribute("l", x.location), XAttribute("c", x.comments),
                XAttribute("cr2", x.change_reason), XAttribute("cs", x.cost_structure), XAttribute("ce", x.cost_element), XAttribute("tpbd", x.time_period_begin_date), XAttribute("pd", x.post_date), XAttribute("apr", x.additional_pay_rate),
                XAttribute("in", x.invoice_number)
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