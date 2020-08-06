using System.Collections.Generic;

namespace Automa.IO.Proxy
{
    public class ErrorResponse
    {
        public class ResponseMetadata
        {
            public IList<string> Messages { get; set; } = new List<string>();
        }

        public string Error { get; set; }
        public ResponseMetadata Metadata { get; set; }
    }
}