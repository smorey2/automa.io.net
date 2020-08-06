using System;
using System.Text.Json;
using System.Threading.Tasks;
using Args = System.Collections.Generic.Dictionary<string, object>;

namespace Automa.IO.Proxy
{
    /// <summary>
    /// WebHost
    /// </summary>
    public class WebHost
    {
        public async Task<LoginResponse> LoginAsync(Args args)
        {
            var client = args.TryGetValue("_client", out var z) ? AutomaClient.Parse(((JsonElement)z).GetObject<Args>()) : throw new ArgumentOutOfRangeException(nameof(args));
            var tag = args.TryGetValue("tag", out z) ? ((JsonElement)z).GetTypedObject() : null;
            await client.Automa.LoginAsync(tag);
            return null;
        }

        public async Task<SelectApplicationResponse> SelectApplicationAsync(Args args)
        {
            var client = args.TryGetValue("_client", out var z) ? AutomaClient.Parse(((JsonElement)z).GetObject<Args>()) : throw new ArgumentOutOfRangeException(nameof(args));
            var application = args.TryGetValue("application", out z) ? ((JsonElement)z).GetString() : null;
            var tag = args.TryGetValue("tag", out z) ? ((JsonElement)z).GetTypedObject() : null;
            await client.Automa.SelectApplicationAsync(application, tag);
            return null;
        }

        public async Task<SetDeviceAccessTokenResponse> SetDeviceAccessTokenAsync(Args args)
        {
            var client = args.TryGetValue("_client", out var z) ? AutomaClient.Parse(((JsonElement)z).GetObject<Args>()) : throw new ArgumentOutOfRangeException(nameof(args));
            var url = args.TryGetValue("url", out z) ? z as string : null;
            var code = args.TryGetValue("code", out z) ? z as string : null;
            var tag = args.TryGetValue("tag", out z) ? ((JsonElement)z).GetTypedObject() : null;
            await client.Automa.SetDeviceAccessTokenAsync(url, code, tag);
            return null;
        }
    }
}
