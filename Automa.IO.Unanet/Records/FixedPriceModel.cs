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
    public class FixedPriceModel : ModelBase
    {
        public string project_org_code { get; set; }
        public string project_code { get; set; }
        public string task_name { get; set; }
        public string description { get; set; }
        public DateTime? bill_date { get; set; }
        public string bill_on_completion { get; set; }
        public string bill_amount { get; set; }
        public string revenue_recognition_method { get; set; }
        public string delete { get; set; }
        //
        public string key { get; set; }
        public DateTime? revenue_recognition_date { get; set; }
        public decimal? revenue_recognition_amount { get; set; }
        //
        public bool posted { get; set; }
        public string project_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["fixed price item"].Item2}.csv");
            var filePath2 = Path.Combine(sourceFolder, $"{una.Exports["fixed price item [post]"].Item2}.csv");
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (File.Exists(filePath2))
                File.Delete(filePath2);
            return Task.Run(() =>
                una.GetEntitiesByExport(una.Exports["fixed price item"].Item1, f =>
                {
                    f.Checked["suppressOutput"] = true;
                    f.Values["dateRange_bDate"] = "BOT"; f.Values["dateRange_eDate"] = "EOT";
                    f.Values["dateRange"] = "bot_eot";
                    f.Values["wbsLevel"] = "both";
                    f.Checked["includePosted"] = true;
                    f.Checked["includeRevSchedules"] = true;
                }, sourceFolder) &&
                una.GetEntitiesByExport(una.Exports["fixed price item [post]"].Item1, f =>
                {
                    f.Checked["suppressOutput"] = true;
                    f.Values["dateRange_bDate"] = "BOT"; f.Values["dateRange_eDate"] = "EOT";
                    f.Values["dateRange"] = "bot_eot";
                    f.Values["wbsLevel"] = "both";
                    f.Checked["includePosted"] = false;
                    f.Checked["includeRevSchedules"] = false;
                }, sourceFolder));
        }

        public static IEnumerable<FixedPriceModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["fixed price item"].Item2}.csv");
            var filePath2 = Path.Combine(sourceFolder, $"{una.Exports["fixed price item [post]"].Item2}.csv");
            using (var sr1 = File.OpenRead(filePath2))
            {
                var post = new HashSet<string>(CsvReader.Read(sr1, x => x.Count > 9 ? x[9] : null, 1).ToList());
                using (var sr = File.OpenRead(filePath))
                    return CsvReader.Read(sr, x => new FixedPriceModel
                    {
                        project_org_code = x[0],
                        project_code = x[1],
                        task_name = x[2],
                        description = x[3].ToString(),
                        bill_date = x[4].ToDateTime(),
                        bill_on_completion = x[5],
                        bill_amount = x[6],
                        revenue_recognition_method = x[7],
                        delete = x[8],
                        //
                        key = x.Count > 9 ? x[9] : null,
                        revenue_recognition_date = x.Count > 10 ? x[10].ToDateTime() : null,
                        revenue_recognition_amount = x.Count > 11 ? x[11].ToDecimal() : null,
                        //
                        posted = x.Count > 9 && !post.Contains(x[9]),
                    }, 1).ToList();
            }
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("tn", x.task_name), XAttribute("d", x.description),
                XAttribute("bd", x.bill_date), XAttribute("boc", x.bill_on_completion), XAttribute("ba", x.bill_amount), XAttribute("rrm", x.revenue_recognition_method),
                XAttribute("rrd", x.revenue_recognition_date), XAttribute("rra", x.revenue_recognition_amount),
                new XAttribute("p", x.posted ? 1 : 0)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, $".j_fp.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_FixedPrice1 : FixedPriceModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_FixedPrice1 s, out string last)
        {
            if (s.revenue_recognition_method == null)
                throw new InvalidOperationException($"{s.project_code} not categorized");
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out last))
                return ManageFlags.FixedPriceChanged;
            var method = !cf.Contains("tainted") ? add ? HttpMethod.Post : HttpMethod.Put : HttpMethod.Delete;
            var r = una.SubmitSubManage("A", method, "projects/accounting/fixed_price_item",
                $"key={s.key}", $"projectkey={s.project_codeKey}", null,
                out last, (z, f) =>
            {
                //if (add || cf.Contains("poc")) f.Values["xxxx"] = s.project_org_code;
                //if (add || cf.Contains("pc")) f.Values["xxxx"] = s.project_code;
                if (add || cf.Contains("tn")) f.FromSelectByPredicate("task", s.task_name, x => x.Value.StartsWith(s.task_name));
                if (add || cf.Contains("d")) f.Values["description"] = s.description;
                //
                if (add || cf.Contains("bd")) f.Values["billDate"] = s.bill_date.FromDateTime();
                if (add || cf.Contains("boc")) f.Checked["useWbsEndDate"] = s.bill_on_completion == "Y";
                if (add || cf.Contains("ba")) f.Values["amount"] = s.bill_amount;
                var recMethod = s.revenue_recognition_method == "WHEN_BILLED" ? "1"
                    : s.revenue_recognition_method == "PERCENT_COMPLETE" ? "2"
                    : s.revenue_recognition_method == "CUSTOM_SCHEDULE" ? "3"
                    : throw new ArgumentOutOfRangeException(nameof(s.revenue_recognition_method), s.revenue_recognition_method);
                if (add || cf.Contains("rrm")) f.FromSelectByKey("revRecMethod", recMethod);
                //
                f.Add("button_save", "action", null);
            });
            return r != null ?
                ManageFlags.FixedPriceChanged :
                ManageFlags.None;
        }
    }
}