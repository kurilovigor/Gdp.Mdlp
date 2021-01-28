using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Data
{
    public enum OutcomeDocumentStatesEnum
    {
        Created = 0,
        Pending = 1,
        Downloaded = 2,
        DownloadFailed = 3,
        DownloadFatal = 4,
        Skipped = 5
    }
}
