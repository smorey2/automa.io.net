using Automa.IO.Drivers;
using Automa.IO.Unanet.Records;
using NFluent;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Automa.IO.Unanet
{
    public class UnanetClientTest
    {
        const string SourcePath = "secret";
        UnanetClient _client;

        [SetUp] public void Configure() => _client = GetUnanetClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        UnanetClient GetUnanetClient(bool deleteFile = false, bool proxy = false)
        {
            if (!Directory.Exists(SourcePath))
                Directory.CreateDirectory(SourcePath);
            var cookieFile = Path.Combine(SourcePath, "unanet.cookies.json");
            if (deleteFile && File.Exists(cookieFile))
                File.Delete(cookieFile);
            return new UnanetClient(new RoundarchUnanetOptions(), proxy ? new Config() : null)
            {
                DriverType = typeof(ChromeDriver),
                Logger = Console.WriteLine,
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = "DARWIN.UNANET.local",
            };
        }

        [Test]
        public async Task Should_export_people()
        {
            var sourcePath = SourcePath;
            var task1 = await PersonModel.ExportFileAsync(_client, sourcePath).ConfigureAwait(false);
            Check.That(task1.hasFile).IsTrue();
            var task2 = PersonModel.GetReadXml(_client, sourcePath);
            Check.That(task2).IsNotEmpty();
        }

        [Test]
        public async Task Should_export_people_proxy()
        {
            _client = GetUnanetClient(true, proxy: true);
            var sourcePath = SourcePath;
            var task1 = await PersonModel.ExportFileAsync(_client, sourcePath).ConfigureAwait(false);
            Check.That(task1.hasFile).IsTrue();
            var task2 = PersonModel.GetReadXml(_client, sourcePath);
            Check.That(task2).IsNotEmpty();
        }
    }
}
