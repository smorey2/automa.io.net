using ExcelTrans.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Unanet.Exports
{
    public class LaborCategoryModel : ModelBase
    {
        public string key { get; set; }
        //
        public string labor_category { get; set; }
        public string delete { get; set; }
        public string description { get; set; }
        public string bill_rate { get; set; }
        public string cost_rate { get; set; }
        public string external_system_code { get; set; }
        public string active { get; set; }
        public string effective_date { get; set; }

        public static Task<bool> ExportFile(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.labor_category_master.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.labor_category_master.key, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("dateRange", "Today");
            }, sourceFolder));
        }

        public static IEnumerable<LaborCategoryModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.labor_category_master.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new LaborCategoryModel
                {
                    key = x[0],
                    //
                    labor_category = x[1],
                    delete = x[2],
                    description = x[3],
                    bill_rate = x[4],
                    cost_rate = x[5],
                    external_system_code = x[6],
                    active = x[7],
                    effective_date = x[8],
                }, 1).ToList();
        }

        public static IEnumerable<LaborCategoryModel> EnsureAndRead(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.labor_category_master.file);
            if (!File.Exists(filePath))
                ExportFile(una, sourceFolder).Wait();
            return Read(una, sourceFolder);
        }
    }
}
