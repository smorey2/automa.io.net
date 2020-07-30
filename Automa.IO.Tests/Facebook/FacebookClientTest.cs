using Automa.IO.Facebook;
using NUnit.Framework;
using System;
using System.Linq;
using System.IO;

namespace Automa.IO
{
    public class FacebookClientTest
    {
        const string SourcePath = "secret";
        FacebookClient _client;

        [SetUp] public void Configure() => _client = GetFacebookClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        FacebookClient GetFacebookClient(bool deleteFile = false)
        {
            if (!Directory.Exists(SourcePath))
                Directory.CreateDirectory(SourcePath);
            var accessTokenFile = Path.Combine(SourcePath, "facebook.token.json");
            var cookieFile = Path.Combine(SourcePath, "facebook.cookies.json");
            if (deleteFile && File.Exists(accessTokenFile))
                File.Delete(accessTokenFile);
            if (deleteFile && File.Exists(cookieFile))
                File.Delete(cookieFile);
            return new FacebookClient
            {
                Logger = Console.WriteLine,
                AccessToken = File.Exists(accessTokenFile) ? File.ReadAllText(accessTokenFile) : null,
                AccessTokenWriter = x => File.WriteAllText(accessTokenFile, x),
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = "DEGSVC.Facebook.local",
                AppId = "",
                AppSecret = "",
                ClientToken = "",
                RequestedScope = "manage_pages,ads_management",
            };
        }

        [Test]
        public void Should_read_getme_normally()
        {
            var me = _client.GetMe();
            Console.WriteLine(me);
        }

        [Test]
        public void Should_rebind()
        {
            var automa = _client.Automa;
            automa.Dispose();
        }

        [Test]
        public void Should_pull_csv_from_page()
        {
            var pageId = 573293769395845L;
            var files = _client.DownloadLeadFormCsvByPage("secret", pageId, DateTime.Now.AddDays(-5), null, FacebookSkipEmptyFile.TextHasSecondLine)
                .ToArray();
            Console.WriteLine(files.Length);
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
