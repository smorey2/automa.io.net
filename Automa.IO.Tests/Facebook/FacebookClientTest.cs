using Automa.IO.Facebook;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Facebook
{
    public class FacebookClientTest
    {
        const string SourcePath = "secret";
        FacebookClient _client;

        [SetUp] public void Configure() => _client = GetFacebookClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        FacebookClient GetFacebookClient(bool deleteFile = false, bool proxy = false)
        {
            if (!Directory.Exists(SourcePath))
                Directory.CreateDirectory(SourcePath);
            var accessTokenFile = Path.Combine(SourcePath, "facebook.token.json");
            var cookieFile = Path.Combine(SourcePath, "facebook.cookies.json");
            if (deleteFile && File.Exists(accessTokenFile))
                File.Delete(accessTokenFile);
            if (deleteFile && File.Exists(cookieFile))
                File.Delete(cookieFile);
            return new FacebookClient(proxy ? new Config() : null)
            {
                Logger = Console.WriteLine,
                AccessToken = File.Exists(accessTokenFile) ? File.ReadAllText(accessTokenFile) : null,
                AccessTokenWriter = x => File.WriteAllText(accessTokenFile, x),
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = "DEGSVC.Facebook.local",
                RequestedScope = "manage_pages,ads_management",
                AppId = "",
                AppSecret = "",
                ClientToken = "",
            };
        }

        [Test]
        public async Task Should_read_getme_normally()
        {
            var me = await _client.GetMeAsync().ConfigureAwait(false);
            Console.WriteLine(me);
        }

        [Test]
        public void Should_rebind()
        {
            var automa = _client.Automa;
            automa.Dispose();
        }

        [Test]
        public async Task Should_pull_csv_from_page()
        {
            var pageId = 573293769395845L;
            var files = (await _client.DownloadLeadFormCsvByPageAsync("secret", pageId, DateTime.Now.AddDays(-5), null, FacebookSkipEmptyFile.TextHasSecondLine).ConfigureAwait(false)).ToList();
            Console.WriteLine(files.Count);
        }

        [Test]
        public void Should_should_create_customaudience()
        {
            //var accountId = 789921621154239;
            //_client.CreateCustomAudience(accountId);
        }

        //[Test]
        //public void shoulrShould_read_normally()
        //{
        //    // given
        //    // when
        //    var token = _ctx.SetDeviceAccessToken("", (a, b, c, d) => { });
        //    Console.WriteLine(me);
        //    // then
        //}

        //[Test]
        //public void Should_read_normally()
        //{
        //    // given
        //    // when
        //    var token = _ctx.SetDeviceAccessToken("", (a, b, c, d) => { });
        //    Console.WriteLine(me);
        //    // then
        //}
    }
}
