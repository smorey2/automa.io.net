using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    // MASTER
    public class LaborCategoryModel : ModelBase
    {
        public string key { get; set; }
        //
        public string labor_category { get; set; }
        public string delete { get; set; }
        public string description { get; set; }
        public decimal? bill_rate { get; set; }
        public decimal? cost_rate { get; set; }
        public string external_system_code { get; set; }
        public string active { get; set; }
        public DateTime effective_date { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.labor_category_master.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.labor_category_master.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("dateRange", "Today");
                return null;
            }, sourceFolder));
        }

        public static IEnumerable<LaborCategoryModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.labor_category_master.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new LaborCategoryModel
                {
                    key = x[0],
                    //
                    labor_category = x[1],
                    delete = x[2],
                    description = x[3],
                    bill_rate = x[4].ToDecimal(),
                    cost_rate = x[5].ToDecimal(),
                    external_system_code = x[6],
                    active = x[7],
                    effective_date = x[8].ToDateTime().Value,
                }, 1).ToList();
        }

        public static Task<IEnumerable<LaborCategoryModel>> EnsureAndReadAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.labor_category_master.file);
            if (!File.Exists(filePath))
                ExportFileAsync(una, sourceFolder).Wait();
            return Task.FromResult(Read(una, sourceFolder));
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("lc", x.labor_category), XAttribute("d", x.description), XAttribute("br", x.bill_rate), XAttribute("cr", x.cost_rate), 
                XAttribute("esc", x.external_system_code), XAttribute("a", x.active), new XAttribute("ed", x.effective_date)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".lc.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }
    }
}
