using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class ResolvedFiasAddress
    {
        public string id { get; set; }
        public AddressFias address_fias { get; set; }
        public AddressResolved address_resolved { get; set; }
        public int status { get; set; }
    }
}
