using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class OrganizationAddressModel : ModelBase
    {
        public string organization_code { get; set; }
        public string street_address1 { get; set; }
        public string street_address2 { get; set; }
        public string street_address3 { get; set; }
        public string city { get; set; }
        public string state_province { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        //
        public string default_bill_to { get; set; }
        public string default_ship_to { get; set; }
        public string default_remit_to { get; set; }
        //
        public string organization_codeKey { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder, string type = "CUSTOMER")
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.organization_address.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.organization_address.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("organizationtype", type);
                return null;
            }, sourceFolder));
        }

        public static Dictionary<string, (string, string)[]> GetList(UnanetClient ctx, string orgKey) =>
            ctx.GetEntitiesBySubList("organizations/addresses", $"orgKey={orgKey}").Single();

        public static IEnumerable<OrganizationAddressModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.organization_address.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new OrganizationAddressModel
                {
                    organization_code = x[0],
                    street_address1 = x[1],
                    street_address2 = x[2],
                    street_address3 = x[3],
                    city = x[4],
                    state_province = x[5],
                    postal_code = x[6],
                    country = x[7],
                    //
                    default_bill_to = x[8],
                    default_ship_to = x[9],
                    default_remit_to = x[10],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("oc", x.organization_code), XAttribute("sa1", x.street_address1), XAttribute("sa2", x.street_address2), XAttribute("sa3", x.street_address3), XAttribute("c", x.city), XAttribute("sp", x.state_province), XAttribute("pc", x.postal_code), XAttribute("c2", x.country),
                XAttribute("dbt", x.default_bill_to), XAttribute("dst", x.default_ship_to), XAttribute("drt", x.default_remit_to)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".o_oa.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_OrganizationAddress1 : OrganizationAddressModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_OrganizationAddress1 s, out Dictionary<string, (Type, object)> fields, out string last, Action<p_OrganizationAddress1> bespoke = null)
        {
            string ToStreetAddress()
            {
                var b = new StringBuilder();
                if (!string.IsNullOrEmpty(_t(s.street_address1, nameof(s.street_address1)))) b.AppendLine(s.street_address1);
                if (!string.IsNullOrEmpty(_t(s.street_address2, nameof(s.street_address2)))) b.AppendLine(s.street_address2);
                if (!string.IsNullOrEmpty(_t(s.street_address3, nameof(s.street_address3)))) b.AppendLine(s.street_address3);
                return b.ToString();
            }
            var _f = fields = new Dictionary<string, (Type, object)>();
            T _t<T>(T value, string name) { _f[name] = (typeof(T), value); return value; }
            //
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.OrganizationAddressChanged;
            var list = add ? null : GetList(una, s.organization_codeKey);
            //if (list?.Count == 2 && Enumerable.SequenceEqual(list.ElementAt(0).Value, list.ElementAt(1).Value))
            //{
            //    ctx.SubmitSubList("organizations/addresses", list.ElementAt(1).Key, $"orgKey={s.organization_codeKey}", "&streetAddress=&city=&state=&postalCode=&country=", true, out last);
            //    return ManageFlags.OrganizationAddressChanged;
            //}
            if (list?.Count > 1) { last = $"list > 1"; return ManageFlags.OrganizationAddressChanged; }
            var key = list?.Single().Key;
            var r = una.SubmitSubManage("C", add ? HttpMethod.Post : HttpMethod.Put, "organizations/addresses", $"key={key}",
                $"orgKey={s.organization_codeKey}", "&streetAddress=&city=&state=&postalCode=&country=",
                out last, (z, f) =>
            {
                //if (add || cf.Contains("oc")) f.Values["xxx"] = _t(s.organization_code, nameof(s.organization_code))s.;
                if (add || cf.Contains("sa1") || cf.Contains("sa2") || cf.Contains("sa3")) f.Values["streetAddress"] = ToStreetAddress();
                if (add || cf.Contains("c")) f.Values["city"] = _t(s.city, nameof(s.city));
                if (add || cf.Contains("sp")) f.Values["state"] = _t(s.state_province, nameof(s.state_province));
                if (add || cf.Contains("pc")) f.Values["postalCode"] = _t(s.postal_code, nameof(s.postal_code));
                if (add || cf.Contains("c2")) f.Values["country"] = _t(s.country, nameof(s.country));
                //
                if (add || cf.Contains("dbt")) f.Checked["billTo"] = _t(s.default_bill_to, nameof(s.default_bill_to)) == "Y";
                if (add || cf.Contains("dst")) f.Checked["shipTo"] = _t(s.default_ship_to, nameof(s.default_ship_to)) == "Y";
                if (add || cf.Contains("drt")) f.Checked["remitTo"] = _t(s.default_remit_to, nameof(s.default_remit_to)) == "Y";
                return f.ToString();
            });
            return r != null ?
                ManageFlags.OrganizationAddressChanged :
                ManageFlags.None;
        }
    }
}