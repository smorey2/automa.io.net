using ExcelTrans.Services;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
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

        /// <summary>
        /// CustomTransactionReport
        /// </summary>
        /// <seealso cref="Automa.IO.ICustom" />
        public class CustomTransactionReport : ICustomWithTransfer
        {
            /// <summary>
            /// Param
            /// </summary>
            public class Param
            {
                /// <summary>
                /// The source folder
                /// </summary>
                public string SourceFolder { get; set; }
                /// <summary>
                /// The begin date
                /// </summary>
                public DateTime? BeginDate { get; set; }
                /// <summary>
                /// The end date
                /// </summary>
                public DateTime? EndDate { get; set; }
            }

            static void ElementSelect(IWebElement element, bool value) { if (element.Selected != value) element.Click(); }

            /// <summary>
            /// Executes the asynchronous.
            /// </summary>
            /// <param name="client">The client.</param>
            /// <param name="param">The parameter.</param>
            /// <param name="tag">The tag.</param>
            /// <returns></returns>
            public Task<object> ExecuteAsync(AutomaClient client, object param = null, object tag = null, CancellationToken? cancellationToken = null)
            {
                var p = param as Param;
                var sourceFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads");
                var filePath = Path.Combine(sourceFolder, "TransactionReport.xls");
                if (File.Exists(filePath))
                    File.Delete(filePath);
                var driver = client.GetWebDriver();
                driver.Navigate().GoToUrl($"{(client as UmbClient).UmbUri}/Reports/report2_1010c.asp");
                try
                {
                    driver.FindElement(By.Name("xs_bi")).SendKeys($"[All Account Issuers]{Keys.Enter}"); // Statement Issuer
                    driver.FindElement(By.Name("xs_pi")).SendKeys(Keys.Enter); // Statement Period
                    driver.FindElement(By.Name("xs_l_ct")).SendKeys($"[All Types]{Keys.Enter}"); // Account Type
                    if (p.BeginDate != null) driver.FindElement(By.Name("xs_start")).SendKeys(p.BeginDate.Value.ToShortDateString()); // Start Date
                    if (p.EndDate != null) driver.FindElement(By.Name("xs_end")).SendKeys(p.EndDate.Value.ToShortDateString()); // End Date
                    ElementSelect(driver.FindElement(By.Name("xs_umt")), true); // Include Unmapped Transactions
                    ElementSelect(driver.FindElement(By.Name("xs_m_f")), false); // Group Results
                    ElementSelect(driver.FindElement(By.Id("xs_m_s_4")), true); // Transaction List

                    // additional fields
                    var additionalFields = driver.FindElements(By.ClassName("accordion")).First(x => x.Text == "Additional Fields");
                    additionalFields.Click(); Thread.Sleep(500); // Additional Fields
                    ElementSelect(driver.FindElement(By.Id("i91")), true); Thread.Sleep(500); // Issuer Reference
                    driver.FindElement(By.ClassName("two")).Click(); Thread.Sleep(500); // page 2
                    ElementSelect(driver.FindElement(By.Id("i85")), true); Thread.Sleep(500); // Authorization Number

                    // submit
                    driver.FindElement(By.Name("xs_filename")).SendKeys("TransactionReport"); // Export File Name
                    driver.FindElement(By.Name("xs_filetype")).SendKeys($"Excel{Keys.Enter}"); // Export File Type
                    driver.FindElement(By.ClassName("search")).Click();
                }
                catch { return Task.FromResult<object>(false); }
                var i = 0; while (!File.Exists(filePath) && i++ < 10)
                    Thread.Sleep(500);
                return Task.FromResult<object>(File.Exists(filePath));
            }

            public async Task SendAsync((WebSocket socket, JsonSerializerOptions jsonOptions) ws, object param = null, CancellationToken? cancellationToken = null)
            {
                var sourceFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads");
                var filePath = Path.Combine(sourceFolder, "TransactionReport.xls");
                var fileExists = File.Exists(filePath);
                await ws.SendAsync(fileExists, cancellationToken).ConfigureAwait(false);
                if (fileExists)
                    await ws.SendAsync(File.ReadAllText(filePath), cancellationToken).ConfigureAwait(false);
            }

            public async Task ReceiveAsync((WebSocket socket, JsonSerializerOptions jsonOptions) ws, object param = null, CancellationToken? cancellationToken = null)
            {
                var sourceFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads");
                var filePath = Path.Combine(sourceFolder, "TransactionReport.xls");
                var fileExists = await ws.ReceiveAsync<bool>(cancellationToken).ConfigureAwait(false);
                if (fileExists)
                    File.WriteAllText(filePath, await ws.ReceiveAsync<string>(cancellationToken).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Exports the file asynchronous.
        /// </summary>
        /// <param name="umb">The umb.</param>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="beginDate">The begin date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public static async Task<bool> ExportFileAsync(UmbClient umb, string sourceFolder, DateTime? beginDate = null, DateTime? endDate = null)
        {
            sourceFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads");
            var filePath = Path.Combine(sourceFolder, "TransactionReport.xls");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return await umb.TryCustomAsync<bool>(typeof(CustomTransactionReport), new CustomTransactionReport.Param
            {
                SourceFolder = sourceFolder,
                BeginDate = beginDate,
                EndDate = endDate,
            }) && File.Exists(filePath);
        }

        /*
        [Obsolete]
        public static Task<bool> ExportFileAsync_(UmbClient umb, string sourceFolder, DateTime? beginDate = null, DateTime? endDate = null)
        {
            var filePath = Path.Combine(sourceFolder, "TransactionReport.xls");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => umb.RunReportAsync("Reports/report2_1010c.asp", f =>
            {
                f.Values["xs_d_st"] = "-1";
                f.Values["xs_d_s_f"] = "i11";
                f.Values["xs_d_s_d"] = "1";
                f.Add("xs_cu", "text", null);
                f.FromSelect("xs_bi", "[All Account Issuers]"); // Statement Issuer
                f.Values["xs_pi"] = "0"; // Statement Period
                f.Values["xs_l_ct"] = "0"; // Account Type
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
                if (beginDate != null) f.Values["xs_start"] = beginDate.Value.ToShortDateString(); // Start Date
                if (endDate != null) f.Values["xs_end"] = endDate.Value.ToShortDateString(); // End Date
                f.Checked["xs_umt"] = true; // Include Unmapped Transactions
                f.Checked["xs_m_f"] = false; // Group Results
                f.FromSelectByKey("xs_m_s", "0"); // Transaction List
                f.FromMultiCheckbox("xs_d_f", new[] { // additional fields
                    "i91", // Issuer Reference
                    "i85", // Authorization Number
                }, merge: HtmlFormPost.Merge.Include);
                // submit
                f.Add("xsl_outmode", "text", "20");
                f.Add("xsl_outname", "text", "TransactionReport.xls");
            }, sourceFolder));
        }
        */

        public static IEnumerable<TransactionReport> Read(UmbClient umb, string sourceFolder)
        {
            sourceFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads");
            var filePath = Path.Combine(sourceFolder, "TransactionReport.xls");
            if (!File.Exists(filePath))
                throw new InvalidOperationException("Export Report Error: File Missing");
            using (var s1 = File.OpenRead(filePath))
                return ExcelReader.ReadRawXml(s1, x => !string.IsNullOrEmpty(x[2]) && x[2] != "Account" && x[2] != "XXXX-XXXX-XXXX-0000" ? new TransactionReport
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