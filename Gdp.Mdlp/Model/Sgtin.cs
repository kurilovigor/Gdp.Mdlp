using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class Sgtin
    {
        public string id { get; set; }
        public string inn { get; set; }
        public string gtin { get; set; }
        public string sgtin { get; set; }
        public string status { get; set; }
        public DateTime? status_date { get; set; }
        public string batch { get; set; }
        public string owner { get; set; }
        public int emission_type { get; set; }
        public DateTime? release_date { get; set; }
        public DateTime? emission_operation_date { get; set; }
        public string federal_subject_code { get; set; }
        public string federal_subject_name { get; set; }
        public DateTime? expiration_date { get; set; }
        public string prod_name { get; set; }
        public string sell_name { get; set; }
        public string full_prod_name { get; set; }
        public string reg_holder { get; set; }
        public string pack1_desc { get; set; }
        public string pack3_id { get; set; }
        public DateTime? last_tracing_op_date { get; set; }
        public int? source_type { get; set; }
        public string drug_code { get; set; }
        public string prod_d_name { get; set; }
        public string prod_form_name { get; set; }
        public string oms_order_id { get; set; }
        public BillingInfo billing_info { get; set; }
        public int billing_state { get; set; }
        public bool vzn_drug { get; set; }
        public bool gnvlp { get; set; }
        public string customs_point_id { get; set; }
        public DateTime? halt_doc_date { get; set; }
        public DateTime? halt_date { get; set; }
        public string halt_doc_num { get; set; }
        public string halt_id { get; set; }
        public string sys_id { get; set; }
    }
}
