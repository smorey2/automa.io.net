using System.Net;

namespace Automa.IO
{
    /// <summary>
    /// IHasCookies
    /// </summary>
    public interface IHasCookies
    {
        CookieCollection Cookies { get; set; }
        void CookiesFlush();
    }
}
