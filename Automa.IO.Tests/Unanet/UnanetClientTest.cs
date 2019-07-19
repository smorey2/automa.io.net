using Automa.IO.Unanet;
using Automa.IO.Unanet.Records;
using NFluent;
using NUnit.Framework;
using System;
using System.IO;

namespace Automa.IO
{
    public class UnanetClientTest
    {
        UnanetClient _client;

        [SetUp] public void Configure() => _client = GetUnanetClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        UnanetClient GetUnanetClient()
        {
            if (!Directory.Exists("secret"))
                Directory.CreateDirectory("secret");
            var cookieFile = Path.Combine("secret", "unanet.cookies.json");
            return new UnanetClient("roundarch", false)
            {
                Logger = Console.WriteLine,
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = "DARWIN.UNANET.local",
            };
        }

        [Test]
        public void Should_export_people()
        {
            var sourcePath = "secret";
            var task1 = PersonModel.ExportFileAsync(_client, sourcePath).Result;
            Check.That(task1).IsTrue();
            var task2 = PersonModel.GetReadXml(_client, sourcePath);
            Check.That(task2).IsNotEmpty();
        }
    }
}
