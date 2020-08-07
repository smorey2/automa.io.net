using Automa.IO.Proxy;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automa.IO
{
    /// <summary>
    /// Default
    /// </summary>
    public static class Default
    {
        static Default()
        {
            JsonOptions = new JsonSerializerOptions();
            JsonOptions.Converters.Add(new CookieCollectionFactory());
            JsonOptions.Converters.Add(new NetworkCredentialFactory());
            JsonOptions.Converters.Add(new ValueTupleFactory());
        }

        /// <summary>
        /// The json options
        /// </summary>
        public static readonly JsonSerializerOptions JsonOptions;

        /// <summary>
        /// HTTPs the specified serializer options.
        /// </summary>
        /// <param name="jsonOptions">The serializer options.</param>
        /// <returns></returns>
        public static IHttp Http(JsonSerializerOptions jsonOptions = null) => new Http(new HttpClient(), jsonOptions ?? JsonOptions);

        /// <summary>
        /// Webs the socket.
        /// </summary>
        /// <param name="serializerOptions">The serializer options.</param>
        /// <returns></returns>
        public static ISocket Socket(IProxyOptions proxyOptions, JsonSerializerOptions jsonOptions = null) => new Socket(() => new ClientWebSocket(), proxyOptions, jsonOptions ?? JsonOptions);
    }
}
