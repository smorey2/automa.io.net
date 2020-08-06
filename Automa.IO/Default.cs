using Automa.IO.Proxy;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automa.IO
{
    public static class Default
    {
        static Default()
        {
            JsonOptions = new JsonSerializerOptions();
            JsonOptions.Converters.Add(new ValueTupleFactory());
        }

        public static readonly JsonSerializerOptions JsonOptions;

        public static IHttp Http(JsonSerializerOptions serializerOptions = null) => new Http(new HttpClient(), serializerOptions ?? JsonOptions);
    }
}
