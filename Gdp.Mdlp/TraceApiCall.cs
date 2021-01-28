using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp
{
    public class TraceApiCall
    {
        public Dictionary<string, string[]> Headers { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public TraceApiCall(
            string method,
            string uri,
            HttpStatusCode statusCode,
            HttpResponseHeaders headers,
            string request,
            string response
            )
        {
            Method = method;
            Uri = uri;
            StatusCode = statusCode;
            Request = request;
            Response = response;
            if (headers != null)
                Headers = headers.ToDictionary(i=>i.Key,i=>i.Value.ToArray());
            else
                Headers = new Dictionary<string, string[]>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb
                .Append(Method)
                .Append(" ")
                .AppendLine(Uri);
            if (Request != null)
                sb.AppendLine(Request);
            sb.AppendLine($"Response {(int)StatusCode} {StatusCode}");
            if (Response != null)
                sb.AppendLine(Response);
            return sb.ToString();
        }
    }
}
