using Automa.IO.Proxy;
using Microsoft.Extensions.Configuration;

namespace Automa.IO
{
    public class Config : IProxyOptions
    {
        static Config()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static IConfiguration Configuration { get; set; }

        string IProxyOptions.ProxyUri => Configuration.GetValue<string>("ProxyUri");
        string IProxyOptions.ProxyToken => Configuration.GetValue<string>("ProxyToken");
    }
}
