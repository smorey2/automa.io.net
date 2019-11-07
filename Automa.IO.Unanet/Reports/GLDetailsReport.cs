using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Reports
{
    public class GLDetailsReport : ReportBase
    {
        public string OrgCode { get; set; }
        public string OrgName { get; set; }
        public string AccountType { get; set; }
        public int AccountCode { get; set; }
        public string AccountDesc { get; set; }
        //
        public DateTime? PostedDate { get; set; }
        public string Source { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string FiscalPeriod { get; set; }
        public string Description { get; set; }
        //
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string PersonName { get; set; }
        public string Reference { get; set; }
        public string ProjectOrg { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectTitle { get; set; }
        //
        public decimal? Quantity { get; set; }
        public decimal? BeginningBalance { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal? EndingBalance { get; set; }
        //
        public string AccountTypeOrderBy { get; set; }
        public string ArrangeBy { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string accountType, string sourceFolder, DateTime? beginDate = null, DateTime? endDate = null, string legalEntity = "75-00-DEG-00 - Digital Evolution Group, LLC", string wipAcct = "1420")
        {
            string account = null;
            if (accountType == "Wip")
            {
                var options = una.GetOptions("AccountMenu", "account", wipAcct);
                if (options.Count != 1)
                    throw new InvalidOperationException($"Can not find: {wipAcct}");
                account = options.First().Key;
            }
            var filePath = Path.Combine(sourceFolder, "report.csv");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.RunReport("financials/detail/general_ledger", f =>
            {
                f.FromSelect("legalEntity", legalEntity);
                if (accountType == "Wip") f.Values["account"] = account;
                else f.FromSelect("accountType", accountType); // "Revenue"
                string fpBegin = (beginDate ?? BeginFinanceDate).ToString("yyyy-MM"), fpEnd = (endDate ?? DateTime.Today).ToString("yyyy-MM");
                f.Values["fpRange_fpBegin"] = f.Selects["fpRange_fpBegin"].Single(x => x.Value == fpBegin).Key;
                f.Values["fpRange_fpEnd"] = f.Selects["fpRange_fpEnd"].Single(x => x.Value == fpEnd).Key;
                f.FromSelect("fpRange", "custom");
                f.FromSelect("orgHierarchy", "Organization");
                return f.ToString();
            }, sourceFolder) != null);
        }

        public static IEnumerable<GLDetailsReport> Read(UnanetClient una, string accountType, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, "report.csv");
            using (var s1 = File.OpenRead(filePath))
                return CsvReader.Read(s1, x => new GLDetailsReport
                {
                    OrgCode = x[0],
                    OrgName = x[1],
                    AccountType = x[2],
                    AccountCode = x[3].ToInt().Value,
                    AccountDesc = x[4],
                    //
                    PostedDate = x[5].ToDateTime(),
                    Source = x[6],
                    DocumentType = x[7],
                    DocumentNumber = x[8],
                    DocumentDate = x[9].ToDateTime(),
                    FiscalPeriod = x[10],
                    Description = x[11],
                    //
                    CustomerCode = x[12],
                    CustomerName = x[13],
                    PersonName = x[14],
                    Reference = x[15],
                    ProjectOrg = x[16].ToProjectToOrg(),
                    ProjectCode = x[17],
                    ProjectTitle = x[18],
                    //
                    Quantity = x[19].ToDecimal(),
                    BeginningBalance = x[20].ToDecimal(),
                    Debit = x[21].ToDecimal(),
                    Credit = x[22].ToDecimal(),
                    EndingBalance = x[23].ToDecimal(),
                    //
                    AccountTypeOrderBy = x[24],
                    ArrangeBy = x[25],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string accountType, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, accountType, sourceFolder).Select(x => new XElement("p",
                XAttribute("oc", x.OrgCode), XAttribute("on", x.OrgName), XAttribute("at", x.AccountType), new XAttribute("ac", x.AccountCode), XAttribute("ad", x.AccountDesc),
                XAttribute("pd", x.PostedDate), XAttribute("s", x.Source), XAttribute("dt", x.DocumentType), XAttribute("dn", x.DocumentNumber), XAttribute("dd", x.DocumentDate), XAttribute("fp", x.FiscalPeriod), XAttribute("d", x.Description),
                XAttribute("cc", x.CustomerCode), XAttribute("cn", x.CustomerName), XAttribute("pn", x.PersonName), XAttribute("r", x.Reference), XAttribute("po", x.ProjectOrg), XAttribute("pc", x.ProjectCode), XAttribute("pt", x.ProjectTitle),
                XAttribute("q", x.Quantity), XAttribute("bb", x.BeginningBalance), XAttribute("d2", x.Debit), XAttribute("c", x.Credit), XAttribute("eb", x.EndingBalance),
                XAttribute("atob", x.AccountTypeOrderBy), XAttribute("ab", x.ArrangeBy)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, $".r_g{accountType[0]}.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }
    }
}