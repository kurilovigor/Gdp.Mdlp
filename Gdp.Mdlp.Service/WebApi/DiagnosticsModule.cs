using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.WebApi
{
    public class DiagnosticsModule: NancyModule
    {
        public DiagnosticsModule(): base("/diagnostics")
        {
            Get("/all", x => 
            {
                return "Hello world";
            });
        }
    }
}
