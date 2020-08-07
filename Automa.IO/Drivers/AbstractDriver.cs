using OpenQA.Selenium;
using System;
using System.Net;
using NetCookie = System.Net.Cookie;
using SelCookie = OpenQA.Selenium.Cookie;

namespace Automa.IO.Drivers
{
    class InternalEmptyDriver : AbstractDriver
    {
        public InternalEmptyDriver() : base(null) { }
    }

    /// <summary>
    /// AbstractDriver
    /// </summary>
    public abstract class AbstractDriver : IDisposable
    {
        public static readonly AbstractDriver EmptyDriver = new InternalEmptyDriver();
        public readonly IWebDriver Driver;
        bool _disposed;

        public AbstractDriver(IWebDriver webDriver, int implicitWaitInMinutes = 5)
        {
            Driver = webDriver;
            if (Driver != null)
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(implicitWaitInMinutes);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) { }
                if (Driver != null)
                {
                    Driver.Quit();
                    Driver.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Automa" /> class.
        /// </summary>
        ~AbstractDriver() { Dispose(false); }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        static string CookieValueEncode(string value) => value.Replace(",", "%2C");
        static string CookieValueDecode(string value) => value.Replace("%2C", ",");

        /// <summary>
        /// Gets the cookies.
        /// </summary>
        /// <value>The cookies.</value>
        /// <exception cref="ArgumentNullException">value</exception>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public virtual CookieCollection Cookies
        {
            get
            {
                var cookies = new CookieCollection();
                foreach (var x in Driver.Manage().Cookies.AllCookies)
                    cookies.Add(new NetCookie(x.Name, CookieValueEncode(x.Value), x.Path, x.Domain) { Expires = x.Expiry != null ? x.Expiry.Value : DateTime.MinValue, HttpOnly = x.IsHttpOnly, Secure = x.Secure });
                return cookies;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                var cookies = Driver.Manage().Cookies;
                cookies.DeleteAllCookies();
                foreach (NetCookie x in value)
                    cookies.AddCookie(new SelCookie(x.Name, CookieValueDecode(x.Value), x.Domain, x.Path, x.Expires != DateTime.MinValue ? (DateTime?)x.Expires : null));
            }
        }
    }
}