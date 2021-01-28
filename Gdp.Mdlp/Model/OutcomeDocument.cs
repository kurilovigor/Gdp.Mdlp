using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class OutcomeDocument: Document
    {
        public string device_id { get; set; }
        public string skzkm_origin_msg_id { get; set; }
        public string skzkm_report_id { get; set; }
    }
}
