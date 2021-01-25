namespace Automa.IO.Unanet
{
    /// <summary>
    /// IUnanetOptions
    /// </summary>
    public interface IUnanetOptions
    {
        string UnanetUri { get; }
        string LegalEntity { get; }
        (string key, string name) DefaultOrg { get; }
        string WipAcct { get; }
        string[] LaborAccts { get; }

        // EXPORTS
        (string key, string file) location_master { get; }
        (string key, string file) labor_category_master { get; }
        //(string key, string file) vendor_profile { get; }

        // Organization
        (string key, string file) organization { get; }
        (string key, string file) customer_profile { get; }
        (string key, string file) vendor_profile { get; }
        (string key, string file) organization_access { get; }
        (string key, string file) organization_address { get; }
        (string key, string file) organization_contact { get; }
        (string key, string file) organization_contact_address { get; }
        (string key, string file) organization_contact_email { get; }
        (string key, string file) organization_contact_phone { get; }

        // Project
        (string key, string file) project { get; }
        (string key, string file) task { get; }
        (string key, string file) fixed_price_item { get; }
        (string key, string file) fixed_price_item_post { get; }
        (string key, string file) assignment { get; }
        (string key, string file) project_administrator { get; }
        (string key, string file) labor_category_project { get; }
        (string key, string file) project_invoice_setup { get; }

        // Person
        (string key, string file) person { get; }
        (string key, string file) alternate { get; }
        (string key, string file) approval_group { get; }

        // Time/Invoice
        (string key, string file) time { get; }
        (string key, string file) invoice { get; }

        // IMPORTS
        (string key, string file) credit_card_generic { get; }
    }

    public class UnanetOptions : IUnanetOptions
    {
        public string UnanetUri { get; set; }
        public string LegalEntity { get; set; }
        public (string key, string name) DefaultOrg { get; set; }
        public string WipAcct { get; set; }
        public string[] LaborAccts { get; set; }
        public (string key, string file) location_master { get; set; }
        public (string key, string file) labor_category_master { get; set; }
        public (string key, string file) organization { get; set; }
        public (string key, string file) customer_profile { get; set; }
        public (string key, string file) vendor_profile { get; set; }
        public (string key, string file) organization_access { get; set; }
        public (string key, string file) organization_address { get; set; }
        public (string key, string file) organization_contact { get; set; }
        public (string key, string file) organization_contact_address { get; set; }
        public (string key, string file) organization_contact_email { get; set; }
        public (string key, string file) organization_contact_phone { get; set; }
        public (string key, string file) project { get; set; }
        public (string key, string file) task { get; set; }
        public (string key, string file) fixed_price_item { get; set; }
        public (string key, string file) fixed_price_item_post { get; set; }
        public (string key, string file) assignment { get; set; }
        public (string key, string file) project_administrator { get; set; }
        public (string key, string file) labor_category_project { get; set; }
        public (string key, string file) project_invoice_setup { get; set; }
        public (string key, string file) person { get; set; }
        public (string key, string file) alternate { get; set; }
        public (string key, string file) approval_group { get; set; }
        public (string key, string file) time { get; set; }
        public (string key, string file) invoice { get; set; }
        public (string key, string file) credit_card_generic { get; set; }
    }

    /// <summary>
    /// RoundarchUnanetOptions.
    /// </summary>
    /// <seealso cref="Automa.IO.Unanet.IUnanetOptions" />
    public class RoundarchUnanetOptions : IUnanetOptions
    {
        public string UnanetUri => "https://roundarch.unanet.biz/roundarch/action";
        public string LegalEntity => "75-00-DEG-00 - Digital Evolution Group, LLC";
        public (string, string) DefaultOrg => ("2845", "75-00-DEG-00");
        public string WipAcct => "1420";
        public string[] LaborAccts => new[] { "5108", "6400", "6920", "8310", "6409" };

        // EXPORTS
        public (string, string) location_master => ("818", "DEG_ Location - Master.csv");
        public (string, string) labor_category_master => ("814", "DEG_ Labor Category - Master.csv");

        // Organization
        public (string, string) organization => ("1172", "Organization.csv");
        public (string, string) customer_profile => ("1155", "Customer Profile.csv");
        public (string, string) vendor_profile => ("1084", "DEG_ Vendor Profile.csv");
        public (string, string) organization_access => ("1015", "DEG_ Organization Access.csv");
        public (string, string) organization_address => ("1174", "Organization Address.csv");
        public (string, string) organization_contact => ("1175", "Organization Contact.csv");
        public (string, string) organization_contact_address => ("1176", "Organization Contact Address.csv");
        public (string, string) organization_contact_email => ("1177", "Organization Contact Email.csv");
        public (string, string) organization_contact_phone => ("1178", "Organization Contact Phone.csv");

        // Project
        public (string, string) project => ("810", "DEG_ Project.csv");
        public (string, string) task => ("820", "DEG_ Task.csv");
        public (string, string) fixed_price_item => ("873", "DEG_ Fixed Price Item.csv");
        public (string, string) fixed_price_item_post => ("874", "DEG_ Fixed Price Item [post].csv");
        public (string, string) assignment => ("812", "DEG_ Assignment.csv");
        public (string, string) project_administrator => ("1185", "Project Administrators.csv");
        public (string, string) labor_category_project => ("817", "DEG_ Labor Category - Project.csv");
        public (string, string) project_invoice_setup => ("1187", "Project Invoice Setup.csv");

        // Person
        public (string, string) person => ("1199", "DEG_ Person.csv");
        public (string, string) alternate => ("1151", "Alternate.csv");
        public (string, string) approval_group => ("1152", "Approval Group.csv");

        // Time/Invoice
        public (string, string) time => ("819", "DEG_ Time.csv");
        public (string, string) invoice => ("724", "Invoice Export.csv");

        // IMPORTS
        public (string, string) credit_card_generic => ("GenericCreditCardImport", @"C:\GenericCreditCard.csv");
    }

    /// <summary>
    /// RoundarchSandUnanetOptions.
    /// </summary>
    /// <seealso cref="Automa.IO.Unanet.IUnanetOptions" />
    public class RoundarchSandUnanetOptions : IUnanetOptions
    {
        public string UnanetUri => "https://roundarch-sand.unanet.biz/roundarch-sand/action";
        public string LegalEntity => "75-00-DEG-00 - Digital Evolution Group, LLC";
        public (string, string) DefaultOrg => ("2845", "75-00-DEG-00");
        public string WipAcct => "1420";
        public string[] LaborAccts => new[] { "5108", "6400", "6920" };

        // EXPORTS
        public (string, string) location_master => ("0", "DEG_ Location - Master.csv");
        public (string, string) labor_category_master => ("0", "DEG_ Labor Category - Master.csv");

        // Organization
        public (string, string) organization => ("0", "DEG_ Organization.csv");
        public (string, string) customer_profile => ("0", "Customer Profile.csv");
        public (string, string) vendor_profile => ("0", "DEG_ Vendor Profile.csv");
        public (string, string) organization_access => ("0", "DEG_ Organization Access.csv");
        public (string, string) organization_address => ("0", "Organization Address.csv");
        public (string, string) organization_contact => ("0", "Organization Contact.csv");
        public (string, string) organization_contact_address => ("0", "Organization Contact Address.csv");
        public (string, string) organization_contact_email => ("0", "Organization Contact Email.csv");
        public (string, string) organization_contact_phone => ("0", "Organization Contact Phone.csv");

        // Project
        public (string, string) project => ("0", "DEG_ Project.csv");
        public (string, string) task => ("0", "DEG_ Task.csv");
        public (string, string) fixed_price_item => ("0", "DEG_ Fixed Price Item.csv");
        public (string, string) fixed_price_item_post => ("0", "DEG_ Fixed Price Item [post].csv");
        public (string, string) assignment => ("0", "DEG_ Assignment.csv");
        public (string, string) project_administrator => ("0", "Project Administrators.csv");
        public (string, string) labor_category_project => ("0", "DEG_ Labor Category - Project.csv");
        public (string, string) project_invoice_setup => ("0", "Project Invoice Setup.csv");

        // Person
        public (string, string) person => ("0", "DEG_ Person.csv");
        public (string, string) alternate => ("0", "Alternate.csv");
        public (string, string) approval_group => ("0", "Approval Group.csv");

        // Time/Invoice
        public (string, string) time => ("0", "DEG_ Time.csv");
        public (string, string) invoice => ("0", "Invoice Export.csv");

        // IMPORTS
        public (string, string) credit_card_generic => ("GenericCreditCardImport", @"C:\GenericCreditCard.csv");
    }
}

//static Dictionary<string, string> _set = new Dictionary<string, string>();
//static XAttribute XAttribute(string name, string value) { Console.WriteLine(name); _set.Add(name, ""); var r = !string.IsNullOrEmpty(value) ? new XAttribute(name, value) : null; return r; }
//static XAttribute XAttribute<T>(string name, T? value) where T : struct { Console.WriteLine(name); _set.Add(name, ""); var r = value != null ? new XAttribute(name, value) : null; return r; }
