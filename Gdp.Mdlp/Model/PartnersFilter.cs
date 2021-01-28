using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class PartnersFilter
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string system_subj_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string addressfederal_subject_code { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string federal_district_code { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string country { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string org_name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string inn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string kpp { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ogrn { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string start_date { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string end_date { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int reg_entity_type { get; set; } = 1;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string op_exec_date_start { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string op_exec_date_end { get; set; }
    }
}
