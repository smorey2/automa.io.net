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
        public string external_system_code { get; set; }
        public string description { get; set; }
        public DateTime? bill_date { get; set; }
        public string bill_on_completion { get; set; }
        public string bill_amount { get; set; }
        public string revenue_recognition_method { get; set; }
        public string delete { get; set; }
        //
        public bool posted { get; set; }
        public bool locked { get; set; }
        public string project_codeKey { get; set; }
        //
        public string key { get; set; }
        public DateTime? revenue_recognition_date { get; set; }
        public decimal? revenue_recognition_amount { get; set; }

        public static Task<(bool success, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.fixed_price_item.file);
            var filePath2 = Path.Combine(sourceFolder, una.Settings.fixed_price_item_post.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (File.Exists(filePath2))
                File.Delete(filePath2);
            return Task.Run(() => (
                una.GetEntitiesByExport(una.Settings.fixed_price_item.key, (z, f) =>
                {
                    f.Checked["suppressOutput"] = true;
                    f.Values["dateRange_bDate"] = "BOT"; f.Values["dateRange_eDate"] = "EOT";
                    f.Values["dateRange"] = "bot_eot";
                    f.Values["wbsLevel"] = "both";
                    f.Checked["includePosted"] = true;
                    f.Checked["includeRevSchedules"] = true;
                    return null;
                }, sourceFolder).success &&
                una.GetEntitiesByExport(una.Settings.fixed_price_item_post.key, (z, f) =>
                {
                    f.Checked["suppressOutput"] = true;
                    f.Values["dateRange_bDate"] = "BOT"; f.Values["dateRange_eDate"] = "EOT";
                    f.Values["dateRange"] = "bot_eot";
                    f.Values["wbsLevel"] = "both";
                    f.Checked["includePosted"] = false;
                    f.Checked["includeRevSchedules"] = true;
                    return null;
                }, sourceFolder).success, true, (object)null));
        }

        public static IEnumerable<FixedPriceModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.fixed_price_item.file);
            var filePath2 = Path.Combine(sourceFolder, una.Settings.fixed_price_item_post.file);
            using (var sr2 = File.OpenRead(filePath2))
            {
                var post = new HashSet<string>(CsvReader.Read(sr2, x => x[9], 1).ToList());
                using (var sr = File.OpenRead(filePath))
                    return CsvReader.Read(sr, x =>
                    {
                        var d = x[3].ToString();
                        //var d0 = d.IndexOf(","); //: d1 = d.IndexOf(":");
                        //var external_system_code = d0 != -1 && d.Length - d0 == 3 ? d.Substring(d0 + 1) : null; //: d0 != -1 && d0 < d1 ? d.Substring(0, d0) : null;
                        //var description = d0 != -1 && d.Length - d0 == 3 ? d.Substring(0, d0) : d; //: d0 != -1 && d0 < d1 ? d.Substring(d0 + 1) : d;
                        var external_system_code = (string)null;
                        var description = d;
                        return new FixedPriceModel
                        {
                            project_org_code = x[0],
                            project_code = x[1],
                            task_name = x[2].DecodeString(),
                            external_system_code = external_system_code,
                            description = description,
                            bill_date = x[4].ToDateTime(),
                            bill_on_completion = x[5],
                            bill_amount = x[6],
                            revenue_recognition_method = x[7],
                            delete = x[8],
                            posted = !post.Contains(x[9]),
                            //
                            key = x[9],
                            revenue_recognition_date = x[10].ToDateTime(),
                            revenue_recognition_amount = x[11].ToDecimal(),
                        };
                    }, 1).ToList();
            }
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code), XAttribute("tn", x.task_name), XAttribute("esc", x.external_system_code), XAttribute("d", x.description),
                XAttribute("bd", x.bill_date), XAttribute("boc", x.bill_on_completion), XAttribute("ba", x.bill_amount), XAttribute("rrm", x.revenue_recognition_method),
                XAttribute("rrd", x.revenue_recognition_date), XAttribute("rra", x.revenue_recognition_amount),
                new XAttribute("p", x.posted ? 1 : 0)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, $".j_fp.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_FixedPrice1 : FixedPriceModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_FixedPrice1 s, out Dictionary<string, (Type, object)> fields, out string last, Action<string> lockFunc, Action<p_FixedPrice1> bespoke = null)
        {
            throw new NotSupportedException("FixedPrice Not Supported");
            var _f = fields = new Dictionary<string, (Type, object)>();
            T _t<T>(T value, string name) { _f[name] = (typeof(T), value); return value; }
            //
            bespoke?.Invoke(s);
            if (s.revenue_recognition_method == null)
                throw new InvalidOperationException($"{s.project_code} not categorized");
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out last))
                return ManageFlags.FixedPriceChanged;
            //throw new NotSupportedException();
            var method = add ? HttpMethod.Post : HttpMethod.Put; // !cf.Contains("tainted") ? add ? HttpMethod.Post : HttpMethod.Put : HttpMethod.Delete;
            var r = una.SubmitSubManage("A", method, "projects/accounting/fixed_price_item", $"key={s.key}",
                $"projectkey={s.project_codeKey}", null,
                out last, (z, f) =>
            {
                if (f.Types["description"] == "disabled")
                {
                    lockFunc(s.key);
                    return null;
                }
                //if (add || cf.Contains("poc")) f.Values["xxxx"] = _t(s.project_org_code, nameof(s.project_org_code));
                //if (add || cf.Contains("pc")) f.Values["xxxx"] = _t(s.project_code, nameof(s.project_code));
                if (add || cf.Contains("tn")) f.FromSelectByPredicate("task", _t(s.task_name, nameof(s.task_name)), x => x.Value.StartsWith(s.task_name));
                var description = _t(s.description, nameof(s.description));
                var description2 = !string.IsNullOrEmpty(_t(s.external_system_code, nameof(s.external_system_code))) ? $"{description},{s.external_system_code}" : description;
                if (add || cf.Contains("esc") || cf.Contains("d") || cf.Contains("bind")) f.Values["description"] = description;
                //
                if (add || cf.Contains("bd")) f.Values["billDate"] = _t(s.bill_date, nameof(s.bill_date)).FromDateTime();
                if (add || cf.Contains("boc")) f.Checked["useWbsEndDate"] = _t(s.bill_on_completion, nameof(s.bill_on_completion)) == "Y";
                if (add || cf.Contains("ba")) f.Values["amount"] = _t(s.bill_amount, nameof(s.bill_amount));
                var recMethod = s.revenue_recognition_method == "WHEN_BILLED" ? "1"
                    : s.revenue_recognition_method == "PERCENT_COMPLETE" ? "2"
                    : s.revenue_recognition_method == "CUSTOM_SCHEDULE" ? "3"
                    : throw new ArgumentOutOfRangeException(nameof(s.revenue_recognition_method), s.revenue_recognition_method);
                if (add || cf.Contains("rrm")) f.FromSelectByKey("revRecMethod", recMethod);
                f.Add("button_save", "action", null);
                return f.ToString();
            });
            return r != null ?
                ManageFlags.FixedPriceChanged :
                ManageFlags.None;
        }
    }
}