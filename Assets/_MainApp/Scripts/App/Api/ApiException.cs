using System;

namespace PushPelmesh.App.Api
{
    public class ApiException : Exception
    {
        public long StatusCode { get; }

        public string ResponseBody { get; }

        public ApiException(
            long statusCode,
            string responseBody,
            string message)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}