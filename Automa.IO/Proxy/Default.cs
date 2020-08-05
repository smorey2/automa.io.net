using System.Net.Http;
using System.Text.Json;

namespace Automa.IO.Proxy
{
    public static class Default
    {
        public static IHttp Http(JsonSerializerOptions serializerOptions = null) => new Http(new HttpClient(), serializerOptions ?? SerializerOptions);

        static JsonSerializerOptions SerializerOptions => new JsonSerializerOptions { };
    }
}
