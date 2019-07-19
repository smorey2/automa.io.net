using ExcelTrans.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Unanet.Exports
{
    public class LaborCategoryModel : ModelBase
    {
        public string labor_category { get; set; }
        public string delete { get; set; }
        public string description { get; set; }
        public string bill_rate { get; set; }
        public string cost_rate { get; set; }
        public string external_system_code { get; set; }
        public string active { get; set; }
        public string effective_date { get; set; }
        //
        public string key { get; set; }

        public static Task<bool> ExportFile(UnanetClient una, string sourceFolder) =>
            Task.Run(() => una.GetEntitiesByExport(una.Exports["labor category master"].Item1, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("dateRange", "Today");
            }, sourceFolder));

        public static IEnumerable<LaborCategoryModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["labor category master"].Item2}.csv");
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new LaborCategoryModel
                {
                    labor_category = x[0],
                    delete = x[1],
                    description = x[2],
                    bill_rate = x[3],
                    cost_rate = x[4],
                    external_system_code = x[5],
                    active = x[6],
                    effective_date = x[7],
                    //
                    key = x.Count > 8 ? x[8] : null,
                }, 1).ToList();
        }

        public static IEnumerable<LaborCategoryModel> EnsureAndRead(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["labor category master"].Item2}.csv");
            if (!File.Exists(filePath))
                ExportFile(una, sourceFolder).Wait();
            return Read(una, sourceFolder);
        }
    }
}
