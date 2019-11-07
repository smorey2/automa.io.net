using System;
using System.Collections.Generic;

namespace Automa.IO.Unanet
{
    internal class Config
    {
        #region Exports

        public readonly static Dictionary<string, Tuple<string, string>> ExportsSandbox = new Dictionary<string, Tuple<string, string>>
        {
            // Lookup
            { "location master", new Tuple<string, string>("742", "DEG_ Location - Master") },
            { "labor category master", new Tuple<string, string>("743", "DEG_ Labor Category - Master") },
            { "vendor profile", new Tuple<string, string>("741", "DEG_ Vendor Profile") },

            // Organization
            { "organization", new Tuple<string, string>("733", "DEG_ Organization") },
            { "customer profile", new Tuple<string, string>("734", "DEG_ Customer Profile") },
            { "organization address", new Tuple<string, string>("735", "DEG_ Organization Address") },
            { "organization contact", new Tuple<string, string>("736", "DEG_ Organization Contact") },

            // Project
            { "project", new Tuple<string, string>("728", "DEG_ Project") },
            { "task", new Tuple<string, string>("729", "DEG_ Task") },
            { "fixed price item", new Tuple<string, string>("0", "DEG_ Fixed Price Item") },
            { "fixed price item [post]", new Tuple<string, string>("732", "DEG_ Fixed Price Item [post]") },
            { "assignment", new Tuple<string, string>("737", "DEG_ Assignment") },
            { "project administrator", new Tuple<string, string>("738", "DEG_ Project Administrators") },
            { "labor category project", new Tuple<string, string>("0", "DEG_ Labor Category - Project") },
            { "project invoice setup", new Tuple<string, string>("0", "Project Invoice Setup") },

            // Person
            { "person", new Tuple<string, string>("730", "DEG_ Person") },
            { "alternate", new Tuple<string, string>("747", "Alternate") },
            { "approval group", new Tuple<string, string>("739", "DEG_ Approval Group") },

            // Time/Invoice
            { "time", new Tuple<string, string>("731", "DEG_ Time") },
            { "invoice", new Tuple<string, string>("0", "DEG_ Invoice Export") },
        };

        public readonly static Dictionary<string, Tuple<string, string>> Exports = new Dictionary<string, Tuple<string, string>>
        {
            // Lookup
            { "location master", new Tuple<string, string>("818", "DEG_ Location - Master") },
            { "labor category master", new Tuple<string, string>("814", "DEG_ Labor Category - Master") },
            //{ "vendor profile", new Tuple<string, string>("741", "DEG_ Vendor Profile") },
            //
            { "organization", new Tuple<string, string>("734", "DEG_ Organization") },
            { "customer profile", new Tuple<string, string>("828", "Customer Profile") },
            { "organization address", new Tuple<string, string>("847", "Organization Address") },
            { "organization contact", new Tuple<string, string>("848", "Organization Contact") },

            // Project
            { "project", new Tuple<string, string>("810", "DEG_ Project") },
            { "task", new Tuple<string, string>("820", "DEG_ Task") },
            { "fixed price item", new Tuple<string, string>("795", "DEG_ Fixed Price Item") },
            { "fixed price item [post]", new Tuple<string, string>("733", "DEG_ Fixed Price Item [post]") },
            { "assignment", new Tuple<string, string>("812", "DEG_ Assignment") },
            { "project administrator", new Tuple<string, string>("857", "Project Administrators") },
            { "labor category project", new Tuple<string, string>("817", "DEG_ Labor Category - Project") },
            { "project invoice setup", new Tuple<string, string>("859", "Project Invoice Setup") },

            // Person
            { "person", new Tuple<string, string>("811", "DEG_ Person") },
            { "alternate", new Tuple<string, string>("824", "Alternate") },
            { "approval group", new Tuple<string, string>("825", "Approval Group") },

            // Time/Invoice
            { "time", new Tuple<string, string>("819", "DEG_ Time") },
            { "invoice", new Tuple<string, string>("724", "Invoice Export") },
        };

        #endregion

        #region Imports

        public readonly static Dictionary<string, string> Imports = new Dictionary<string, string>
        {
            { "credit card - generic", "GenericCreditCardImport" },
        };

        #endregion
    }
}

//static Dictionary<string, string> _set = new Dictionary<string, string>();
//static XAttribute XAttribute(string name, string value) { Console.WriteLine(name); _set.Add(name, ""); var r = !string.IsNullOrEmpty(value) ? new XAttribute(name, value) : null; return r; }
//static XAttribute XAttribute<T>(string name, T? value) where T : struct { Console.WriteLine(name); _set.Add(name, ""); var r = value != null ? new XAttribute(name, value) : null; return r; }
