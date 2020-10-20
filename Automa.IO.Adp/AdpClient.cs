using Automa.IO.Proxy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Automa.IO.Adp
{
    /// <summary>
    /// AdpClient
    /// </summary>
    public partial class AdpClient : AutomaClient
    {
        const string AdpUri = "https://workforcenow.adp.com";
        IEnumerable<Report>[] _reports;
        static readonly object ValidationReport = new object();

        public AdpClient(IProxyOptions proxyOptions = null)
            : base(client => new Automa(client, automa => new AdpAutomation(client, automa)), proxyOptions) { }

        void InterceptRequest(HttpWebRequest r) => r.Headers["X-Requested-With"] = "XMLHttpRequest";

        public override bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null) =>
            mode == AccessMode.Request && tag == ValidationReport && method == AccessMethod.TryFunc
                ? ((string)value).Contains("<h2>Enter Your User ID</h2>")
                : ((string)value).IndexOf("authorization process") != -1;

        #region Reports

        public enum ReportType
        {
            Custom = 0,
            MyReport,
            Other,
        }

        class Report
        {
            public ReportType Type { get; set; }
            public string Id { get; set; }
            public string ReportId { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public string Status { get; set; }
            public DateTime CreatedOn { get; set; }
            public string TypeAsString
            {
                get => Type.ToString();
                set
                {
                    try { Type = (ReportType)Enum.Parse(typeof(ReportType), value.Replace(" ", "")); }
                    catch { Type = ReportType.Other; }
                }
            }
        }

        public enum ReportRangeMode
        {
            AllRecords = 2,
            EffectiveAsOfAGivenDate = 5,
            EffectiveBetweenTwoDates = 9,
            EffectiveAsOfAPayrollCycle = 11,
        }

        public class ReportOptions
        {
            public static ReportOptions Standard = new ReportOptions { Mode = ReportRangeMode.EffectiveAsOfAGivenDate, Distinct = true };
            public static ReportOptions AllRecords = new ReportOptions { Mode = ReportRangeMode.AllRecords, Distinct = true };

            public ReportRangeMode Mode { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool Distinct { get; set; }
        }

        async Task<IEnumerable<Report>> GetReportsAsync(ReportType type)
        {
            if (_reports != null)
                return _reports[(int)type];
            // r0 - custom reports
            IEnumerable<Report> r0;
            {
                var d = await this.TryFuncAsync(() => this.DownloadDataAsync(
                    HttpMethod.Post,
                    $"{AdpUri}/basic/Reporting/externalCustomReportsLoad.do",
                    $"iPageSize=25&iCurrentPage=1&sOrderColumn=LAST_CHANGE&sOrderDirection=DESC&sFilterName=&sFilterSubjectArea=&sFilterFile=&sFilterType=&sFilterDescription=&bRunReportPage=&iFilterFolder=&iReportKey=&iRequestId=&iRequestKey=&sFileExtension=&sKey=&sQryType=&sReferrerMapping=&sReportMode=&sReportTitle=&sReportToCompare=&sRunId=&sScheduleReferrer=&sType=",
                    interceptRequest: InterceptRequest));
                if (string.IsNullOrEmpty(d) || d[0] != '{')
                    throw new InvalidOperationException("Result is not Json");
                var r = JObject.Parse(d);
                var gridId = (string)r["oGridData"]["identifier"];
                var gridLabel = (string)r["oGridData"]["label"];
                r0 = ((JArray)r["oGridData"]["items"]).Select(x => new Report
                {
                    Type = ReportType.Custom,
                    Id = (string)x[gridId],
                    Name = (string)x[gridLabel],
                    Title = (string)x["sTitle"],
                }).ToList();
            }
            // r1 - my reports
            IEnumerable<Report> r1;
            {
                var d = await this.TryFuncAsync(() => this.DownloadDataAsync(
                    HttpMethod.Post,
                    $"{AdpUri}/basic/Reporting/externalMyReportsReportsLoad.do",
                    $"iPageSize=25&iCurrentPage=1&sOrderColumn=CREATED&sOrderDirection=DESC&sReportType=&sFilterName=&iFilterFolder=&bRunReportPage=&iReportKey=&iRequestId=&iRequestKey=&sKey=&sQryType=&sReferrerMapping=&sReportMode=&sReportTitle=&sType=&bIsPartner=&bIsPartnerAccessMode=",
                    interceptRequest: InterceptRequest));
                if (string.IsNullOrEmpty(d) || d[0] != '{')
                    throw new InvalidOperationException("Result is not Json");
                var r = JObject.Parse(d);
                var gridId = (string)r["oGridData"]["identifier"];
                var gridLabel = (string)r["oGridData"]["label"];
                r1 = ((JArray)r["oGridData"]["items"]).Select(x => new Report
                {
                    Type = ReportType.MyReport,
                    Id = (string)x[gridId],
                    Name = (string)x[gridLabel],
                    Title = (string)x["sTitle"],
                }).ToList();
            }
            _reports = new[] { r0, r1 };
            return _reports[(int)type];
        }

        async Task<string[]> RunReportAsync(Report report, ReportOptions range, int maxLines = 40000, string folderId = "132075")
        {
            switch (report.Type)
            {
                case ReportType.MyReport:
                    {
                        var d = await this.TryFuncAsync(() => this.DownloadDataAsync(
                            HttpMethod.Post,
                            $"{AdpUri}/basic/Reporting/externalMyReportsRunNow.do",
                            $"iReportKey={report.Id}",
                            interceptRequest: InterceptRequest));
                        if (string.IsNullOrEmpty(d) || d[0] != '{')
                            throw new InvalidOperationException("Result is not Json");
                        var r = JObject.Parse(d);
                        if ((bool)r["bError"])
                            throw new InvalidOperationException((string)r["sMessage"]);
                        var v = (await GetReportOutputsAsync(folderId = null))
                            .Where(x => x.ReportId == report.Id).OrderByDescending(x => x.CreatedOn)
                            .FirstOrDefault();
                        if (v == null)
                            throw new InvalidOperationException($"Result Report \"{report.Id}\" not found in ADP.");
                        return new[] { v.Id, folderId };
                    }
                case ReportType.Custom:
                    {
                        var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(
                            HttpMethod.Post,
                            $"{AdpUri}/basic/Reporting/manageReportLoad.do",
                            $"iPageSize=25&iCurrentPage=1&sOrderColumn=LAST_CHANGE&sOrderDirection=DESC&sFilterName=&sFilterSubjectArea=&sFilterFile=&sFilterType=&sFilterDescription=&bRunReportPage=1&iFilterFolder=&iReportKey={report.Id}&iRequestId=&iRequestKey=&sFileExtension=&sKey=&sQryType=&sReferrerMapping=integratedNavigationController.do&sReportMode=&sReportTitle=&sReportToCompare=&sRunId=&sScheduleReferrer=&sType=",
                            interceptRequest: InterceptRequest));
                        var htmlForm = new HtmlFormPost(d0, new HtmlFormOptions { Marker = "<div " });
                        var htmlFormValues = htmlForm.Values;
                        var htmlFormTypes = htmlForm.Types;
                        var htmlFormChecked = htmlForm.Checked;
                        htmlFormChecked["bDistinct"] = range.Distinct;
                        htmlFormValues["sDefaultFormat"] = "CSV";
                        htmlFormValues["iMaxLines"] = maxLines.ToString();
                        htmlFormValues["sFoldersKey"] = folderId;
                        htmlFormValues["sStep7_QryTitle"] = report.Title;
                        var effectiveIds = htmlFormValues.Keys.Where(x => x.StartsWith("aEffective[")).ToList();
                        if (effectiveIds.Count == 0)
                            throw new InvalidOperationException("Unable to find \"aEffective[]\" field.");
                        foreach (var effectiveId in effectiveIds)
                            htmlFormValues[effectiveId] = ((int)range.Mode).ToString();
                        switch (range.Mode)
                        {
                            case ReportRangeMode.AllRecords:
                                {
                                    var keys = htmlFormValues.Keys.Where(x => x.StartsWith("aEffStartDate") || x.StartsWith("aStartDate") || x.StartsWith("aEffEndDate") || x.StartsWith("aEndDate")).ToList();
                                    foreach (var key in keys)
                                        htmlFormValues.Remove(key);
                                    break;
                                }
                            case ReportRangeMode.EffectiveBetweenTwoDates:
                                {
                                    var keys = htmlFormValues.Keys.Where(x => x.StartsWith("aEffStartDate") || x.StartsWith("aStartDate") || x.StartsWith("aEffEndDate") || x.StartsWith("aEndDate")).ToList();
                                    foreach (var key in keys)
                                    {
                                        if (key.StartsWith("aEffStartDate[")) htmlFormValues[key] = "0";
                                        else if (key.StartsWith("aEffStartDate")) htmlFormValues.Remove(key);
                                        else if (key.StartsWith("aStartDate")) htmlFormValues[key] = range.StartDate.ToString("yyyy-MM-dd");
                                        else if (key.StartsWith("aEffEndDate[")) htmlFormValues[key] = "0";
                                        else if (key.StartsWith("aEffEndDate")) htmlFormValues.Remove(key);
                                        else if (key.StartsWith("aEndDate")) htmlFormValues[key] = range.EndDate.ToString("yyyy-MM-dd");
                                    }
                                    break;
                                }
                            case ReportRangeMode.EffectiveAsOfAGivenDate:
                                {
                                    var keys = htmlFormValues.Keys.Where(x => x.StartsWith("aEffStartDate") || x.StartsWith("aStartDate") || x.StartsWith("aEffEndDate") || x.StartsWith("aEndDate")).ToList();
                                    foreach (var key in keys)
                                    {
                                        if (key.StartsWith("aEffStartDate[")) htmlFormValues[key] = "3";
                                        else if (key.StartsWith("aStartDate")) htmlFormValues.Remove(key);
                                        else if (key.StartsWith("aEffEndDate")) htmlFormValues.Remove(key);
                                        else if (key.StartsWith("aEndDate")) htmlFormValues.Remove(key);
                                    }
                                    break;
                                }
                            case ReportRangeMode.EffectiveAsOfAPayrollCycle: throw new NotImplementedException();
                            default: throw new InvalidOperationException($"{range.Mode} undefined");
                        }
                        //
                        var d = await this.TryFuncAsync(() => this.DownloadDataAsync(
                            HttpMethod.Post,
                            $"{AdpUri}/basic/Reporting/summaryReportRun.do",
                            htmlForm.ToString().Replace("&bRunReportPage=1", "&bRunReportPage=1&bRunReportPage=1"),
                            interceptRequest: InterceptRequest));
                        if (string.IsNullOrEmpty(d) || d[0] != '{')
                            throw new InvalidOperationException("Result is not Json");
                        var r = JObject.Parse(d);
                        if ((bool)r["message"]["bError"])
                            throw new InvalidOperationException((string)r["message"]["sMessage"]);
                        return new[] { (string)r["reqKey"], folderId };
                    }
                default: throw new InvalidOperationException();
            }
        }

        Report WaitForReportOutput(string[] id, decimal timeoutInSeconds = 10 * 60M)
        {
            Func<Report> action = () =>
            {
                do
                {
                    Console.Write(".");
                    Thread.Sleep(2000);
                    var r = GetReportOutputsAsync(id[1]).ConfigureAwait(false).GetAwaiter().GetResult().SingleOrDefault(x => x.Id == id[0]);
                    if (r == null)
                        throw new InvalidOperationException($"Result \"{r.Id}\" not found in ADP.");
                    if (r.Status == "OK" || r.Status == "ERR")
                        return r;
                }
                while (true);
            };
            if (timeoutInSeconds > 0) return action.TimeoutInvoke((int)(timeoutInSeconds * 1000M));
            else return action();
        }

        async Task DeleteReportOutputAsync(string[] id)
        {
            var d = await this.TryFuncAsync(() => this.DownloadDataAsync(
                HttpMethod.Post,
                $"{AdpUri}/basic/Reporting/viewReportsDelete.do",
                $"sToDelete=%5B%22{id[0]}%22%5D",
                interceptRequest: InterceptRequest));
            if (string.IsNullOrEmpty(d) || d[0] != '{')
                throw new InvalidOperationException("Result is not Json");
            var r = JObject.Parse(d);
        }

        async Task<IEnumerable<Report>> GetReportOutputsAsync(string folderId = "132075")
        {
            var d = await this.TryFuncAsync(() => this.DownloadDataAsync(
                HttpMethod.Post,
                $"{AdpUri}/basic/Reporting/viewReportsLoad.do",
                $"iPageSize=25&iCurrentPage=1&sOrderColumn=RUN_DATE&sOrderDirection=DESC&sFilterReportType=&sFilterCategory=&sFilterSubjectArea=&sFilterTitle=&sFilterType=&dFilterRunStart=&dFilterRunEnd=&sFilterScheduleStatus=&bRunReportPage=&iFilterFolder={folderId}&iReportKey=&iRequestId=&iRequestKey=&sFileExtension=&sViewSPIData=&sKey=&sQryType=&sReferrerMapping=&sReportMode=&sReportTitle=&sReportToCompare=&sRunId=&sScheduleReferrer=&bIsPartner=&bIsPartnerAccessMode=&sType=&sMessage=",
                interceptRequest: InterceptRequest));
            if (string.IsNullOrEmpty(d) || d[0] != '{')
                throw new InvalidOperationException("Result is not Json");
            var r = JObject.Parse(d);
            var gridId = (string)r["oGridData"]["identifier"];
            var gridLabel = (string)r["oGridData"]["label"];
            var reports = ((JArray)r["oGridData"]["items"]).Select(x => new Report
            {
                Id = (string)x[gridId],
                Name = (string)x[gridLabel],
                Status = (string)x["sStatus"],
                ReportId = (string)x["iReportKey"],
                Title = (string)x["sReportTitle"],
                TypeAsString = (string)x["sTypeLabel"],
                CreatedOn = DateTime.Parse(((string)x["sCreationDate"]).Replace(" - ", "-")),
            }).ToList();
            return reports;
        }

        async Task<string> DownloadReportOutputAsync(Report report, MemoryStream stream, string syncFolder)
        {
            if (syncFolder != null)
            {
                syncFolder = Path.GetDirectoryName(syncFolder);
                if (!Directory.Exists(syncFolder))
                    Directory.CreateDirectory(syncFolder);
            }
            //
            string url = null;
            string postData = null;
            switch (report.Type)
            {
                case ReportType.MyReport:
                    {
                        url = $"{AdpUri}/basic/Reporting/auditOutput.do";
                        postData = $"iRequestKey={report.Id}&sOutputType=XLS";
                        break;
                    }
                case ReportType.Custom:
                    {
                        async Task<HtmlFormPost> action()
                        {
                            var s = await this.TryFuncAsync(() => this.DownloadDataAsync(
                                HttpMethod.Post,
                                $"{AdpUri}/basic/Reporting/viewCsvFormatRun.viewer",
                                $"iPageSize=25&iCurrentPage=1&sOrderColumn=RUN_DATE&sOrderDirection=DESC&sFilterReportType=&sFilterCategory=&sFilterSubjectArea=&sFilterTitle=&sFilterType=&dFilterRunStart=&dFilterRunEnd=&sFilterScheduleStatus=&bRunReportPage=&iFilterFolder=&iReportKey={report.ReportId}&iRequestId={report.Id}&iRequestKey=&sFileExtension=CSV&sViewSPIData=&sKey=&sQryType=COL&sReferrerMapping=viewReports&sReportMode=&sReportTitle={HttpUtility.UrlEncode(report.Title)}&sReportToCompare=&sRunId=&sScheduleReferrer=&bIsPartner=&bIsPartnerAccessMode=&sType=&sMessage=",
                                interceptRequest: InterceptRequest));
                            var marker = "{\"url\":\"viewCsvFormatShow.do\",\"params\":";
                            var startIndex = s.IndexOf(marker);
                            if (startIndex < 0)
                                throw new InvalidOperationException("unable to find marker");
                            var endIndex = s.IndexOfSkip("}", startIndex, true);
                            s = s.Substring(startIndex, endIndex - startIndex);
                            var r = new HtmlFormPost();
                            foreach (KeyValuePair<string, JToken> pair in JObject.Parse(s))
                                r.Values.Add(pair.Key, (string)pair.Value);
                            return r;
                        }
                        HtmlFormPost htmlForm = null;
                        for (var i = 0; htmlForm == null && i < 10; i++)
                            try { htmlForm = await action(); }
                            catch (InvalidOperationException)
                            {
                                Console.Write("#");
                                Thread.Sleep(2000);
                            }
                        if (htmlForm == null)
                            throw new InvalidOperationException($"Report \"{report.Title}\" download error.");
                        //
                        url = $"{AdpUri}/basic/Reporting/viewCsvFormatShow.do";
                        postData = htmlForm.ToString();
                        break;
                    }
            }
            if (stream != null)
                return await this.TryFuncAsync(() => this.DownloadFileAsync(stream, HttpMethod.Post, url, postData, interceptRequest: InterceptRequest));
            return await this.TryFuncAsync(() => this.DownloadFileAsync(syncFolder, HttpMethod.Post, url, postData, interceptRequest: InterceptRequest));
        }

        async Task<string> GetReportAsync(Report report, ReportOptions range, MemoryStream stream, string syncFolder, bool deleteAfter = true)
        {
            var id = await RunReportAsync(report, range);
            var r = WaitForReportOutput(id);
            if (r.Status != "OK")
                throw new InvalidOperationException($"Report \"{r.Title}\" errored in ADP.");
            var file = await DownloadReportOutputAsync(r, stream, syncFolder);
            if (deleteAfter)
                await DeleteReportOutputAsync(id);
            return file;
        }

        public async Task<string> GetSingleReportAsync(ReportType type, string reportName, ReportOptions range, MemoryStream stream = null, string syncFolder = null, bool deleteAfter = true)
        {
            var r = (await GetReportsAsync(type)).SingleOrDefault(x => x.Name == reportName);
            if (r == null)
                throw new InvalidOperationException($"Report \"{reportName}\" not found.");
            return await GetReportAsync(r, range, stream, syncFolder, deleteAfter);
        }

        public async Task PreLoginAsync()
        {
            var timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var url = $"{AdpUri}/theme-services/v1/clients/O/users/A/login?now={timestamp}&impersonate=false";
            var d = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, url), tag: ValidationReport);
        }

        public async Task<IEnumerable<T>> GetValidationTablesAsync<T>(string type, string type2, Func<JToken, T> func)
        {
            var url = AdpUri + $"/mascsr/wfn/validationtables/metaservices/{type}/infinite{type2}?searchParam=&startIndex=0&chunkSize=500&showInactive=true&columnName=code?p={DateTime.Now.Ticks}";
            var d = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, url), tag: ValidationReport);
            if (string.IsNullOrEmpty(d) || d[0] != '{')
                throw new InvalidOperationException("Result is not Json");
            var r = JObject.Parse(d);
            var data = ((JArray)r["data"]).Select(func).ToList();
            return data;
        }

        #endregion
    }
}
