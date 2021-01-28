using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class DocStatus
    {
        public const string UPLOADING_DOCUMENT = "UPLOADING_DOCUMENT";
        public const string PROCESSING_DOCUMENT = "PROCESSING_DOCUMENT";
        public const string CORE_PROCESSING_DOCUMENT = "CORE_PROCESSING_DOCUMENT";
        public const string CORE_PROCESSED_DOCUMENT = "CORE_PROCESSED_DOCUMENT";
        public const string PROCESSED_DOCUMENT = "PROCESSED_DOCUMENT";
        public const string FAILED = "FAILED";
        public const string FAILED_RESULT_READY = "FAILED_RESULT_READY";
    }
}
