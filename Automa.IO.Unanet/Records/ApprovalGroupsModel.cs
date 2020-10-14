using System.Threading.Tasks;

namespace Automa.IO.Unanet.Records
{
    public class ApprovalGroupsModel : ModelBase
    {
        public string approval_group_name { get; set; }
        public string approvers { get; set; }
        public string description { get; set; }
        public string delete { get; set; }
        //
        public string key { get; set; }

        public static Task<(bool success, string message, bool hasFile, object tag)> ExportFileAsync(UnanetClient una, string sourceFolder) =>
            Task.Run(() => una.GetEntitiesByExportAsync(una.Options.approval_group.key, (z, f) =>
              {
                  f.Checked["suppressOutput"] = true;
                  return null;
              }, sourceFolder));

        //public static Dictionary<string, Tuple<string, string>> GetList(UnanetClient una) =>
        //    una.GetEntitiesByList("admin/setup/people/approval_groups", null, f =>
        //    {
        //        f.Values["list"] = "true";
        //    }, entityPrefix: "k_").Single()
        //        .Where(x => !x.Key.StartsWith("d"))
        //        .ToDictionary(x => x.Value[4].Item1, x => new Tuple<string, string>(x.Key, x.Value[5].Item1));

        //public static IEnumerable<ApprovalGroupsModel> Read(UnanetClient una, string sourceFolder)
        //{
        //    var list = GetList(una);
        //    var filePath = Path.Combine(sourceFolder, una.Settings.approval_group.file);
        //    return new CsvReader().Execute(sr, x => new ApprovalGroupsModel
        //        return cr.Execute(sr, x => new ApprovalGroupsModel
        //        {
        //            approval_group_name = x[0],
        //            approvers = x[1],
        //            description = x[2],
        //            delete = x[3],
        //            //
        //            key = list.TryGetValue(x[0], out var item) ? item.Item1 : null,
        //        }).Skip(1).ToList();
        //}

        //public static IEnumerable<ApprovalGroupsModel> EnsureAndRead(UnanetClient una, string sourceFolder)
        //{
        //    var filePath = Path.Combine(sourceFolder, una.Settings.approval_group.file);
        //    if (!File.Exists(filePath))
        //        ExportFileAsync(una, sourceFolder);
        //    return Read(una, sourceFolder);
        //}

        //public static string GetReadXml(UnanetClient ctx, string sourceFolder, string syncFileA = null)
        //{
        //    var xml = new XElement("r", Read(ctx, sourceFolder).Select(x => new XElement("p", XAttribute("k", x.key),
        //        XAttribute("agn", x.approval_group_name), XAttribute("a", x.approvers), XAttribute("d", x.description)
        //    )).ToArray()).ToString();
        //    var syncFile = string.Format(syncFileA, ".p_ag.xml");
        //    if (!Directory.Exists(Path.GetDirectoryName(syncFile)))
        //        Directory.CreateDirectory(Path.GetDirectoryName(syncFile));
        //    File.WriteAllText(syncFile, xml);
        //    return xml;
        //}
    }
}