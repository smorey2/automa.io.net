using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using System;
using SeleniumInternetExplorerDriver = OpenQA.Selenium.IE.InternetExplorerDriver;

namespace Automa.IO.Drivers
{
    /// <summary>
    /// InternetExplorerDriver
    /// </summary>
    public class InternetExplorerDriver : AbstractDriver
    {
        public InternetExplorerDriver(Action<DriverOptions> driverOptions) : base(GetDriver(driverOptions)) { }

        static IWebDriver GetDriver(Action<DriverOptions> driverOptions)
        {
            var options = new InternetExplorerOptions();
            driverOptions?.Invoke(options);
            return new SeleniumInternetExplorerDriver(AppDomain.CurrentDomain.BaseDirectory, options);
        }
    }
}