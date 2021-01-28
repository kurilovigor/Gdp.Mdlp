using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class SsccHierarchyResponse: BaseResponse
    {
        public SsccInfo[] up { get; set; }
        public SsccInfo[] down { get; set; }
        public string error_desc { get; set; }
    }
}
