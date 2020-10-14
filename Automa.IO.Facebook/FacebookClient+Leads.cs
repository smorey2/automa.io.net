using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Automa.IO.Facebook
{
    public partial class FacebookClient
    {
        /// <summary>
        /// Downloads the lead form CSV by page.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="skipEmptyFile">The skip empty file.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public async Task<IEnumerable<string>> DownloadLeadFormCsvByPageAsync(string path, long pageId, DateTime? fromDate, DateTime? toDate, FacebookSkipEmptyFile skipEmptyFile = FacebookSkipEmptyFile.None)
        {
            _logger($"DownloadLeadFormCsvByPage: {pageId}");
            EnsureAppIdAndSecret();
            EnsureRequestedScope();
            var maps = LoadCsvPageMaps(path, pageId);
            var list = new List<string>();
            foreach (var x in await GetLeadFormsByPageAsync(pageId).ConfigureAwait(false))
            {
                var id = x["id"].ToString();
                var name = (string)x["name"];
                var url = ToBusinessUrl((string)x["leadgen_export_csv_url"]);
                _logger($"{id} - {name}");
                var file = await DownloadFacebookUrlAsync(path, url, new
                {
                    from_date = fromDate != null ? (int?)ToTimestamp(fromDate.Value) : null,
                    to_date = toDate != null ? (int?)ToTimestamp(toDate.Value) : null,
                }, null, skipEmptyFile, maps.TryGetValue(id, out var map) ? (Action<Stream, Stream>)((a, b) => CsvPageIntercept(id, map, a, b)) : null).ConfigureAwait(false);
                if (file != null)
                    list.Add(file);
            }
            return list;
        }

        static string ToBusinessUrl(string url) => url?.Replace("www.facebook.com", "business.facebook.com");

        class CsvPageMap
        {
            public string f { get; set; }
            public string t { get; set; }
        }

        IDictionary<string, CsvPageMap[]> LoadCsvPageMaps(string path, long pageId) => Directory.GetFiles(path, "*.map")
            .Select(x => new { Name = Path.GetFileNameWithoutExtension(x), Map = JsonConvert.DeserializeObject<CsvPageMap[]>(File.ReadAllText(x)) })
            .ToDictionary(x => x.Name, x => x.Map);

        void CsvPageIntercept(string mapName, CsvPageMap[] map, Stream fileStream, Stream input)
        {
            _logger($"CsvPageIntercept: {mapName}[{map.Length}]");
            var buf = new MemoryStream();
            var v = input.ReadByte();
            while (v != -1)
            {
                buf.WriteByte((byte)v);
                if (v == '\n')
                    break;
                v = input.ReadByte();
            }
            buf.Position = 0;
            var line = buf.ToArray();
            if (v == '\n')
                foreach (var m in map)
                {
                    var search = Encoding.Unicode.GetBytes(m.f);
                    var repl = Encoding.Unicode.GetBytes(m.t);
                    int fidx = 0;
                    var index = Array.FindIndex(line, 0, line.Length, (byte b) =>
                    {
                        fidx = b == search[fidx] ? fidx + 1 : 0;
                        return fidx == search.Length;
                    }) - search.Length + 1;
                    if (index > -1)
                    {
                        var newLine = new byte[line.Length - search.Length + repl.Length];
                        Buffer.BlockCopy(line, 0, newLine, 0, index);
                        Buffer.BlockCopy(repl, 0, newLine, index, repl.Length);
                        Buffer.BlockCopy(line, index + search.Length, newLine, index + repl.Length, line.Length - (index + search.Length));
                        line = newLine;
                    }
                }
            fileStream.Write(line, 0, line.Length);
        }

        async Task<IEnumerable<JToken>> GetLeadFormsByPageAsync(long pageId)
        {
            await LoadMeAccountsAsync().ConfigureAwait(false);
            var cursor = (Func<Task<JToken>>)(() => this.DownloadJson2Async(HttpMethod.Get, $"{BASEv}/{pageId}/leadgen_forms", interceptRequest: r => InterceptRequestForAccount(r, pageId)));
            var list = new List<JToken>();
            while (cursor != null)
            {
                var r = await this.TryFuncAsync(typeof(WebException), cursor).ConfigureAwait(false);
                var paging = (IDictionary<string, JToken>)r["paging"];
                cursor = paging.ContainsKey("next") ? (Func<Task<JToken>>)(() => this.DownloadJson2Async(HttpMethod.Get, (string)paging["next"], interceptRequest: r2 => InterceptRequestForAccount(r2, pageId))) : null;
                foreach (var i in (JArray)r["data"])
                    list.Add(i);
            }
            return list;
        }
    }
}
