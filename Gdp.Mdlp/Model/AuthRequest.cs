using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class AuthRequest
    {
        public string client_secret { get; set; }
        public string client_id { get; set; }
        public string user_id { get; set; }
        public string auth_type { get; set; }
    }
}
