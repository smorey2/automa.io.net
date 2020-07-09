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
    public class CustomerProfileModel : ModelBase
    {
        public string organization_code { get; set; }
        public string legal_entity_code { get; set; }
        public string active { get; set; }
        public string payment_terms { get; set; }
        //
        public string user01 { get; set; }
        public string user02 { get; set; }
        public string user03 { get; set; }
        public string user04 { get; set; }
        public string user05 { get; set; }
        public string user06 { get; set; }
        public string user07 { get; set; }
        public string user08 { get; set; }
        public string user09 { get; set; }
        public string user10 { get; set; }
        public string delete { get; set; }
        // NEW
        public string user11 { get; set; }
        public string user12 { get; set; }
        public string user13 { get; set; }
        public string user14 { get; set; }
        public string user15 { get; set; }
        public string user16 { get; set; }
        public string user17 { get; set; }
        public string user18 { get; set; }
        public string user19 { get; set; }
        public string user20 { get; set; }
        // custom
        public string organization_codeKey { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.customer_profile.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.customer_profile.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.Checked["includeALL"] = true;
                f.Checked["includeSELECTED"] = false;
                f.Checked["suppress"] = false;
                //f.Values["legalEntities"] = legalEntityKey ?? una.Settings.DefaultOrg.key;
                return null;
            }, sourceFolder));
        }

        public static Dictionary<string, (string, string)[]> GetList(UnanetClient ctx, string orgKey) =>
            ctx.GetEntitiesBySubList("organizations/customer_org", $"orgKey={orgKey}").Single();

        public static IEnumerable<CustomerProfileModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.customer_profile.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new CustomerProfileModel
                {
                    organization_code = x[0],
                    legal_entity_code = x[1],
                    active = x[2],
                    payment_terms = x[3],
                    //
                    user01 = x[4],
                    user02 = x[5],
                    user03 = x[6],
                    user04 = x[7],
                    user05 = x[8],
                    user06 = x[9],
                    user07 = x[10],
                    user08 = x[11],
                    user09 = x[12],
                    user10 = x[13],
                    delete = x[14],
                    // NEW
                    user11 = x[15],
                    user12 = x[16],
                    user13 = x[17],
                    user14 = x[18],
                    user15 = x[19],
                    user16 = x[20],
                    user17 = x[21],
                    user18 = x[22],
                    user19 = x[23],
                    user20 = x[24],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("oc", x.organization_code), XAttribute("lec", x.legal_entity_code), XAttribute("a", x.active), XAttribute("pt", x.payment_terms),
                XAttribute("u1", x.user01), XAttribute("u2", x.user02), XAttribute("u3", x.user03), XAttribute("u4", x.user04), XAttribute("u5", x.user05), XAttribute("u6", x.user06), XAttribute("u7", x.user07), XAttribute("u8", x.user08), XAttribute("u9", x.user09), XAttribute("u10", x.user10)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".o_cp.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_CustomerProfile1 : CustomerProfileModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_CustomerProfile1 s, out Dictionary<string, (Type, object)> fields, out string last, Action<p_CustomerProfile1> bespoke = null)
        {
            var _f = fields = new Dictionary<string, (Type, object)>();
            T _t<T>(T value, string name) { _f[name] = (typeof(T), value); return value; }
            //
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.CustomerProfileChanged;
            var list = add ? null : GetList(una, s.organization_codeKey);
            if (list?.Count > 1) { last = $"list > 1"; return ManageFlags.CustomerProfileChanged; }
            var key = list?.Single().Key;
            var r = una.SubmitSubManage("C", add ? HttpMethod.Post : HttpMethod.Put, "organizations/customer_org", $"key={key}",
                $"orgKey={s.organization_codeKey}", "legalEntityOrg=-1&paymentTerm=1&active=true",
                out last, (z, f) =>
            {
                //if (add || cf.Contains("oc")) f.FromSelect("xxx", _t(s.organization_code, nameof(s.organization_code)));
                if (add || cf.Contains("lec")) f.FromSelect("legalEntityOrg", _t(s.legal_entity_code, nameof(s.legal_entity_code)) ?? f.Selects["legalEntityOrg"].FirstOrDefault().Value);
                if (add || cf.Contains("a")) f.Checked["active"] = _t(s.active, nameof(s.active)) == "Y";
                if (add || cf.Contains("pt")) f.FromSelect("paymentTerm", _t(s.payment_terms, nameof(s.payment_terms)));
                //
                //if (add || cf.Contains("u1")) f.Values["udf_0"] = _t(s.user01, nameof(s.user01));
                //if (add || cf.Contains("u2")) f.Values["udf_1"] = _t(s.user02, nameof(s.user02));
                //if (add || cf.Contains("u3")) f.Values["udf_2"] = _t(s.user03, nameof(s.user03));
                //if (add || cf.Contains("u4")) f.Values["udf_3"] = _t(s.user04, nameof(s.user04));
                //if (add || cf.Contains("u5")) f.Values["udf_4"] = _t(s.user05, nameof(s.user05));
                //if (add || cf.Contains("u6")) f.Values["udf_5"] = _t(s.user06, nameof(s.user06));
                //if (add || cf.Contains("u7")) f.Values["udf_6"] = _t(s.user07, nameof(s.user07));
                //if (add || cf.Contains("u8")) f.Values("udf_7", _t(s.user08, nameof(s.user08)));
                //if (add || cf.Contains("u9")) f.Values["udf_8"] = _t(s.user09, nameof(s.user09));
                //if (add || cf.Contains("u10")) f.Values["udf_9"] = _t(s.user10, nameof(s.user10));
                return f.ToString();
            });
            return r != null ?
                ManageFlags.CustomerProfileChanged :
                ManageFlags.None;
        }
    }
}