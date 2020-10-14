using System;
using System.Collections.Generic;
using System.Linq;

namespace Automa.IO
{
    public class WebSocketException : Exception
    {
        public string ErrorCode { get; }
        public IReadOnlyList<string> ErrorMessages { get; }

        public WebSocketException(WebSocketErrorResponse response)
            : base(response == null
                ? "Proxy returned an unknown error response type"
                : $"Proxy returned an error response: {response.Error}.")
        {
            ErrorCode = response?.Error ?? "unknown";
            ErrorMessages = response?.Metadata?.Messages.ToList() ?? new List<string>();
        }
    }
}
