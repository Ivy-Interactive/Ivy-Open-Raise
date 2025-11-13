using System;
using System.Net;

namespace Ivy.Hosting.Sliplane
{
    public class SliplaneApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ErrorCode { get; }

        public SliplaneApiException(string message, HttpStatusCode statusCode, string? errorCode = null) 
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}