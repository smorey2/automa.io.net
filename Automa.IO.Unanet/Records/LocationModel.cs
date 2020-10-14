using ExcelTrans.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    // MASTER
    public class LocationModel : ModelBase
    {
        public string key { get; set; }
        //
        public string location { get; set; }
        public string active { get; set; }
        public string delete { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.location_master.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.location_master.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                return null;
            }, sourceFolder));
        }

        public static IEnumerable<LocationModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.location_master.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new LocationModel
                {
                    key = x[0],
                    //
                    location = x[1],
                    active = x[2],
                    delete = x[3],
                }, 1).ToList();
        }

        public static Task<IEnumerable<LocationModel>> EnsureAndReadAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.location_master.file);
            if (!File.Exists(filePath))
                ExportFileAsync(una, sourceFolder).Wait();
            return Task.FromResult(Read(una, sourceFolder));
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("l", x.location), XAttribute("a", x.active)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".l.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }
    }
}
