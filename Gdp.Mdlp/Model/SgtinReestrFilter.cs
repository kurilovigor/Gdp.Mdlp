using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class SgtinReestrFilter
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> status { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string gtin { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sgtin { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string batch { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sys_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? emission_operation_date_from { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? emission_operation_date_to { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? last_tracing_op_date_from { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? last_tracing_op_date_to { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string oms_order_id { get; set; }
        #region Fluent builder
        public static SgtinReestrFilter Create()
        {
            return new SgtinReestrFilter();
        }
        public SgtinReestrFilter EmissionOperationDateFrom(DateTime value)
        {
            emission_operation_date_from = value;
            return this;
        }
        public SgtinReestrFilter EmissionOperationDateTo(DateTime value)
        {
            emission_operation_date_to = value;
            return this;
        }
        public SgtinReestrFilter LastTracingOpDateFrom(DateTime value)
        {
            last_tracing_op_date_from = value;
            return this;
        }
        public SgtinReestrFilter LastTracingOpDateTo(DateTime value)
        {
            last_tracing_op_date_to = value;
            return this;
        }

        public SgtinReestrFilter Status(string[] value)
        {
            status = value.ToList();
            return this;
        }
        public SgtinReestrFilter AddStatus(string[] value)
        {
            if (status == null)
                status = value.ToList();
            else
                status.AddRange(value);
            return this;
        }
        public SgtinReestrFilter AddStatus(string value)
        {
            if (status == null)
                status = new List<string>();
            status.Add(value);
            return this;
        }
        public SgtinReestrFilter Gtin(string value)
        {
            gtin = value;
            return this;
        }
        public SgtinReestrFilter Sgtin(string value)
        {
            sgtin = value;
            return this;
        }
        public SgtinReestrFilter Batch(string value)
        {
            batch = value;
            return this;
        }
        public SgtinReestrFilter SysId(string value)
        {
            sys_id = value;
            return this;
        }
        public SgtinReestrFilter OmsOrderId(string value)
        {
            oms_order_id = value;
            return this;
        }
        #endregion
    }
}
