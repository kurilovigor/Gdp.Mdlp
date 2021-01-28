using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp
{
    public class ApiCallException: Exception
    {
        public TraceApiCall Trace { get; set; }
        public ApiCallException(TraceApiCall trace)
            : base(trace.ToString())
        {
            Trace = trace;
        }
    }
}
