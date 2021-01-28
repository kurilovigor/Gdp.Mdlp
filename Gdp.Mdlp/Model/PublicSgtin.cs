using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class PublicSgtin
    {
        public string sgtin { get; set; }
        public string batch { get; set; }
        public DateTime? expiration_date { get; set; }
        public string prod_name { get; set; }
        public string sell_name { get; set; }
        public string prod_d_name { get; set; }
        public string prod_form_name { get; set; }
        public DateTime? reg_date { get; set; }
        public string reg_number { get; set; }
        public string drug_code { get; set; }
        public string reg_holder { get; set; }
        public string state { get; set; }
        public int emission_type { get; set; }
        public string branch_id { get; set; }
        public string sscc { get; set; }
    }
}
