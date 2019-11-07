using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Reports
{
    public class ARAgingReport : ReportBase
    {
        public string CustomerOrgCode { get; set; }
        public string CustomerOrgName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectTitle { get; set; }
        public string Type { get; set; }
        //
        public string DocNo { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Period { get; set; }
        public string Description { get; set; }
        //
        public decimal? OriginalAmount { get; set; }
        public decimal? Balance { get; set; }
        public decimal? Current { get; set; }
        public decimal? PastDue1 { get; set; } // PastDue(1-30)
        public decimal? PastDue31 { get; set; } // PastDue(31-60)
        public decimal? PastDue61 { get; set; } // PastDue(61-90)
        public decimal? PastDue90 { get; set; } // PastDue(Over90)

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, DateTime? beginDate = null, DateTime? endDate = null, string legalEntity = "75-00-DEG-00 - Digital Evolution Group, LLC")
        {
            var filePath = Path.Combine(sourceFolder, "report.csv");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.RunReport("financials/detail/accounts_receivable", f =>
            {
                f.FromSelect("legalEntity", legalEntity);
                f.FromSelect("arrangeBy", "Customer");
                return f.ToString();
            }, sourceFolder) != null);
        }

        public static IEnumerable<ARAgingReport> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, "report.csv");
            using (var s1 = File.OpenRead(filePath))
                return CsvReader.Read(s1, x => new ARAgingReport
                {
                    CustomerOrgCode = x[0],
                    CustomerOrgName = x[1],
                    ProjectCode = x[2],
                    ProjectTitle = x[3],
                    Type = x[4],
                    //
                    DocNo = x[5],
                    DocDate = x[6].ToDateTime(),
                    DueDate = x[7].ToDateTime(),
                    Period = x[8],
                    Description = x[9],
                    //
                    OriginalAmount = x[10].ToDecimal(),
                    Balance = x[11].ToDecimal(),
                    Current = x[12].ToDecimal(),
                    PastDue1 = x[13].ToDecimal(),
                    PastDue31 = x[14].ToDecimal(),
                    PastDue61 = x[15].ToDecimal(),
                    PastDue90 = x[16].ToDecimal(),
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("coc", x.CustomerOrgCode), XAttribute("con", x.CustomerOrgName), XAttribute("pc", x.ProjectCode), XAttribute("pt", x.ProjectTitle), XAttribute("t", x.Type),
                XAttribute("dn", x.DocNo), XAttribute("dd", x.DocDate), XAttribute("dd2", x.DueDate), XAttribute("p", x.Period), XAttribute("d", x.Description),
                XAttribute("oa", x.OriginalAmount), XAttribute("b", x.Balance), XAttribute("c", x.Current), XAttribute("pd1", x.PastDue1), XAttribute("pd31", x.PastDue31), XAttribute("pd61", x.PastDue61), XAttribute("pd90", x.PastDue90)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".r_a.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }
    }
}