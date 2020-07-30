using Automa.IO.Unanet;
using Automa.IO.Unanet.Records;
using NFluent;
using NUnit.Framework;
using System;
using System.IO;

namespace Automa.IO
{
    public class AutomaTest
    {
        const string SourcePath = "secret";
        UnanetClient _client;

        [SetUp] public void Configure() => _client = GetUnanetClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        UnanetClient GetUnanetClient(bool deleteFile = false, int credentialType = 0)
        {
            if (!Directory.Exists(SourcePath))
                Directory.CreateDirectory(SourcePath);
            var cookieFile = Path.Combine(SourcePath, "unanet.cookies.json");
            if (deleteFile && File.Exists(cookieFile))
                File.Delete(cookieFile);
            return new UnanetClient(new RoundarchUnanetSetting())
            {
                Logger = Console.WriteLine,
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = credentialType == 0 ? "DARWIN.UNANET.local" : null,
                ConnectionString = credentialType == 1 ? "Credential=DARWIN.UNANET.local" : null,
            };
        }

        [Test]
        public void Should_connect_with_ServiceCredential()
        {
            _client = GetUnanetClient(true, 0);
            var sourcePath = SourcePath;
            var task1 = PersonModel.ExportFileAsync(_client, sourcePath).Result;
            Check.That(task1.hasFile).IsTrue();
        }

        [Test]
        public void Should_connect_with_ConnectionString()
        {
            _client = GetUnanetClient(true, 1);
            var sourcePath = SourcePath;
            var task1 = PersonModel.ExportFileAsync(_client, sourcePath).Result;
            Check.That(task1.hasFile).IsTrue();
        }
    }
}
