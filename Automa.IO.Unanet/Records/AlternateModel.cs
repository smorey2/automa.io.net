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
        public string alternate_usernameKey { get; set; }

        public static Task<(bool success, bool hasFile)> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.alternate.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.alternate.key, f =>
            {
                f.Checked["suppressOutput"] = true;
            }, sourceFolder));
        }

        public static IEnumerable<AlternateModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.alternate.file);
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
            var syncFile = string.Format(syncFileA, ".p_a.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Alternate1 : AlternateModel
        {
            public string XA { get; set; }
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_Alternate1 s, out Dictionary<string, (Type, object)> fields, out string last, Action<p_Alternate1> bespoke = null)
        {
            var _f = fields = new Dictionary<string, (Type, object)>();
            T _t<T>(T value, string name) { _f[name] = (typeof(T), value); return value; }
            //
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last, canDelete: true))
                return ManageFlags.AlternateChanged;
            var method = !cf.Contains("delete") ? add ? HttpMethod.Post : HttpMethod.Put : HttpMethod.Delete;
            var r = una.SubmitSubManage("D", HttpMethod.Get, $"people/alternates", null,
                $"personkey={s.alternate_usernameKey}", null,
                out last, (z, f) =>
                {
                    var roleKey = f.Selects["attributes"].FirstOrDefault(x => x.Value.Replace(" ", "").ToLowerInvariant() == s.role.ToLowerInvariant()).Key;
                    if (roleKey == null)
                        return null;
                    f.Values[method == HttpMethod.Delete ? "unassign" : "assign"] = $"{s.usernameKey};{roleKey}";
                    f.Add("button_save", "action", null);
                    return f.ToString();
                });
            return r != null ?
                ManageFlags.AlternateChanged :
                ManageFlags.None;
        }
    }
}
