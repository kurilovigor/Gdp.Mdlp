using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class DocumentsIncomeResponse: BaseResponse
    {
        public int? total { get; set; }
        public IncomeDocument[] documents { get; set; }
    }
}
