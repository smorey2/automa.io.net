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

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.alternate.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.alternate.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                return null;
            }, sourceFolder));
        }

        public static IEnumerable<AlternateModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.alternate.file);
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

        public static async Task<(ChangedFields changed, string last)> ManageRecordAsync(UnanetClient una, p_Alternate1 s, Action<p_Alternate1> bespoke = null)
        {
            var _ = new ChangedFields(ManageFlags.AlternateChanged);
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out var last2, canDelete: true))
                return (_.Changed(), last2);
            var method = !cf.Contains("delete") ? add ? HttpMethod.Post : HttpMethod.Put : HttpMethod.Delete;
            var (r, last) = await una.SubmitSubManageAsync("D", HttpMethod.Get, $"people/alternates", null,
                $"personkey={s.alternate_usernameKey}", null,
                (z, f) =>
                {
                    var roleKey = f.Selects["attributes"].FirstOrDefault(x => x.Value.Replace(" ", "").ToLowerInvariant() == s.role.ToLowerInvariant()).Key;
                    if (roleKey == null)
                        return null;
                    f.Values[method == HttpMethod.Delete ? "unassign" : "assign"] = $"{s.usernameKey};{roleKey}";
                    f.Add("button_save", "action", null);
                    return f.ToString();
                }).ConfigureAwait(false);
            return (_.Changed(r), last);
        }
    }
}
