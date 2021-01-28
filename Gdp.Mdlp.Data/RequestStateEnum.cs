using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Data
{
    public enum RequestStateEnum
    {
        Created = 0,
        DocumentSent = 1,
        TickedDownloaded = 2,
        DocumentSendFailed = 3,
        TickedDownloadFailed = 4,
        QueryReusingRecover = 5,
        QueryReusingRecoverFailed = 6,
        TickedDownloadFatal = 7,
        QueryReusingRecoverFatal = 8,
        Canceled = 9,
        TickedDownloadPending = 10
    }
}
