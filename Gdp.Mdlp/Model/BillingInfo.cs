using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class BillingInfo
    {
        public bool is_prepaid { get; set; }
        public bool free_code { get; set; }
        public bool is_paid { get; set; }
        public bool contains_vzn { get; set; }
        public Payment[] payments { get; set; }
    }
}
