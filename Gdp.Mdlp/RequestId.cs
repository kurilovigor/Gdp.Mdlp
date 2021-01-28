using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp
{
    public static class RequestId
    {
        public static string New()
        {
            return Guid.NewGuid().ToString("D");
        }
    }
}
