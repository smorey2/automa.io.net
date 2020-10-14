using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using SeleniumFirefoxDriver = OpenQA.Selenium.Firefox.FirefoxDriver;

namespace Automa.IO.Drivers
{
    /// <summary>
    /// FirefoxDriver
    /// </summary>
    public class FirefoxDriver : AbstractDriver
    {
        public FirefoxDriver(Action<DriverOptions> driverOptions) : base(GetDriver(driverOptions)) { }

        static IWebDriver GetDriver(Action<DriverOptions> driverOptions)
        {
            var options = new FirefoxOptions();
            driverOptions?.Invoke(options);
            return new SeleniumFirefoxDriver(AppDomain.CurrentDomain.BaseDirectory, options);
        }
    }
}