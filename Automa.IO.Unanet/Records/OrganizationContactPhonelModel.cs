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
    public class OrganizationContactPhoneModel : ModelBase
    {
        public string organization_code { get; set; }
        public string first_name { get; set; }
        public string middle_initial { get; set; }
        public string last_name { get; set; }
        public string suffix { get; set; }
        public string phone_type { get; set; }
        public string primary_ind { get; set; }
        //
        public string phone_number { get; set; }
        public string delete { get; set; }
        // custom
        public string organization_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_phone.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.organization_contact_phone.key, f =>
            {
                f.Checked["suppressOutput"] = true;
            }, sourceFolder));
        }

        public static IEnumerable<OrganizationContactPhoneModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_phone.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new OrganizationContactPhoneModel
                {
                    organization_code = x[0],
                    first_name = x[1],
                    middle_initial = x[2],
                    last_name = x[3],
                    suffix = x[4],
                    phone_type = x[5],
                    primary_ind = x[6],
                    //
                    phone_number = x[7],
                    delete = x[8],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("oc", x.organization_code), XAttribute("fn", x.first_name), XAttribute("mi", x.middle_initial), XAttribute("ln", x.last_name), XAttribute("s", x.suffix), XAttribute("pt", x.phone_type), XAttribute("pi", x.primary_ind),
                XAttribute("pn", x.phone_number)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".o_ocp.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_OrganizationContactPhone1 : OrganizationContactModel
        {
            public int Id { get; set; }
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_OrganizationContactPhone1 s, out Dictionary<string, (Type, object)> fields, out string last, Action<p_OrganizationContactPhone1> bespoke = null)
        {
            var _f = fields = new Dictionary<string, (Type, object)>();
            T _t<T>(T value, string name) { _f[name] = (typeof(T), value); return value; }
            //
            throw new Exception();
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.OrganizationContactChanged;
            var r = una.SubmitSubManage("Z", add ? HttpMethod.Post : HttpMethod.Put, "aaa", $"aaa={s.Id}",
                null, null,
                out last, (z, f) =>
            {
                return f.ToString();
            });
            return r != null ?
                ManageFlags.OrganizationContactChanged :
                ManageFlags.None;
        }
    }
}