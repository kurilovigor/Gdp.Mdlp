using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class OperationDate
    {
        [JsonProperty("$date")]
        public DateTime date { get; set; }
    }
}
