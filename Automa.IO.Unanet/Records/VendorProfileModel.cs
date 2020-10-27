using ExcelTrans.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class VendorProfileModel : ModelBase
    {
        public string organization_code { get; set; }
        public string legal_entity_code { get; set; }
        public string active { get; set; }
        public string payment_terms { get; set; }
        //
        public string account_number { get; set; }
        public string hold_payment { get; set; }
        public string hold_payment_reason { get; set; }
        public string payee_names { get; set; }
        public string separate_payment { get; set; }
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
            var filePath = Path.Combine(sourceFolder, una.Options.vendor_profile.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.vendor_profile.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.Checked["includeALL"] = true;
                f.Checked["includeSELECTED"] = false;
                f.Checked["suppress"] = false;
                //f.Values["legalEntities"] = legalEntityKey ?? una.Settings.DefaultOrg.key;
                return null;
            }, sourceFolder));
        }

        //public static async Task<Dictionary<string, (string, string)[]>> GetListAsync(UnanetClient ctx, string orgKey) =>
        //    (await ctx.GetEntitiesBySubListAsync("organizations/vendor_org", $"orgKey={orgKey}").ConfigureAwait(false)).Single();

        public static IEnumerable<VendorProfileModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.vendor_profile.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new VendorProfileModel
                {
                    organization_code = x[0],
                    legal_entity_code = x[1],
                    active = x[2],
                    payment_terms = x[3],
                    //
                    account_number = x[4],
                    hold_payment = x[5],
                    hold_payment_reason = x[6],
                    payee_names = x[7],
                    separate_payment = x[8],
                    //
                    user01 = x[9],
                    user02 = x[10],
                    user03 = x[11],
                    user04 = x[12],
                    user05 = x[13],
                    user06 = x[14],
                    user07 = x[15],
                    user08 = x[16],
                    user09 = x[17],
                    user10 = x[18],
                    delete = x[19],
                    user11 = x[20],
                    user12 = x[21],
                    user13 = x[22],
                    user14 = x[23],
                    user15 = x[24],
                    user16 = x[25],
                    user17 = x[26],
                    user18 = x[27],
                    user19 = x[28],
                    user20 = x[29],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("oc", x.organization_code), XAttribute("lec", x.legal_entity_code), XAttribute("a", x.active), XAttribute("pt", x.payment_terms),
                XAttribute("an", x.account_number), XAttribute("hp", x.hold_payment), XAttribute("hpr", x.hold_payment_reason), XAttribute("pn", x.payee_names), XAttribute("sp", x.separate_payment),
                XAttribute("u1", x.user01), XAttribute("u2", x.user02), XAttribute("u3", x.user03), XAttribute("u4", x.user04), XAttribute("u5", x.user05), XAttribute("u6", x.user06), XAttribute("u7", x.user07), XAttribute("u8", x.user08), XAttribute("u9", x.user09), XAttribute("u10", x.user10),
                XAttribute("u11", x.user11), XAttribute("u12", x.user12), XAttribute("u13", x.user13), XAttribute("u14", x.user14), XAttribute("u15", x.user15), XAttribute("u16", x.user16), XAttribute("u17", x.user17), XAttribute("u18", x.user18), XAttribute("u19", x.user19), XAttribute("u20", x.user20)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".o_vp.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_VendorProfile1 : VendorProfileModel
        {
            public string XCF { get; set; }
        }

        //public static async Task<(ChangedFields changed, string last)> ManageRecordAsync(UnanetClient una, p_VendorProfile1 s, Action<p_VendorProfile1> bespoke = null)
        //{
        //    var _ = new ChangedFields(ManageFlags.VendorProfileChanged);
        //    bespoke?.Invoke(s);
        //    if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out var last2))
        //        return (_.Changed(), last2);
        //    var list = add ? null : await GetListAsync(una, s.organization_codeKey).ConfigureAwait(false);
        //    if (list?.Count > 1) return (_.Changed(), $"list > 1");
        //    var key = list?.Single().Key;
        //    var (r, last) = await una.SubmitSubManageAsync("C", add ? HttpMethod.Post : HttpMethod.Put, "organizations/vendor_org", $"key={key}",
        //        $"orgKey={s.organization_codeKey}", "legalEntityOrg=-1&paymentTerm=1&active=true",
        //        (z, f) =>
        //    {
        //        //if (add || cf.Contains("oc")) f.FromSelect("xxx", _._(s.organization_code, nameof(s.organization_code)));
        //        if (add || cf.Contains("lec")) f.FromSelect("legalEntityOrg", _._(s.legal_entity_code, nameof(s.legal_entity_code)) ?? f.Selects["legalEntityOrg"].FirstOrDefault().Value);
        //        if (add || cf.Contains("a")) f.Checked["active"] = _._(s.active, nameof(s.active)) == "Y";
        //        if (add || cf.Contains("pt")) f.FromSelect("paymentTerm", _._(s.payment_terms, nameof(s.payment_terms)));
        //        //
        //if (add || cf.Contains("u1")) f.Values["udf_0"] = _._(s.user01, nameof(s.user01));
        //if (add || cf.Contains("u2")) f.Values["udf_1"] = _._(s.user02, nameof(s.user02));
        //if (add || cf.Contains("u3")) f.Values["udf_2"] = _._(s.user03, nameof(s.user03));
        //if (add || cf.Contains("u4")) f.Values["udf_3"] = _._(s.user04, nameof(s.user04));
        //if (add || cf.Contains("u5")) f.Values["udf_4"] = _._(s.user05, nameof(s.user05));
        //if (add || cf.Contains("u6")) f.Values["udf_5"] = _._(s.user06, nameof(s.user06));
        //if (add || cf.Contains("u7")) f.Values["udf_6"] = _._(s.user07, nameof(s.user07));
        //if (add || cf.Contains("u8")) f.Values("udf_7", _._(s.user08, nameof(s.user08)));
        //if (add || cf.Contains("u9")) f.Values["udf_8"] = _._(s.user09, nameof(s.user09));
        //if (add || cf.Contains("u10")) f.Values["udf_9"] = _._(s.user10, nameof(s.user10));
        //if (add || cf.Contains("u11")) f.Values["udf_10"] = _._(s.user11, nameof(s.user11));
        //if (add || cf.Contains("u12")) f.Values["udf_11"] = _._(s.user12, nameof(s.user12));
        //if (add || cf.Contains("u13")) f.Values["udf_12"] = _._(s.user13, nameof(s.user13));
        //if (add || cf.Contains("u14")) f.Values["udf_13"] = _._(s.user14, nameof(s.user14));
        //if (add || cf.Contains("u15")) f.Values["udf_14"] = _._(s.user15, nameof(s.user15));
        //if (add || cf.Contains("u16")) f.Values["udf_15"] = _._(s.user16, nameof(s.user16));
        //if (add || cf.Contains("u17")) f.Values["udf_16"] = _._(s.user17, nameof(s.user17));
        //if (add || cf.Contains("u18")) f.Values["udf_17"] = _._(s.user18, nameof(s.user18));
        //if (add || cf.Contains("u19")) f.Values["udf_18"] = _._(s.user19, nameof(s.user19));
        //if (add || cf.Contains("u20")) f.Values["udf_19"] = _._(s.user20, nameof(s.user20));
        //        return f.ToString();
        //    }).ConfigureAwait(false);
        //    return (_.Changed(r), last);
        //}
    }
}