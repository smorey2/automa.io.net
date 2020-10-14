using System.Net;
using System.Threading.Tasks;

namespace Automa.IO
{
    /// <summary>
    /// IHasCookies
    /// </summary>
    public interface IHasCookies
    {
        CookieCollection Cookies { get; set; }
        Task CookiesFlushAsync();
    }
}
