using ExcelTrans.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Unanet.Exports
{
    public class LocationModel : ModelBase
    {
        public string location { get; set; }
        public string active { get; set; }
        public string delete { get; set; }
        //
        public string key { get; set; } //NEED

        public static Task<bool> ExportFile(UnanetClient una, string sourceFolder) =>
            Task.Run(() => una.GetEntitiesByExport(una.Exports["location master"].Item1, f =>
            {
                f.Checked["suppressOutput"] = true;
            }, sourceFolder));

        public static IEnumerable<LocationModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["location master"].Item2}.csv");
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new LocationModel
                {
                    location = x[0],
                    active = x[1],
                    delete = x[2],
                    key = x.Count > 3 ? x[3] : null,
                }, 1).ToList();
        }

        public static IEnumerable<LocationModel> EnsureAndRead(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["location master"].Item2}.csv");
            if (!File.Exists(filePath))
                ExportFile(una, sourceFolder).Wait();
            return Read(una, sourceFolder);
        }
    }
}
