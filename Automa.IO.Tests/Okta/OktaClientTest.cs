using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Automa.IO.Okta
{
    public class OktaClientTest
    {
        const string SourcePath = "secret";
        OktaClient _client;

        [SetUp] public void Configure() => _client = GetOktaClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        OktaClient GetOktaClient(bool deleteFile = false, bool proxy = false)
        {
            if (!Directory.Exists(SourcePath))
                Directory.CreateDirectory(SourcePath);
            var accessTokenFile = Path.Combine(SourcePath, "okta.token.json");
            var cookieFile = Path.Combine(SourcePath, "okta.cookies.json");
            if (deleteFile && File.Exists(accessTokenFile))
                File.Delete(accessTokenFile);
            if (deleteFile && File.Exists(cookieFile))
                File.Delete(cookieFile);
            return new OktaClient(new Uri("https://isobar.okta.com"), "dentsuaegis", proxy ? new Config() : null)
            {
                Logger = Console.WriteLine,
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = "DARWIN.OKTA.local",
            };
        }

        [Test]
        public async Task Should_read_normally()
        {
            var app = await _client.TrySelectApplicationAsync("workday").ConfigureAwait(false);
            //var file = _client.GetReport("/d/task/1422$2059.htmld");
            //Console.WriteLine(me);
        }

        [Test]
        public async Task Should_read_getme_normally()
        {
            var app = await _client.TrySelectApplicationAsync("workday").ConfigureAwait(false);
            //var file = _client.GetReport("/d/task/1422$2059.htmld");
            //Console.WriteLine(me);
        }
    }
}
