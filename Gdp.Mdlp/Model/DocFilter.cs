using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class DocFilter
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string start_date { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string end_date { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string document_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string request_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? doc_type { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string doc_status { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? file_uploadtype { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string processed_date_from { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string processed_date_to { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sender_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string receiver_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string skzkm_report_id { get; set; }
        #region Fluent builder
        public static DocFilter Create()
        {
            return new DocFilter();
        }
        public DocFilter StartDate(DateTime value)
        {
            start_date = Format.ToMskDateTime(value);
            return this;
        }
        public DocFilter EndDate(DateTime value)
        {
            end_date = Format.ToMskDateTime(value);
            return this;
        }
        public DocFilter DocumentId(string value)
        {
            document_id = value;
            return this;
        }
        public DocFilter RequestId(string value)
        {
            request_id = value;
            return this;
        }
        public DocFilter DocType(int value)
        {
            doc_type = value;
            return this;
        }
        public DocFilter DocStatus(string value)
        {
            doc_status = value;
            return this;
        }
        public DocFilter FileUploadType(int value)
        {
            file_uploadtype = value;
            return this;
        }
        public DocFilter ProcessedDateFrom(DateTime value)
        {
            processed_date_from = Format.ToMskDateTime(value);
            return this;
        }
        public DocFilter ProcessedDateTo(DateTime value)
        {
            processed_date_to = Format.ToMskDateTime(value);
            return this;
        }
        public DocFilter SenderId(string value)
        {
            sender_id = value;
            return this;
        }
        public DocFilter ReceiverId(string value)
        {
            receiver_id = value;
            return this;
        }
        public DocFilter SkzkmReportId(string value)
        {
            skzkm_report_id = value;
            return this;
        }
        #endregion
    }
}
