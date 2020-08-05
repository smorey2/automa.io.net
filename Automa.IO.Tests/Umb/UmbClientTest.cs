using Automa.IO.Umb.Reports;
using NFluent;
using NUnit.Framework;
using System;
using System.IO;

namespace Automa.IO.Umb
{
    public class UmbClientTest
    {
        const string SourcePath = "secret";
        UmbClient _client;

        [SetUp] public void Configure() => _client = GetUmbClient();
        [TearDown] public void TearDown() { _client?.Dispose(); _client = null; }

        UmbClient GetUmbClient(bool deleteFile = false, bool proxy = false)
        {
            if (!Directory.Exists(SourcePath))
                Directory.CreateDirectory(SourcePath);
            var cookieFile = Path.Combine(SourcePath, "umb.cookies.json");
            if (deleteFile && File.Exists(cookieFile))
                File.Delete(cookieFile);
            return new UmbClient(proxy ? new Config() : null)
            {
                Logger = Console.WriteLine,
                CookiesBytes = File.Exists(cookieFile) ? File.ReadAllBytes(cookieFile) : null,
                CookiesWriter = x => File.WriteAllBytes(cookieFile, x),
                ServiceCredential = "DARWIN.UMB.local",
            };
        }

        [Test]
        public void Should_export_transactionreport()
        {
            var sourcePath = SourcePath;
            var beginDate = DateTime.Today.AddDays(-10);
            var endDate = DateTime.Today.AddDays(-1);
            var task1 = TransactionReport.ExportFileAsync(_client, sourcePath, beginDate, endDate).Result;
            Check.That(task1).IsTrue();
            var task2 = TransactionReport.GetReadXml(_client, sourcePath);
            Check.That(task2).IsNotEmpty();
        }

        [Test]
        public void Should_export_transactionreport_proxy()
        {
            _client = GetUmbClient(true, proxy: true);
            var sourcePath = SourcePath;
            var beginDate = DateTime.Today.AddDays(-10);
            var endDate = DateTime.Today.AddDays(-1);
            var task1 = TransactionReport.ExportFileAsync(_client, sourcePath, beginDate, endDate).Result;
            Check.That(task1).IsTrue();
            var task2 = TransactionReport.GetReadXml(_client, sourcePath);
            Check.That(task2).IsNotEmpty();
        }
    }
}
