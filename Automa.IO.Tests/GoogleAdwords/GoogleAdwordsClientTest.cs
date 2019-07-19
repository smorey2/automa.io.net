using Automa.IO.GoogleAdwords;
using NUnit.Framework;
using System;
using System.IO;

namespace Automa.IO
{
    public class GoogleAdwordsClientTest
    {
        GoogleAdwordsClient _client;

        [SetUp] public void Configure() => _client = GetGoogleAdwordsClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        GoogleAdwordsClient GetGoogleAdwordsClient()
        {
            if (!Directory.Exists("secret"))
                Directory.CreateDirectory("secret");
            var cookieFile = Path.Combine("secret", "googleAdwords.cookies.json");
            return new GoogleAdwordsClient
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
