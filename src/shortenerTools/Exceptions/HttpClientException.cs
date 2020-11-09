using System;
using System.Runtime.Serialization;

namespace shortenerTools.Exceptions
{
    [Serializable]
    public class HttpClientException : Exception
    {
        public string RequestUri { get; }
        public string Method { get; }
        public int StatusCode { get; }
        public string Content { get; }
        public string RequestBody { get; }

        public HttpClientException(string requestUri, string method, int statusCode, string content, string requestBody)
        {
            RequestUri = requestUri;
            Method = method;
            StatusCode = statusCode;
            Content = content;
            RequestBody = requestBody;
        }

        protected HttpClientException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}