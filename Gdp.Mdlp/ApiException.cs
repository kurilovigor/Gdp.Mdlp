using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp
{
    public class ApiException: Exception
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public ApiException(string message, string errorCode, string errorDescription)
            : base($"{message} [{errorCode}]: {errorDescription}")
        {
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
        }
    }
}
