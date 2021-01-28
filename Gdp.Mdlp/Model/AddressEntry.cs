using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class AddressEntry
    {
        public string address_id { get; set; }
        public Address address { get; set; }
        public int? entity_type { get; set; }
    }
}
