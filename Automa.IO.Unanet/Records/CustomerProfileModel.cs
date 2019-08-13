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
        //
        public string organization_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntityKey = "2845")
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["customer profile"].Item2}.csv");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Exports["customer profile"].Item1, f =>
            {
                f.Checked["suppressOutput"] = true;
                f.Checked["includeALL"] = true;
                f.Checked["includeSELECTED"] = false;
                f.Checked["suppress"] = false;
                //f.Values["legalEntities"] = legalEntityKey;
            }, sourceFolder));
        }

        public static Dictionary<string, Tuple<string, string>[]> GetList(UnanetClient ctx, string orgKey) =>
            ctx.GetEntitiesBySubList("organizations/customer_org", $"orgKey={orgKey}").Single();

        public static IEnumerable<CustomerProfileModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["customer profile"].Item2}.csv");
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
            if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_CustomerProfile1 : CustomerProfileModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_CustomerProfile1 s, out string last)
        {
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.CustomerProfileChanged;
            var list = add ? null : GetList(una, s.organization_codeKey);
            if (list?.Count > 1) { last = $"list > 1"; return ManageFlags.CustomerProfileChanged; }
            var key = list?.Single().Key;
            var r = una.SubmitSubManage("C", add ? HttpMethod.Post : HttpMethod.Put, "organizations/customer_org",
                $"key={key}", $"orgKey={s.organization_codeKey}", "legalEntityOrg=-1&paymentTerm=1&active=true",
                out last, (z, f) =>
            {
                //if (add || cf.Contains("oc")) f.FromSelect("xxx", s.organization_code);
                if (add || cf.Contains("lec")) f.FromSelect("legalEntityOrg", s.legal_entity_code ?? f.Selects["legalEntityOrg"].FirstOrDefault().Value);
                if (add || cf.Contains("a")) f.Checked["active"] = s.active == "Y";
                if (add || cf.Contains("pt")) f.FromSelect("paymentTerm", s.payment_terms);
                //
                //if (add || cf.Contains("u1")) f.Values["udf_0"] = s.user01;
                //if (add || cf.Contains("u2")) f.Values["udf_1"] = s.user02;
                //if (add || cf.Contains("u3")) f.Values["udf_2"] = s.user03;
                //if (add || cf.Contains("u4")) f.Values["udf_3"] = s.user04;
                //if (add || cf.Contains("u5")) f.Values["udf_4"] = s.user05;
                //if (add || cf.Contains("u6")) f.Values["udf_5"] = s.user06;
                //if (add || cf.Contains("u7")) f.Values["udf_6"] = s.user07;
                //if (add || cf.Contains("u8")) f.Values("udf_7", s.user08);
                //if (add || cf.Contains("u9")) f.Values["udf_8"] = s.user09;
                //if (add || cf.Contains("u10")) f.Values["udf_9"] = s.user10;
            });
            return r != null ?
                ManageFlags.CustomerProfileChanged :
                ManageFlags.None;
        }
    }
}