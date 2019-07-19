using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Umb.Reports
{
    public class TransactionReport : ReportBase
    {
        public string Id { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime? TranDate { get; set; }
        public string Account { get; set; }
        public string Authorization { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Supplier { get; set; }
        public decimal? Amount { get; set; }

        public static Task<bool> ExportFileAsync(UmbClient umb, string sourceFolder, DateTime? beginDate = null, DateTime? endDate = null) =>
            Task.Run(() => umb.RunReport("Reports/report2_1010c.asp", f =>
            {
                f.Values["xs_d_st"] = "-1";
                f.Values["xs_d_s_f"] = "i11";
                f.Values["xs_d_s_d"] = "1";
                f.Add("xs_cu", "text", null);
                f.FromSelect("xs_bi", "[All Account Issuers]"); // All Account Issuers
                f.Values["xs_pi"] = "0";
                f.Values["xs_l_ct"] = "0";
                f.Values["xs_dt"] = "0";
                f.Values["xs_tt"] = "0";
                f.Values["xs_ts"] = "0";
                f.Values["xs_ap"] = "0";
                f.Values["xs_apx"] = "0";
                f.Values["xs_mg"] = "0";
                f.Values["xs_asc"] = "0";
                f.Values["xs_abc"] = "0";
                f.Values["xs_ar"] = "0";
                f.Values["xs_eti"] = "0";
                f.Values["xs_eti_c"] = "0";
                f.Values["xs_ccm"] = "0";
                f.Values["xs_bxt"] = "0";
                f.Values["xs_bxs"] = "0";
                if (beginDate != null) f.Values["xs_start"] = beginDate.Value.ToShortDateString();
                if (endDate != null) f.Values["xs_end"] = endDate.Value.ToShortDateString();
                f.Checked["xs_umt"] = true; // Include Unmapped Transactions
                f.Checked["xs_m_f"] = false; // Group Results
                f.FromSelectByKey("xs_m_s", "0"); // Transaction List
                // additional fields
                f.FromMultiCheckbox("xs_d_f", new[] {
                    "i91", // Issuer Reference
                    "i85", // Authorization Number
                }, merge: HtmlFormPost.Merge.Include);
                // submit
                f.Add("xsl_outmode", "text", "20");
                f.Add("xsl_outname", "text", "TransactionReport.xls");
            }, sourceFolder));

        public static IEnumerable<TransactionReport> Read(UmbClient umb, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, "TransactionReport.xls");
            using (var s1 = File.OpenRead(filePath))
                return ExcelReader.ReadRawXml(s1, x => !string.IsNullOrEmpty(x[0]) && x[2] != "XXXX-XXXX-XXXX-0000" ? new TransactionReport
                {
                    Id = x[7],
                    PostingDate = x[0].ToDateTime(),
                    TranDate = x[1].ToDateTime(),
                    Account = x[2],
                    Authorization = x[3],
                    LastName = x[4],
                    FirstName = x[5],
                    Supplier = x[6],
                    Amount = x[8].ToDecimal(),
                } : null, 9, 1).Where(x => x != null).ToList();
        }

        public static string GetReadXml(UmbClient umb, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(umb, sourceFolder).Select(x => new XElement("p", new XAttribute("i", x.Id),
                XAttribute("pd", x.PostingDate), XAttribute("td", x.TranDate), XAttribute("a", x.Account), XAttribute("a2", x.Authorization), XAttribute("ln", x.LastName), XAttribute("fn", x.FirstName), XAttribute("s", x.Supplier), XAttribute("a3", x.Amount)
            ))).ToString();
            if (!string.IsNullOrEmpty(syncFileA))
            {
                var syncFile = string.Format(syncFileA, ".t.xml");
                if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                    Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
                File.WriteAllText(syncFile, xml);
            }
            return xml;
        }
    }
}