using Microsoft.Extensions.Configuration;

namespace Automa.IO
{
    internal class Config
    {
        public static string ProxyToken => Startup.Configuration.GetValue<string>("ProxyToken");
    }
}
