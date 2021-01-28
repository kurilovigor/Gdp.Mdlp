using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class PartnersReestrResponse
    {
        public RegistrationEntry[] filtered_records { get; set; }
        public long filtered_records_count { get; set; }
        public long code { get; set; }
    }
}
