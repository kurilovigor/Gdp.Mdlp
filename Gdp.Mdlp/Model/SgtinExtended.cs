using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class SgtinExtended: Sgtin
    {
        public string packing_inn { get; set; }
        public string packing_name { get; set; }
        public string packing_id { get; set; }
        public string control_inn { get; set; }
        public string control_name { get; set; }
        public string control_id { get; set; }
    }
}
