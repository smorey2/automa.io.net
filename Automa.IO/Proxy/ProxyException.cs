using System;
using System.Collections.Generic;
using System.Linq;

namespace Automa.IO.Proxy
{
    public class ProxyException : Exception
    {
        public string ErrorCode { get; }
        public IReadOnlyList<string> ErrorMessages { get; }

        public ProxyException(ErrorResponse errorResponse)
            : base(errorResponse == null
                ? "Proxy returned an unknown error response type"
                : $"Proxy returned an error response: {errorResponse.Error}.")
        {
            ErrorCode = errorResponse?.Error ?? "unknown";
            ErrorMessages = errorResponse?.Metadata?.Messages.ToList() ?? new List<string>();
        }
    }
}
