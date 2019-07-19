using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
//using System.Net.Http;

namespace Automa.IO.Okta
{
    public partial class OktaClient : AutomaClient
    {
        //const string WorkdayUri = "https://wd3.myworkday.com/dentsuaegis";
        readonly string _oktaId;

        public OktaClient(Uri oktaUri, string oktaId)
            : base(x => new Automa(x, (ctx, driver) => new OktaAutomation(x, ctx, driver, oktaUri), driverOptions: DriverOptions)) => _oktaId = oktaId;

        static void DriverOptions(DriverOptions driverOptions)
        {
            var options = (ChromeOptions)driverOptions;
        }

        public override bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null)
        {
            switch (mode)
            {
                case AccessMode.Request: return ((string)value).IndexOf($"/authgwy/{_oktaId}/login.htmld") != -1;
            }
            return false;
        }

        public object GetReport(string url)
        {
            //var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, WorkdayUri + "/d/home.htmld"));
            //var d0 = this.TryFunc(() => this.DownloadData(HttpMethod.Get, WorkdayUri + url));
            return null;
        }
    }
}
