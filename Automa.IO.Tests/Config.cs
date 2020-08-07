using Automa.IO.Proxy;

namespace Automa.IO
{
    public class Config : IProxyOptions
    {
        public string ProxyUri => "wss://localhost:44332";
        public string Token => "TOKEN";
    }
}
