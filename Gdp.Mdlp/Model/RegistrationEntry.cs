using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class RegistrationEntry
    {
        public string system_subj_id { get; set; }
        public ResolvedFiasAddress[] branches { get; set; }
        public ResolvedFiasAddress[] safe_warehouses { get; set; }
        public string inn { get; set; }
        public string KPP { get; set; }
        public string ORG_NAME { get; set; }
        public string OGRN { get; set; }
        public string FIRST_NAME { get; set; }
        public string MIDDLE_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public int entity_type { get; set; }
        public OperationDate op_date { get; set; }
        public DateTime op_exec_date { get; set; }
        public string country_code { get; set; }
        public string federal_subject_code { get; set; }
        public List<ChiefInfo> chiefs { get; set; }
        public bool state_gov_supplier { get; set; }
    }
}
