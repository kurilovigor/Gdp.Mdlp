using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Data
{
    public enum IncomeDocumentStatesEnum
    {
        Created = 0,
        Pending = 1,
        Downloaded = 2,
        DownloadFailed = 3,
        Skipped = 4,
        Processed = 5,
        ProcessFailed = 6,
        DownloadFatal = 7
    }
}
