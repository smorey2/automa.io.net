using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automa.IO.Proxy
{
    class ProxyResponse
    {
        public long frame { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Data { get; set; }
    }
}
