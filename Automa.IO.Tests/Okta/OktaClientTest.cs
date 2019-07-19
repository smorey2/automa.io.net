using Automa.IO.Okta;
using NUnit.Framework;
using System;
using System.IO;

namespace Automa.IO
{
    public class OktaClientTest
    {
        OktaClient _client;

        [SetUp] public void Configure() => _client = GetOktaClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        OktaClient GetOktaClient()
        {
            if (!Directory.Exists("secret"))
                Directory.CreateDirectory("secret");
            var accessTokenFile = Path.Combine("secret", "okta.token.json");
            var cookieFile = Path.Combine("secret", "okta.cookies.json");
            return new OktaClient(new Uri("https://isobar.okta.com"), "dentsuaegis")
            {
                Logger = Console.WriteLine,
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = "DARWIN.OKTA.local",
            };
        }

        [Test]
        public void Should_read_normally()
        {
            var app = _client.TrySelectApplication("workday");
            //var file = _client.GetReport("/d/task/1422$2059.htmld");
            //Console.WriteLine(me);
        }

        [Test]
        public void Should_read_getme_normally()
        {
            var app = _client.TrySelectApplication("workday");
            //var file = _client.GetReport("/d/task/1422$2059.htmld");
            //Console.WriteLine(me);
        }
    }
}
