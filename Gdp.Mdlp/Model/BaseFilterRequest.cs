using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class BaseFilterRequest<T> where T: class, new()
    {
        public T filter { get; set; } = new T();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? start_from { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? count { get; set; }

    }
}
