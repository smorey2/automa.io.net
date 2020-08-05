using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
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
        public string addresses { get; set; }
        public string emails { get; set; }
        public string phones { get; set; }
        public (string, string, string) Key => (organization_code, first_name, last_name);

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder, string type = "CUSTOMER")
        {
            AddressModel.ExportFileAsync(una, sourceFolder);
            EmailModel.ExportFileAsync(una, sourceFolder);
            PhoneModel.ExportFileAsync(una, sourceFolder);
            var filePath = Path.Combine(sourceFolder, una.Options.organization_contact.file);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.Run(() => una.GetEntitiesByExportAsync(una.Options.organization_contact.key, (z, f) =>
            {
                f.Checked["suppressOutput"] = true;
                f.FromSelect("organizationtype", type);
                return null;
            }, sourceFolder));
        }

        public static Task<Dictionary<string, (string, string)[]>> GetListAsync(UnanetClient ctx, string orgKey) =>
            Single(ctx.GetEntitiesBySubListAsync("organizations/contacts", $"orgKey={orgKey}"));

        public static IEnumerable<OrganizationContactModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, una.Options.organization_contact.file);
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
                    // NEW
                    user11 = x[24],
                    user12 = x[25],
                    user13 = x[26],
                    user14 = x[27],
                    user15 = x[28],
                    user16 = x[29],
                    user17 = x[30],
                    user18 = x[31],
                    user19 = x[32],
                    user20 = x[33],
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var addresses = AddressModel.Read(una, sourceFolder).OrderBy(x => x.street_address1).ToLookup(x => x.Key, x => x.Value);
            var emails = EmailModel.Read(una, sourceFolder).OrderBy(x => x.email_address).ToLookup(x => x.Key, x => x.Value);
            var phones = PhoneModel.Read(una, sourceFolder).OrderBy(x => x.phone_number).ToLookup(x => x.Key, x => x.Value);
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p",
                XAttribute("oc", x.organization_code), XAttribute("s", x.salutation), XAttribute("fn", x.first_name), XAttribute("mi", x.middle_initial), XAttribute("ln", x.last_name), XAttribute("s2", x.suffix), XAttribute("t", x.title), XAttribute("c", x.comment),
                XAttribute("a", x.active), XAttribute("dbt", x.default_bill_to), XAttribute("dst", x.default_ship_to), XAttribute("drt", x.default_remit_to), XAttribute("cc", x.contact_category),
                XAttribute("u1", x.user01), XAttribute("u2", x.user02), XAttribute("u3", x.user03), XAttribute("u4", x.user04), XAttribute("u5", x.user05), XAttribute("u6", x.user06), XAttribute("u7", x.user07), XAttribute("u8", x.user08), XAttribute("u9", x.user09), XAttribute("u10", x.user10),
                addresses[x.Key].ToArray(), emails[x.Key].ToArray(), phones[x.Key].ToArray()
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".o_c.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        #region Address/Email/Phone

        public class AddressModel
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
            public (string, string, string) Key => (organization_code, first_name, last_name);
            public XElement Value => new XElement("a",
                XAttribute("at", address_type), XAttribute("pi", primary_ind),
                XAttribute("st1", street_address1), XAttribute("st2", street_address2), XAttribute("st3", street_address3), XAttribute("c", city), XAttribute("sp", state_province), XAttribute("pc", postal_code), XAttribute("c2", country)
            );

            public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder)
            {
                return Task.FromResult(true);
                //var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_address.file);
                //if (File.Exists(filePath))
                //    File.Delete(filePath);
                //return Task.Run(() => una.GetEntitiesByExport(una.Settings.organization_contact_address.key, f =>
                //{
                //    f.Checked["suppressOutput"] = true;
                //}, sourceFolder));
            }

            public static IEnumerable<AddressModel> Read(UnanetClient una, string sourceFolder)
            {
                return Enumerable.Empty<AddressModel>();
                //var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_address.file);
                //using (var sr = File.OpenRead(filePath))
                //    return CsvReader.Read(sr, x => new AddressModel
                //    {
                //        organization_code = x[0],
                //        first_name = x[1],
                //        middle_initial = x[2],
                //        last_name = x[3],
                //        suffix = x[4],
                //        address_type = x[5],
                //        primary_ind = x[6],
                //        //
                //        street_address1 = x[7],
                //        street_address2 = x[8],
                //        street_address3 = x[9],
                //        city = x[10],
                //        state_province = x[11],
                //        postal_code = x[12],
                //        country = x[13],
                //        delete = x[14],
                //    }, 1).ToList();
            }

            public static void ManageRecord(HtmlFormPost f, string xml)
            {
                int i;
                var exists = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var count = int.Parse(f.Values["addresscnt"]);
                for (i = 1; i <= count; i++)
                {
                    exists.Add(f.Values[$"street_{i}"], f.Values[$"streetKey_{i}"]);
                    f.Remove($"streetKey_{i}", $"street_{i}", $"city_{i}", $"state_province_{i}", $"postal_code_{i}", $"country_{i}", $"cntAddressType_{i}", $"primaryAddress_{i}");
                }
                i = 0;
                var xml2 = XDocument.Parse($"<r>{xml}</r>");
                var typeOptions = f.Selects[$"cntAddressType_0"].ToDictionary(x => x.Value, x => x.Key);
                foreach (var item in xml2.Descendants("a"))
                {
                    i++;
                    var (street, city, state_province, postal_code, country, addressType, primaryIndicator) = (
                        item.Attributes("sa").FirstOrDefault()?.Value,
                        item.Attributes("c").FirstOrDefault()?.Value,
                        item.Attributes("sp").FirstOrDefault()?.Value,
                        item.Attributes("pc").FirstOrDefault()?.Value,
                        item.Attributes("c").FirstOrDefault()?.Value,
                        item.Attributes("at").FirstOrDefault()?.Value,
                        item.Attributes("pi").FirstOrDefault()?.Value == "Y");
                    if (exists.TryGetValue(street, out var exist))
                    {
                        exists.Remove(street);
                        f.Add($"streetKey_{i}", "hidden", exist);
                    }
                    f.Add($"street_{i}", "text", street);
                    f.Add($"city_{i}", "text", city);
                    f.Add($"state_province_{i}", "text", state_province);
                    f.Add($"postal_code_{i}", "text", postal_code);
                    f.Add($"country_{i}", "text", country);
                    f.Add($"cntAddressType_{i}", "select", typeOptions.TryGetValue(addressType, out var option) ? option : null);
                    f.Add($"primaryAddress_{i}", "checkbox", "true", primaryIndicator);
                }
                f.Values["addresscnt"] = $"{i}";
            }
        }

        public class EmailModel
        {
            public string organization_code { get; set; }
            public string first_name { get; set; }
            public string middle_initial { get; set; }
            public string last_name { get; set; }
            public string suffix { get; set; }
            public string email_type { get; set; }
            public string primary_ind { get; set; }
            //
            public string email_address { get; set; }
            public string delete { get; set; }
            // custom
            public (string, string, string) Key => (organization_code, first_name, last_name);
            public XElement Value => new XElement("e",
                XAttribute("et", email_type), XAttribute("pi", primary_ind),
                XAttribute("ea", email_address)
            );

            public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder)
            {
                return Task.FromResult((true, (string)null, false, (object)null));
                //var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_email.file);
                //if (File.Exists(filePath))
                //    File.Delete(filePath);
                //return Task.Run(() => una.GetEntitiesByExport(una.Settings.organization_contact_email.key, (z, f) =>
                //{
                //    f.Checked["suppressOutput"] = true;
                //    return null;
                //}, sourceFolder));
            }

            public static IEnumerable<EmailModel> Read(UnanetClient una, string sourceFolder)
            {
                return Enumerable.Empty<EmailModel>();
                //var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_email.file);
                //using (var sr = File.OpenRead(filePath))
                //    return CsvReader.Read(sr, x => new EmailModel
                //    {
                //        organization_code = x[0],
                //        first_name = x[1],
                //        middle_initial = x[2],
                //        last_name = x[3],
                //        suffix = x[4],
                //        email_type = x[5],
                //        primary_ind = x[6],
                //        //
                //        email_address = x[7],
                //        delete = x[8],
                //    }, 1).ToList();
            }

            public static void ManageRecord(HtmlFormPost f, string xml)
            {
                int i;
                var exists = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var count = int.Parse(f.Values["emlcnt"]);
                for (i = 1; i <= count; i++)
                {
                    exists.Add(f.Values[$"email_{i}"], f.Values[$"emailKey_{i}"]);
                    f.Remove($"emailKey_{i}", $"email_{i}", $"cntEmailType_{i}", $"primaryEmail_{i}");
                }
                i = 0;
                var xml2 = XDocument.Parse($"<r>{xml}</r>");
                var typeOptions = f.Selects[$"cntEmailType_0"].ToDictionary(x => x.Value, x => x.Key);
                foreach (var item in xml2.Descendants("e"))
                {
                    i++;
                    var (email, emailType, primaryIndicator) = (item.Attributes("ea").FirstOrDefault()?.Value, item.Attributes("et").FirstOrDefault()?.Value, item.Attributes("pi").FirstOrDefault()?.Value == "Y");
                    if (exists.TryGetValue(email, out var exist))
                    {
                        exists.Remove(email);
                        f.Add($"emailKey_{i}", "hidden", exist);
                    }
                    f.Add($"email_{i}", "text", email);
                    f.Add($"cntEmailType_{i}", "select", typeOptions.TryGetValue(emailType, out var option) ? option : null);
                    f.Add($"primaryEmail_{i}", "checkbox", "true", primaryIndicator);
                }
                f.Values["emlcnt"] = $"{i}";
            }
        }

        public class PhoneModel
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
            public (string, string, string) Key => (organization_code, first_name, last_name);
            public XElement Value => new XElement("n",
                XAttribute("pt", phone_type), XAttribute("pi", primary_ind),
                XAttribute("pn", phone_number)
            );

            public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder)
            {
                return Task.FromResult(true);
                //var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_phone.file);
                //if (File.Exists(filePath))
                //    File.Delete(filePath);
                //return Task.Run(() => una.GetEntitiesByExport(una.Settings.organization_contact_phone.key, f =>
                //{
                //    f.Checked["suppressOutput"] = true;
                //}, sourceFolder));
            }

            public static IEnumerable<PhoneModel> Read(UnanetClient una, string sourceFolder)
            {
                return Enumerable.Empty<PhoneModel>();
                //var filePath = Path.Combine(sourceFolder, una.Settings.organization_contact_phone.file);
                //using (var sr = File.OpenRead(filePath))
                //    return CsvReader.Read(sr, x => new PhoneModel
                //    {
                //        organization_code = x[0],
                //        first_name = x[1],
                //        middle_initial = x[2],
                //        last_name = x[3],
                //        suffix = x[4],
                //        phone_type = x[5],
                //        primary_ind = x[6],
                //        //
                //        phone_number = x[7],
                //        delete = x[8],
                //    }, 1).ToList();
            }

            public static void ManageRecord(HtmlFormPost f, string xml)
            {
                int i;
                var exists = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var count = int.Parse(f.Values["phonecnt"]);
                for (i = 1; i <= count; i++)
                {
                    exists.Add(f.Values[$"phone_{i}"], f.Values[$"phoneKey_{i}"]);
                    f.Remove($"phoneKey_{i}", $"phone_{i}", $"cntPhoneType_{i}", $"primaryPhone_{i}");
                }
                i = 0;
                var xml2 = XDocument.Parse($"<r>{xml}</r>");
                var typeOptions = f.Selects[$"cntPhoneType_0"].ToDictionary(x => x.Value, x => x.Key);
                foreach (var item in xml2.Descendants("n"))
                {
                    i++;
                    var (phone, phoneType, primaryIndicator) = (item.Attributes("pn").FirstOrDefault()?.Value, item.Attributes("pt").FirstOrDefault()?.Value, item.Attributes("pi").FirstOrDefault()?.Value == "Y");
                    if (exists.TryGetValue(phone, out var exist))
                    {
                        exists.Remove(phone);
                        f.Add($"phoneKey_{i}", "hidden", exist);
                    }
                    f.Add($"phone_{i}", "text", phone);
                    f.Add($"cntPhoneType_{i}", "select", typeOptions.TryGetValue(phoneType, out var option) ? option : null);
                    f.Add($"primaryPhone_{i}", "checkbox", "true", primaryIndicator);
                }
                f.Values["phonecnt"] = $"{i}";
            }
        }

        #endregion

        public class p_OrganizationContact1 : OrganizationContactModel
        {
            public int Id { get; set; }
            public string XCF { get; set; }
        }

        public static async Task<(ChangedFields changed, string last)> ManageRecordAsync(UnanetClient una, p_OrganizationContact1 s, Action<p_OrganizationContact1> bespoke = null)
        {
            var _ = new ChangedFields(ManageFlags.OrganizationContactChanged);
            bespoke?.Invoke(s);
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out var last2))
                return (_.Changed(), last2);
            var list = add ? null : await GetListAsync(una, s.organization_codeKey);
            //var key0 = list?.Where(x => string.Equals(x.Value[3].Item1, $"DEG, {s.last_name}", StringComparison.OrdinalIgnoreCase)).SingleOrDefault().Key;
            var key1 = list?.Where(x => string.Equals(x.Value[3].Item1, $"{s.last_name}, {s.first_name}", StringComparison.OrdinalIgnoreCase)).SingleOrDefault().Key;
            var key = key1;
            if (!add && key == null)
                throw new Exception("unable to find record");
            var (r, last) = await una.SubmitSubManageAsync("A", add ? HttpMethod.Post : HttpMethod.Put, "organizations/contacts", $"contactKey={key}&orgKey={s.organization_codeKey}",
                 $"orgKey={s.organization_codeKey}", null,
                (z, f) =>
            {
                //if (add || cf.Contains("oc")) f.Values["xxx"] = _._(s.organization_code, nameof(s.organization_code))s.;
                if (add || cf.Contains("s")) f.Values["salutation"] = _._(s.salutation, nameof(s.salutation));
                if (add || cf.Contains("fn")) f.Values["first_name"] = _._(s.first_name, nameof(s.first_name));
                if (add || cf.Contains("mi")) f.Values["middleInitial"] = _._(s.middle_initial, nameof(s.middle_initial));
                if (add || cf.Contains("ln")) f.Values["last_name"] = _._(s.last_name, nameof(s.last_name));
                if (add || cf.Contains("s2")) f.Values["suffix"] = _._(s.suffix, nameof(s.suffix));
                if (add || cf.Contains("t")) f.Values["title"] = _._(s.title, nameof(s.title));
                if (add || cf.Contains("c") || cf.Contains("bind")) f.Values["comments"] = _._(s.comment, nameof(s.comment));
                //
                if (add || cf.Contains("a")) f.Checked["active"] = _._(s.active, nameof(s.active)) == "Y";
                if (add || cf.Contains("dbt")) f.Checked["default_bill_to"] = _._(s.default_bill_to, nameof(s.default_bill_to)) == "Y";
                if (add || cf.Contains("dst")) f.Checked["default_ship_to"] = _._(s.default_ship_to, nameof(s.default_ship_to)) == "Y";
                if (add || cf.Contains("drt")) f.Checked["default_remit_to"] = _._(s.default_remit_to, nameof(s.default_remit_to)) == "Y";
                //if (add || cf.Contains("cc")) f.Values["xxxx"] = _._(s.contact_category, nameof(s.contact_category)); //: no field
                //
                if (add || cf.Contains("u1")) f.Values["udf_0"] = _._(s.user01, nameof(s.user01));
                //if (add || cf.Contains("u2")) f.Values["udf_1"] = _._(s.user02, nameof(s.user02));
                //if (add || cf.Contains("u3")) f.Values["udf_2"] = _._(s.user03, nameof(s.user03));
                //if (add || cf.Contains("u4")) f.Values["udf_3"] = _._(s.user04, nameof(s.user04));
                //if (add || cf.Contains("u5")) f.Values["udf_4"] = _._(s.user05, nameof(s.user05));
                //if (add || cf.Contains("u6")) f.Values["udf_5"] = _._(s.user06, nameof(s.user06));
                //if (add || cf.Contains("u7")) f.Values["udf_6"] = _._(s.user07, nameof(s.user07));
                //if (add || cf.Contains("u8")) f.Values["udf_7"] = _._(s.user08, nameof(s.user08));
                //if (add || cf.Contains("u9")) f.Values["udf_8"] = _._(s.user09, nameof(s.user09));
                //if (add || cf.Contains("u10")) f.Values["udf_9"] = _._(s.user10, nameof(s.user10));
                //
                //if (add || cf.Contains("xa")) AddressModel.ManageRecord(f, s.addresses);
                //if (add || cf.Contains("xe")) EmailModel.ManageRecord(f, s.emails);
                //if (add || cf.Contains("xn")) PhoneModel.ManageRecord(f, s.phones);
                return f.ToString();
            });
            return (_.Changed(r), last);
        }
    }
}