using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class DocumentsRequestResponse: BaseResponse
    {
        public Document[] documents { get; set; }
        public long? total { get; set; }
    }
}
