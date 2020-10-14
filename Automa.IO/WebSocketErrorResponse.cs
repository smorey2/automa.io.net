using System;
using System.Collections.Generic;

namespace Automa.IO
{
    public class WebSocketErrorResponse
    {
        public class ResponseMetadata
        {
            public IList<string> Messages { get; set; } = new List<string>();
        }

        public WebSocketErrorResponse() { }
        public WebSocketErrorResponse(Exception e)
        {
            Error = e.Message;
            Metadata = new ResponseMetadata { Messages = { e.StackTrace } };
        }

        public string Error { get; set; }
        public ResponseMetadata Metadata { get; set; }
    }
}