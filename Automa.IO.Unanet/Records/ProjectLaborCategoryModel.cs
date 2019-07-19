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
    public class ProjectLaborCategoryModel : ModelBase
    {
        public string labor_category { get; set; }
        public string project_org_code { get; set; }
        public string project_code { get; set; }
        public string delete { get; set; }
        //
        public decimal? cost_rate { get; set; }
        public decimal? bill_rate { get; set; }
        public DateTime? effective_date { get; set; }
        public string default_to_master_rate { get; set; }
        //
        public string key { get; set; }
        public string project_codeKey { get; set; }

        public static Task<bool> ExportFileAsync(UnanetClient una, string sourceFolder, string legalEntity = "75-00-DEG-00 - Digital Evolution Group, LLC") =>
            Task.Run(() => una.GetEntitiesByExport(una.Exports["labor category project"].Item1, f =>
              {
                  f.Checked["suppressOutput"] = true;
                  f.FromSelect("legalEntity", legalEntity);
                  f.FromSelect("dateRange", "BOT to EOT");
              }, sourceFolder));

        public static IEnumerable<ProjectLaborCategoryModel> Read(UnanetClient una, string sourceFolder)
        {
            var filePath = Path.Combine(sourceFolder, $"{una.Exports["labor category project"].Item2}.csv");
            using (var sr = File.OpenRead(filePath))
                return CsvReader.Read(sr, x => new ProjectLaborCategoryModel
                {
                    labor_category = x[0],
                    project_org_code = x[1],
                    project_code = x[2],
                    delete = x[3],
                    cost_rate = x[4].ToDecimal(),
                    bill_rate = x[5].ToDecimal(),
                    //
                    effective_date = x[6].ToDateTime(),
                    default_to_master_rate = x[7],
                    //
                    key = x.Count > 8 ? x[8] : null,
                }, 1).ToList();
        }

        public static string GetReadXml(UnanetClient una, string sourceFolder, string syncFileA = null)
        {
            var xml = new XElement("r", Read(una, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
                XAttribute("lc", x.labor_category), XAttribute("poc", x.project_org_code), XAttribute("pc", x.project_code),
                XAttribute("cr", x.cost_rate), XAttribute("br", x.bill_rate), XAttribute("ed", x.effective_date), XAttribute("dtmr", x.default_to_master_rate)
            )).ToArray()).ToString();
            if (syncFileA == null)
                return xml;
            var syncFile = string.Format(syncFileA, ".j_lc.xml");
            if (!Directory.Exists(Path.GetDirectoryName(syncFileA)))
                Directory.CreateDirectory(Path.GetDirectoryName(syncFileA));
            File.WriteAllText(syncFile, xml);
            return xml;
        }

        public class p_ProjectLaborCategory1 : ProjectLaborCategoryModel
        {
            public string XCF { get; set; }
        }

        public static ManageFlags ManageRecord(UnanetClient una, p_ProjectLaborCategory1 s, out string last)
        {
            if (ManageRecordBase(null, s.XCF, 1, out var cf, out var add, out last))
                return ManageFlags.ProjectLaborCategoryChanged;
            var laborCategories = Unanet.LaborCategoryLookup.LaborCategories;
            var r = una.SubmitSubManage(add ? "E" : "C", HttpMethod.Put, $"projects/labor_category",
                $"key={s.key}", $"projectkey={s.project_codeKey}", "blindInsert=false&list=true&reload=true&canEditBill=true&canEditCost=true&canViewBill=true&canViewCost=true&labor_category_dbValue=&labor_category_filterInactiveLabCat=false&showLaborCategory=false",
                out last, (z, f) =>
                {
                    if (add) f.Values["assign"] = laborCategories[s.labor_category]; // LOOKUP
                    if (!add && cf.Contains("dtmr")) f.FromSelectByKey("master", s.default_to_master_rate == "Y" ? "true" : "false");

                    // edit rate row for effective_date|exempt_status|costStructLabor|bill_rate|cost_rate
                    if (!add && (cf.Contains("ed") || cf.Contains("br") || cf.Contains("cr")))
                    {
                        //throw new InvalidOperationException("MANUAL: found multiple rate rows");
                        if (f.Values["rateSize"] != "1")
                        {
                            f.Values["rateSize"] = "1";
                            f.Remove(f.Values.Keys.Where(x =>
                            (x.StartsWith("dbrate_") && x != "dbrate_0") ||
                            (x.StartsWith("bDate_") && x != "bDate_0") ||
                            (x.StartsWith("billRate_") && x != "billRate_0") ||
                            (x.StartsWith("costRate_") && x != "costRate_0")).ToArray());
                        }
                        if (cf.Contains("ed") && s.effective_date != UnanetClient.BOT)
                            throw new InvalidOperationException($"MANUAL: {nameof(s.effective_date)} not BOT");
                        if (cf.Contains("br")) f.Values["billRate_0"] = s.bill_rate.ToString();
                        if (cf.Contains("cr")) f.Values["costRate_0"] = s.cost_rate.ToString();
                    }
                    if (add) f.Add("button_save", "action", null);
                    else f.Action = f.Action.Replace("/list", "/save");
                }, marker: add ? null : "<form name=\"simpleList\"");
            return r != null ?
                ManageFlags.ProjectLaborCategoryChanged :
                ManageFlags.None;
        }
    }
}