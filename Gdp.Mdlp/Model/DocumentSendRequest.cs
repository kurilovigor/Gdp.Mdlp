using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class DocumentSendRequest
    {
        public string document { get; set; }
        public string sign { get; set; }
        public string request_id { get; set; }
    }
}
