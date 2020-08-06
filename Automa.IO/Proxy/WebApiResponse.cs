using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automa.IO.Proxy
{
    class WebApiResponse
    {
        public bool ok { get; set; }
        [JsonExtensionData]
        public Dictionary<string, JsonElement> data { get; set; }
    }
}
