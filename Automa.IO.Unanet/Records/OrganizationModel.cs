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
    public class OrganizationModel : ModelBase
    {
        public string organization_code { get; set; }
        public string organization_name { get; set; }
        public string parent_org_code { get; set; }
        public string org_type { get; set; }
        //
        public string size { get; set; }
        public string external_system_code { get; set; }
        public string sic_code { get; set; }
        public string classification { get; set; }
        public string industry { get; set; }
        public string sector { get; set; }
        public string stock_symbol { get; set; }
        public string url { get; set; }
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
        public string financial_org { get; set; }
        public string legal_entity { get; set; }
        public string legal_entity_code { get; set; }
        public string default_gl_post_org { get; set; }
        public string entry_allowed { get; set; }
        public string entry_begin_date { get; set; }
        public string entry_end_date { get; set; }
        public string financial_parent { get; set; }
        public string cost_pool_parent { get; set; }
        //
        public string active { get; set; }
        public string vendor_1099 { get; set; }
        public string recipient_name_1099 { get; set; }
        public string email_1099 { get; set; }
        public string federal_tax_id_type { get; set; }
        public string federal_tax_id { get; set; }
        public string start_with_proj_code_number { get; set; }
        //
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
        //
        public string key { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder, IEnumerable<string> types)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.organization.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.organization.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromMultiSelect("organizationtype", types);
                return null;
            }, sourceFolder));
        }

        public static async Task<Dictionary<string, Tuple<string, string>>> GetListAsync(UnanetClient una, IEnumerable<string> types) =>
            (await una.GetEntitiesByListAsync("organizations", null, (z, f) =>
            {
                f.FromMultiSelect("organizationtype", types);
                f.Values["list"] = "true";
            }).ConfigureAwait(false)).Single()
            .ToDictionary(x => x.Value[4].Item1, x => new Tuple<string, string>(x.Key, x.Value[5].Item1));

        public static IEnumerable<OrganizationModel> Read(UnanetClient una, string sourceFolder, IEnumerable<string> types)
        {
            var list = GetListAsync(una, types).GetAwaiter().GetResult();
            var filePath = Path.Combine(sourceFolder, una.Options.organization.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new OrganizationModel
                {
                    organization_code = x[0],
                    organization_name = x[1],
                    parent_org_code = x[2],
                    org_type = x[3],
                    //
                    size = x[4],
                    external_system_code = x[5],
                    sic_code = x[6],
                    classification = x[7],
                    industry = x[8],
                    sector = x[9],
                    stock_symbol = x[10],
                    url = x[11],
                    //
                    user01 = x[12],
                    user02 = x[13],
                    user03 = x[14],
                    user04 = x[15],
                    user05 = x[16],
                    user06 = x[17],
                    user07 = x[18],
                    user08 = x[19],
                    user09 = x[20],
                    user10 = x[21],
                    delete = x[22],
                    //
                    financial_org = x[23],
                    legal_entity = x[24],
                    legal_entity_code = x[25],
                    default_gl_post_org = x[26],
                    entry_allowed = x[27],
                    entry_begin_date = x[28],
                    entry_end_date = x[29],
                    financial_parent = x[30],
                    cost_pool_parent = x[31],
                    //
                    active = x[32],
                    vendor_1099 = x[33],
                    recipient_name_1099 = x[34],
                    email_1099 = x[35],
                    federal_tax_id_type = x[36],
                    federal_tax_id = x[37],
                    start_with_proj_code_number = x[38],
                    //
                    user11 = x[39],
                    user12 = x[40],
                    user13 = x[41],
                    user14 = x[42],
                    user15 = x[43],
                    user16 = x[44],
                    user17 = x[45],
                    user18 = x[46],
                    user19 = x[47],
                    user20 = x[48],
                    //
                    key = list.TryGetValue(x[0], out var item) ? item.Item1 : null,
                }, 1).ToList();
        }

        //public static IEnumerable<OrganizationModel> EnsureAndRead(UnanetClient una, string sourceFolder, string type)
        //{
        //    var filePath = Path.Combine(sourceFolder, una.Options.organization.file);
        //    if (!File.Exists(filePath))
        //        ExportFileAsync(una, sourceFolder, type);
        //    return Read(una, sourceFolder, type);
        //}

        public static string GetReadXml(UnanetClient una, string sourceFolder, IEnumerable<string> types, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder, types).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("oc", x.organization_code), XAttribute("on", x.organization_name), XAttribute("poc", x.parent_org_code), XAttribute("ot", x.org_type),
                XAttribute("s", x.size), XAttribute("esc", x.external_system_code), XAttribute("sc", x.sic_code), XAttribute("c", x.classification), XAttribute("i", x.industry), XAttribute("s2", x.sector), XAttribute("ss", x.stock_symbol), XAttribute("u", x.url),
                XAttribute("u1", x.user01), XAttribute("u2", x.user02), XAttribute("u3", x.user03), XAttribute("u4", x.user04), XAttribute("u5", x.user05), XAttribute("u6", x.user06), XAttribute("u7", x.user07), XAttribute("u8", x.user08), XAttribute("u9", x.user09), XAttribute("u10", x.user10),
                XAttribute("fo", x.financial_org), XAttribute("le", x.legal_entity), XAttribute("lec", x.legal_entity_code), XAttribute("dgpo", x.default_gl_post_org), XAttribute("ea", x.entry_allowed), XAttribute("ebd", x.entry_begin_date), XAttribute("eed", x.entry_end_date), XAttribute("fp", x.financial_parent), XAttribute("cpp", x.cost_pool_parent),
                XAttribute("a", x.active), XAttribute("v_1", x.vendor_1099), XAttribute("rn_1", x.recipient_name_1099), XAttribute("e_1", x.email_1099), XAttribute("ftit", x.federal_tax_id_type), XAttribute("fti", x.federal_tax_id), XAttribute("swpcn", x.start_with_proj_code_number),
                XAttribute("u11", x.user11), XAttribute("u12", x.user12), XAttribute("u13", x.user13), XAttribute("u14", x.user14), XAttribute("u15", x.user15), XAttribute("u16", x.user16), XAttribute("u17", x.user17), XAttribute("u18", x.user18), XAttribute("u19", x.user19), XAttribute("u20", x.user20)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".o.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_Organization1 : OrganizationModel
        {
            public string XCF { get; set; }
        }

        public static async Task<(ChangedFields changed, string last)> ManageRecordAsync(UnanetClient una, p_Organization1 s, Action<p_Organization1> bespoke = null)
        {
            if (s.org_type != "CUSTOMER") throw new NotSupportedException();
            var _ = new ChangedFields(ManageFlags.OrganizationChanged);
            bespoke?.Invoke(s);
            if (ManageRecordBase(s.key, s.XCF, 0, out var cf, out var add, out var last2))
                return (_.Changed(), last2);
            var (r, last) = await una.SubmitManageAsync(add ? HttpMethod.Post : HttpMethod.Put, "organizations",
                $"orgKey={s.key}",
                (z, f) =>
            {
                if (add || cf.Contains("oc")) f.Values["orgCode"] = _._(s.organization_code, nameof(s.organization_code));
                if (add || cf.Contains("on")) f.Values["orgName"] = _._(s.organization_name, nameof(s.organization_name));
                if (add) // || cf.Contains("poc"))
                    if (_._(s.parent_org_code, nameof(s.parent_org_code)) == "CLIENT_TRADE")
                    {
                        //f.Values["poupdate"] = "Y";
                        f.Values["porg"] = "CLIENT_TRADE - Client, Trade";
                        f.Values["parentOrg"] = "480";
                    }
                //else if (s.parent_org_code == "CLIENT_TRADE")
                //{
                //    //f.Values["poupdate"] = "Y";
                //    f.Values["porg"] = "CLIENT_Z_ARCHIVES - Client, Archives";
                //    f.Values["parentOrg"] = "507";
                //}
                //
                //if (add || cf.Contains("ot")) f.Values["xxx"] = _._(s.org_type, nameof(s.org_type));
                if (add || cf.Contains("s")) f.Values["orgSize"] = _._(s.size, nameof(s.size));
                if (add || cf.Contains("esc") || cf.Contains("bind")) f.Values["externalSystemCode"] = _._(s.external_system_code, nameof(s.external_system_code));
                if (add || cf.Contains("sic")) f.Values["sicCode"] = _._(s.sic_code, nameof(s.sic_code));
                if (add || cf.Contains("c")) f.Values["classification"] = _._(s.classification, nameof(s.classification));
                if (add || cf.Contains("i")) f.Values["industry"] = _._(s.industry, nameof(s.industry));
                if (add || cf.Contains("s2")) f.Values["sector"] = _._(s.sector, nameof(s.sector));
                if (add || cf.Contains("ss")) f.Values["stockSymbol"] = _._(s.stock_symbol, nameof(s.stock_symbol));
                if (add || cf.Contains("u")) f.Values["homepage"] = _._(s.url, nameof(s.url));
                //
                if (add || cf.Contains("u1")) f.Values["udf_0"] = _._(s.user01, nameof(s.user01));
                if (add || cf.Contains("u2")) f.Values["udf_1"] = _._(s.user02, nameof(s.user02));
                //if (add || cf.Contains("u3")) f.Values["udf_2"] = _._(s.user03, nameof(s.user03));
                //if (add || cf.Contains("u4")) f.Values["udf_3"] = _._(s.user04, nameof(s.user04));
                if (add || cf.Contains("u5")) f.Values["udf_4"] = _._(s.user05, nameof(s.user05));
                if (add || cf.Contains("u6")) f.Values["udf_5"] = _._(s.user06, nameof(s.user06));
                //if (add || cf.Contains("u7")) f.Values["udf_6"] = _._(s.user07, nameof(s.user07));
                if (add || cf.Contains("u8")) f.Values["udf_7"] = _._(s.user08, nameof(s.user08));
                if (add || cf.Contains("u9")) f.Values["udf_8"] = _._(s.user09, nameof(s.user09));
                if (add || cf.Contains("u10")) f.Values["udf_9"] = _._(s.user10, nameof(s.user10));
                //
                if (add || cf.Contains("fo")) f.Checked["financialOrg"] = _._(s.financial_org, nameof(s.financial_org)) == "Y";
                if (s.financial_org == "Y")
                {
                    if (add || cf.Contains("le")) f.FromSelectByKey("legalEntity", _._(s.legal_entity, nameof(s.legal_entity)));
                    //if (add || cf.Contains("lec")) f.Values["xxxx"] = _._(s.legal_entity_code, nameof(s.legal_entity_code));
                    if (add || cf.Contains("dgpo")) f.FromSelect("defaultGLPostOrg", _._(s.default_gl_post_org, nameof(s.default_gl_post_org)));
                    if (add || cf.Contains("ea")) f.Checked["entryAllowed"] = _._(s.entry_allowed, nameof(s.entry_allowed)) == "Y";
                    if (s.entry_allowed == "Y")
                    {
                        if (add || cf.Contains("ebd")) f.Values["beginDate"] = _._(s.entry_begin_date, nameof(s.entry_begin_date));
                        if (add || cf.Contains("eed")) f.Values["endDate"] = _._(s.entry_end_date, nameof(s.entry_end_date));
                    }
                    //if (add || cf.Contains("fp")) f.Values["finParentOrg"] = _._(s.financial_parent, nameof(s.financial_parent));
                    //if (add || cf.Contains("cpp")) f.Values["costPoolParentOrg"] = _._(s.cost_pool_parent, nameof(s.cost_pool_parent));
                }
                //
                if (add || cf.Contains("a")) f.Checked["active"] = _._(s.active, nameof(s.active)) == "Y";
                if (add || cf.Contains("v_1")) f.Checked["vendor1099"] = _._(s.vendor_1099, nameof(s.vendor_1099)) == "Y";
                if (add || cf.Contains("rn_1")) f.Values["recipientName"] = _._(s.recipient_name_1099, nameof(s.recipient_name_1099));
                if (add || cf.Contains("e_1")) f.Values["recipientEmail"] = _._(s.email_1099, nameof(s.email_1099));
                if (add || cf.Contains("ftit")) f.FromSelectByKey("fedTaxIdType", _._(s.federal_tax_id_type, nameof(s.federal_tax_id_type)));
                if (add || cf.Contains("fti")) f.Values["fedTaxId"] = _._(s.federal_tax_id, nameof(s.federal_tax_id));
                //if (add || cf.Contains("swpcn")) f.Values["xxxx"] = _._(s.start_with_proj_code_number, nameof(s.start_with_proj_code_number));
                //
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
                //
                if (add) f.FromSelect("orgType", "CUSTOMER");
                f.Add("button_save", "action", null);
                f.Remove("legalEntity", "defaultGLPostOrg", "legalEntityOrg",
                    "beginDate", "endDate", "fporg", "finParentOrg", "cporg", "costPoolParentOrg");
            }).ConfigureAwait(false);
            return (_.Changed(r), last);
        }
    }
}