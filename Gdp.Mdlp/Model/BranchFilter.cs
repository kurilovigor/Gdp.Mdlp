using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class BranchFilter
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string branch_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string houseguid { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string federal_subject_code { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string federal_district_code { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? status { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? start_date { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? end_date { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? is_withdrawal_via_document_allowed { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? has_pharm_license { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? has_prod_license { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? has_med_license { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? has_narcotic_license { get; set; }

    }
}
