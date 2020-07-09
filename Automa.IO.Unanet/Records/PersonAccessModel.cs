using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Automa.IO.Unanet.Records
{
    public class PersonAccessModel : ModelBase
    {
        public string username { get; set; }
        public string role { get; set; }
        public string access_type { get; set; }
        public string org_access { get; set; }
        public string legal_entity_ind { get; set; }
        //
        public string key { get; set; }
        public string usernameKey { get; set; }

        [Flags]
        public enum AccessTypes
        {
            All = Person | Project | Financial | Contact | Owning | Document | Vendor,
            Person = 0x0001,
            Project = 0x0002,
            Financial = 0x0004,
            Contact = 0x0008,
            Owning = 0x0010,
            Document = 0x0020,
            Vendor = 0x0040,
        }

        const int TimeoutInSeconds = 1200; // 20 minutes

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder, AccessTypes accessTypes = AccessTypes.All, string[] roles = null)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.organization_access.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExport(una.Settings.organization_access.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                // access type
                f.Checked["access_person"] = (accessTypes & AccessTypes.Person) == AccessTypes.Person;
                f.Checked["access_project"] = (accessTypes & AccessTypes.Project) == AccessTypes.Project;
                f.Checked["access_financial"] = (accessTypes & AccessTypes.Financial) == AccessTypes.Financial;
                f.Checked["access_contact"] = (accessTypes & AccessTypes.Contact) == AccessTypes.Contact;
                f.Checked["access_owning"] = (accessTypes & AccessTypes.Owning) == AccessTypes.Owning;
                f.Checked["access_document"] = (accessTypes & AccessTypes.Document) == AccessTypes.Document;
                f.Checked["access_vendor"] = (accessTypes & AccessTypes.Vendor) == AccessTypes.Vendor;
                // roles
                var idx = z.IndexOf("<td class=\"label\">Roles:</td>");
                var source = z.ExtractSpan("<table class=\"control-group\"", "</table>", idx);
                var doc = source.ToHtmlDocument();
                var elems = doc.DocumentNode.SelectNodes($"//td").ToArray();
                var elemsByName = elems.Where(x => !string.IsNullOrEmpty(x.InnerText)).ToDictionary(x => x.InnerText, x => x.SelectSingleNode("input").Attributes["value"].Value);
                var roleKeys = roles.Select(x => elemsByName[x]).ToArray();
                f.FromMultiCheckbox("rolekeys", roleKeys);
                return null;
            }, sourceFolder, timeoutInSeconds: TimeoutInSeconds));
        }

        public static IEnumerable<PersonAccessModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Settings.organization_access.file);
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new PersonAccessModel
                {
                    username = x[0],
                    role = x[1],
                    access_type = x[2],
                    org_access = x[3],
                    legal_entity_ind = x[4],
                    //
                    key = x[5],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("u", x.username), XAttribute("r", x.role), XAttribute("at", x.access_type), XAttribute("oa", x.org_access), XAttribute("lei", x.legal_entity_ind)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".p_a2.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_PersonAccess1 : PersonAccessModel
        {
            public string XA { get; set; }
            public string XCF { get; set; }
        }

        static DateTime FormExpires;
        static HtmlFormOptions FormOptions;
        static Dictionary<string, string> FormOrgTreesByName;

        static Dictionary<string, string> BuildOrgTreesByName(string source)
        {
            source = source.ExtractSpan("<div id=\"tree-data\">", "</div>");
            var doc = source.ToHtmlDocument();
            var elems = doc.DocumentNode.SelectNodes($"//span").ToArray();
            var elemsByName = elems.Where(x => !string.IsNullOrEmpty(x.InnerText)).ToDictionary(x => x.InnerText.Substring(0, x.InnerText.IndexOf("&#8211;")).Trim(), x => x.SelectSingleNode("input").Attributes["value"].Value);
            return elemsByName;
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_PersonAccess1 s, out Dictionary<string, (Type, object)> fields, out string last, Action<p_PersonAccess1> bespoke = null)
        {
            var _f = fields = new Dictionary<string, (Type, object)>();
            T _t<T>(T value, string name) { _f[name] = (typeof(T), value); return value; }
            //
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.OrganizationAddressChanged;
            if (add)
            {
                last = "role missing";
                return ManageFlags.None;
            }
            //
            var r = una.SubmitSubManage("0", HttpMethod.Get, "people/orgaccess/edit", null,
                $"personkey={s.usernameKey}&oapkey={s.key}", null,
                out last, (z, f) =>
            {
                // cache template
                if (DateTime.Now > FormExpires || FormOptions == null || FormOrgTreesByName == null)
                {
                    f.Values["personkey"] = f.Values["oapkey"] = null;
                    FormExpires = DateTime.Now.AddHours(3);
                    FormOptions = new HtmlFormOptions
                    {
                        FormTemplate = new HtmlFormTemplate(f)
                    };
                    FormOrgTreesByName = BuildOrgTreesByName(z);
                }
                if (!cf.Contains("oa"))
                    throw new ArgumentOutOfRangeException(nameof(cf));
                //
                f.Values["personkey"] = s.usernameKey;
                f.Values["oapkey"] = s.key;
                var org_access = _t(s.org_access, nameof(s.org_access));
                switch (org_access)
                {
                    case "!ALL!": f.FromSelectByKey("orgaccess", "all"); break;
                    case "!NONE!": f.FromSelectByKey("orgaccess", "none"); break;
                    default:
                        f.FromSelectByKey("orgaccess", "org");
                        var orgTreeKeys = org_access.Split(',').Select(x => FormOrgTreesByName[x.Trim()]).ToArray();
                        f.FromMultiCheckbox("orgTree_selected", orgTreeKeys);
                        break;
                }
                return f.ToString();
            }, formOptions: FormOptions);
            return r != null ?
                ManageFlags.PersonAccessChanged :
                ManageFlags.None;
        }
    }
}