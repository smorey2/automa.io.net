using OpenQA.Selenium;
using OpenQA.Selenium.Safari;
using System;
using SeleniumSafariDriver = OpenQA.Selenium.Safari.SafariDriver;

namespace Automa.IO.Drivers
{
    /// <summary>
    /// SafariDriver
    /// </summary>
    public class SafariDriver : AbstractDriver
    {
        public SafariDriver(Action<DriverOptions> driverOptions) : base(GetDriver(driverOptions)) { }

        static IWebDriver GetDriver(Action<DriverOptions> driverOptions)
        {
            var options = new SafariOptions();
            driverOptions?.Invoke(options);
            return new SeleniumSafariDriver(AppDomain.CurrentDomain.BaseDirectory, options);
        }
    }
}