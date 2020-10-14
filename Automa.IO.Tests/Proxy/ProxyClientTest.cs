using Automa.IO.Drivers;
using Automa.IO.Facebook;
using Automa.IO.GoogleAdwords;
using Automa.IO.Okta;
using Automa.IO.Umb;
using Automa.IO.Unanet;
using NFluent;
using NUnit.Framework;
using System;
using System.Text.Json;
using Args = System.Collections.Generic.Dictionary<string, object>;

namespace Automa.IO.Proxy
{
    public class ProxyClientTest
    {
        [Test]
        public void Should_rebuild_FacebookClient()
        {
            var client = new FacebookClient()
            {
                ServiceCredential = "DEGSVC.Facebook.local",
                RequestedScope = "manage_pages,ads_management",
                AppId = "",
                AppSecret = "",
                ClientToken = "",
            };
            var orgArgs = client.GetClientArgs();
            var args = JsonSerializer.Deserialize<Args>(JsonSerializer.Serialize(orgArgs, Default.JsonOptions));
            Check.That(args.Count > 0).IsTrue();
            var newClient = AutomaClient.Parse(args);
            Check.That(newClient).IsNotNull();
        }

        [Test]
        public void Should_rebuild_GoogleAdwordsClient()
        {
            var client = new GoogleAdwordsClient()
            {
                ServiceCredential = "DARWIN.GoogleAdwords.local",
                AppId = "",
                AppSecret = "",
                DeveloperToken = "",
            };
            var orgArgs = client.GetClientArgs();
            var args = JsonSerializer.Deserialize<Args>(JsonSerializer.Serialize(orgArgs, Default.JsonOptions));
            Check.That(args.Count > 0).IsTrue();
            var newClient = AutomaClient.Parse(args);
            Check.That(newClient).IsNotNull();
        }

        [Test]
        public void Should_rebuild_OktaClient()
        {
            var client = new OktaClient(new Uri("https://isobar.okta.com"), "dentsuaegis")
            {
                ServiceCredential = "DARWIN.OKTA.local",
            };
            var orgArgs = client.GetClientArgs();
            var args = JsonSerializer.Deserialize<Args>(JsonSerializer.Serialize(orgArgs, Default.JsonOptions));
            Check.That(args.Count > 0).IsTrue();
            var newClient = AutomaClient.Parse(args);
            Check.That(newClient).IsNotNull();
        }

        [Test]
        public void Should_rebuild_UmbClient()
        {
            var client = new UmbClient()
            {
                DriverType = typeof(EdgeDriver),
                ServiceCredential = "DARWIN.UMB.local",
            };
            var orgArgs = client.GetClientArgs();
            var args = JsonSerializer.Deserialize<Args>(JsonSerializer.Serialize(orgArgs, Default.JsonOptions));
            Check.That(args.Count > 0).IsTrue();
            var newClient = AutomaClient.Parse(args);
            Check.That(newClient).IsNotNull();
        }

        [Test]
        public void Should_rebuild_UnanetClient()
        {
            var client = new UnanetClient(new RoundarchUnanetOptions())
            {
                DriverType = typeof(ChromeDriver),
                ServiceCredential = "DARWIN.UNANET.local",
            };
            var orgArgs = client.GetClientArgs();
            var args = JsonSerializer.Deserialize<Args>(JsonSerializer.Serialize(orgArgs, Default.JsonOptions));
            Check.That(args.Count > 0).IsTrue();
            var newClient = AutomaClient.Parse(args);
            Check.That(newClient).IsNotNull();
        }
    }
}
