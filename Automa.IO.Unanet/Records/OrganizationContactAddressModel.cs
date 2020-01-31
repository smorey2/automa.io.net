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
    public class OrganizationContactAddressModel : ModelBase
    {
        public string organization_code { get; set; }
        public string first_name { get; set; }
        public string middle_initial { get; set; }
        public string last_name { get; set; }
        public string suffix { get; set; }
        public string address_type { get; set; }
        public string primary_ind { get; set; }
        //
        public string street_address1 { get; set; }
        public string street_address2 { get; set; }
        public string street_address3 { get; set; }
        public string city { get; set; }
        public string state_province { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string delete { get; set; }
        // custom
        public string organization_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_address.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.organization_contact_address.key, f =>
            {
                f.Checked["suppressOutput"] = true;
            }, sourceFolder));
        }

        public static IEnumerable<OrganizationContactAddressModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_address.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new OrganizationContactAddressModel
                {
                    organization_code = x[0],
                    first_name = x[1],
                    middle_initial = x[2],
                    last_name = x[3],
                    suffix = x[4],
                    address_type = x[5],
                    primary_ind = x[6],
                    //
                    street_address1 = x[7],
                    street_address2 = x[8],
                    street_address3 = x[9],
                    city = x[10],
                    state_province = x[11],
                    postal_code = x[12],
                    country = x[13],
                    delete = x[14],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("oc", x.organization_code), XAttribute("fn", x.first_name), XAttribute("mi", x.middle_initial), XAttribute("ln", x.last_name), XAttribute("s", x.suffix), XAttribute("at", x.address_type), XAttribute("pi", x.primary_ind),
                XAttribute("st1", x.street_address1), XAttribute("st2", x.street_address2), XAttribute("st3", x.street_address3), XAttribute("c", x.city), XAttribute("sp", x.state_province), XAttribute("pc", x.postal_code), XAttribute("c2", x.country)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".o_oca.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_OrganizationContactAddress1 : OrganizationContactAddressModel
        {
            public int Id { get; set; }
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_OrganizationContactAddress1 s, out Dictionary<string, (Type, object)> fields, out string last, Action<p_OrganizationContactAddress1> bespoke = null)
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