using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class Document
    {
        public string request_id { get; set; }
        public string document_id { get; set; }
        public string date { get; set; }
        public string processed_date { get; set; }
        public string sender { get; set; }
        public string receiver { get; set; }
        public string sys_id { get; set; }
        public string doc_type {get; set;}
        public string doc_status { get; set; }
        public int? file_uploadtype { get; set; }
        public string version { get; set; }
    }
}
