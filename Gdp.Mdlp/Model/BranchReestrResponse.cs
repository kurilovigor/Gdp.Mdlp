using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class BranchReestrResponse: BranchEntry
    {
        public string error_code { get; set; }
        public string error_description { get; set; }
    }
}
