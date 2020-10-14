using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using SeleniumChromeDriver = OpenQA.Selenium.Chrome.ChromeDriver;

namespace Automa.IO.Drivers
{
    /// <summary>
    /// ChromeDriver
    /// </summary>
    public class ChromeDriver : AbstractDriver
    {
        public ChromeDriver(Action<DriverOptions> driverOptions) : base(GetDriver(driverOptions)) { }

        static IWebDriver GetDriver(Action<DriverOptions> driverOptions)
        {
            var options = new ChromeOptions();
            options.AddUserProfilePreference("profile.managed_default_content_settings.notifications", 1);
            driverOptions?.Invoke(options);
            return new SeleniumChromeDriver(AppDomain.CurrentDomain.BaseDirectory, options);
        }
    }
}