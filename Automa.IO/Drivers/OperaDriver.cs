using OpenQA.Selenium;
using OpenQA.Selenium.Opera;
using System;
using SeleniumOperaDriver = OpenQA.Selenium.Opera.OperaDriver;

namespace Automa.IO.Drivers
{
    /// <summary>
    /// OperaDriver
    /// </summary>
    public class OperaDriver : AbstractDriver
    {
        public OperaDriver(Action<DriverOptions> driverOptions) : base(GetDriver(driverOptions)) { }

        static IWebDriver GetDriver(Action<DriverOptions> driverOptions)
        {
            var options = new OperaOptions();
            driverOptions?.Invoke(options);
            return new SeleniumOperaDriver(AppDomain.CurrentDomain.BaseDirectory, options);
        }
    }
}