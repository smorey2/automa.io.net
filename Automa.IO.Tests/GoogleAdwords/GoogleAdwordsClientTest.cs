using NUnit.Framework;
using System;
using System.IO;

namespace Automa.IO.GoogleAdwords
{
    public class GoogleAdwordsClientTest
    {
        const string SourcePath = "secret";
        GoogleAdwordsClient _client;

        [SetUp] public void Configure() => _client = GetGoogleAdwordsClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        GoogleAdwordsClient GetGoogleAdwordsClient(bool deleteFile = false, bool proxy = false)
        {
            if (!Directory.Exists(SourcePath))
                Directory.CreateDirectory(SourcePath);
            var cookieFile = Path.Combine(SourcePath, "googleAdwords.cookies.json");
            if (deleteFile && File.Exists(cookieFile))
                File.Delete(cookieFile);
            return new GoogleAdwordsClient(proxy ? new Config() : null)
            {
                Logger = Console.WriteLine,
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = "DARWIN.GoogleAdwords.local",
                AppId = "",
                AppSecret = "",
                DeveloperToken = "",
            };
        }

        [Test]
        public void Should_read_getme_normally()
        {
            //_client.DoAuth2Authorization();
            //_client.AccessToken = "1/1q-YwHeIpPIrl_ym2CIb05MWd0PlvdshZDs4PTq7tbywb5xAaggUlCE0E5JRHEvw";
            //_client.Test("857-327-6907");
        }
    }
}
