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
    public class AlternateModel : ModelBase
    {
        public string username { get; set; }
        public string alternate_for_username { get; set; }
        public string role { get; set; }
        public string delete { get; set; }
        // custom
        public string usernameKey { get; set; }
        public string altenate_usernameKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["alternate"].Item2}.csv");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Exports["alternate"].Item1, f =>
            {
                f.Checked["suppressOutput"] = true;
            }, sourceFolder));
        }

        public static IEnumerable<AlternateModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["alternate"].Item2}.csv");
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new AlternateModel
                {
                    username = x[0],
                    alternate_for_username = x[1],
                    role = x[2],
                    delete = x[3],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("u", x.username), XAttribute("afu", x.alternate_for_username), XAttribute("r", x.role)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, "p_a.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Alternate1 : AlternateModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_Alternate1 s, out string last)
        {
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.AlternateChanged;
            var r = una.SubmitSubManage("D", HttpMethod.Get, $"people/alternates",
                $"personkey={s.usernameKey}", null, null,
                out last, (z, f) =>
                {
                    var roleKey = f.Selects["attributes"].First(x => x.Value.Replace(" ", "").ToLowerInvariant() == s.role.ToLowerInvariant()).Key;
                    f.Values["assign"] = $"{s.altenate_usernameKey};{roleKey}";
                    f.Add("button_save", "action", null);
                    return f.ToString();
                });
            return r != null ?
                ManageFlags.AlternateChanged :
                ManageFlags.None;
        }
    }
}
