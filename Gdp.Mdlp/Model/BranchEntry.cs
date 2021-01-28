using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class BranchEntry
    {
        public string branch_id { get; set; }
        public string federal_subject_code { get; set; }
        public string federal_district_code { get; set; }
        public string org_name { get; set; }
        public string[] work_list { get; set; }
        public Address address { get; set; }
        public int status { get; set; }
        public DateTime? suspension_date { get; set; }
        public DateTime? registration_date { get; set; }
        public bool? is_withdrawal_via_document_allowed { get; set; }
        public bool? has_pharm_license { get; set; }
        public bool? has_prod_license { get; set; }
        public bool? has_med_license { get; set; }
        public bool? has_narcotic_license { get; set; }
    }
}
