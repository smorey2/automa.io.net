using ExcelTrans.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class OrganizationContactModel : ModelBase
    {
        public string organization_code { get; set; }
        public string salutation { get; set; }
        public string first_name { get; set; }
        public string middle_initial { get; set; }
        public string last_name { get; set; }
        public string suffix { get; set; }
        public string title { get; set; }
        public string comment { get; set; }
        //
        public string active { get; set; }
        public string default_bill_to { get; set; }
        public string default_ship_to { get; set; }
        public string default_remit_to { get; set; }
        public string delete { get; set; }
        public string contact_category { get; set; }
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
        //
        public string organization_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string type = "CUSTOMER") =>
            Task.Run(() => una.GetEntitiesByExport(una.Exports["organization contact"].Item1, f =>
              {
                  f.Checked["suppressOutput"] = true;
                  f.FromSelect("organizationtype", type);
              }, sourceFolder));

        public static IEnumerable<OrganizationContactModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["organization contact"].Item2}.csv");
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new OrganizationContactModel
                {
                    organization_code = x[0],
                    salutation = x[1],
                    first_name = x[2],
                    middle_initial = x[3],
                    last_name = x[4],
                    suffix = x[5],
                    title = x[6],
                    comment = x[7],
                    //
                    active = x[8],
                    default_bill_to = x[9],
                    default_ship_to = x[10],
                    default_remit_to = x[11],
                    delete = x[12],
                    contact_category = x[13],
                    //
                    user01 = x[14],
                    user02 = x[15],
                    user03 = x[16],
                    user04 = x[17],
                    user05 = x[18],
                    user06 = x[19],
                    user07 = x[20],
                    user08 = x[21],
                    user09 = x[22],
                    user10 = x[23],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("oc", x.organization_code), XAttribute("s", x.salutation), XAttribute("fn", x.first_name), XAttribute("mi", x.middle_initial), XAttribute("ln", x.last_name), XAttribute("s2", x.suffix), XAttribute("t", x.title), XAttribute("c", x.comment),
                XAttribute("a", x.active), XAttribute("dbt", x.default_bill_to), XAttribute("dst", x.default_ship_to), XAttribute("drt", x.default_remit_to), XAttribute("cc", x.contact_category),
                XAttribute("u1", x.user01), XAttribute("u2", x.user02), XAttribute("u3", x.user03), XAttribute("u4", x.user04), XAttribute("u5", x.user05), XAttribute("u6", x.user06), XAttribute("u7", x.user07), XAttribute("u8", x.user08), XAttribute("u9", x.user09), XAttribute("u10", x.user10)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".o_oc.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_OrganizationContact1 : OrganizationContactModel
        {
            public int Id { get; set; }
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_OrganizationContact1 s, out string last)
        {
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.OrganizationContactChanged;
            var r = una.SubmitSubManage("Z", add ? HttpMethod.Post : HttpMethod.Put, "aaa",
                $"aaa={s.Id}", "", null,
                out last, (z, f) =>
            {
            });
            return r != null ?
                ManageFlags.OrganizationContactChanged :
                ManageFlags.None;
        }
    }
}