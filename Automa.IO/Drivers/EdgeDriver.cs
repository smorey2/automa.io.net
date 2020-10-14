using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System;
using SeleniumEdgeDriver = OpenQA.Selenium.Edge.EdgeDriver;

namespace Automa.IO.Drivers
{
    /// <summary>
    /// EdgeDriver
    /// </summary>
    public class EdgeDriver : AbstractDriver
    {
        public EdgeDriver(Action<DriverOptions> driverOptions) : base(GetDriver(driverOptions)) { }

        static IWebDriver GetDriver(Action<DriverOptions> driverOptions)
        {
            var options = new EdgeOptions();
            driverOptions?.Invoke(options);
            return new SeleniumEdgeDriver(AppDomain.CurrentDomain.BaseDirectory, options);
        }
    }
}