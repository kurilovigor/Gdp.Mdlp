using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Data
{
    public enum EventStateEnum
    {
        Created = 0,
        Pending = 1,
        Processed = 2,
        Failed = 3,
        Paused = 4,
        Skipped = 5,
        Processing = 6,
        Canceled = 7
    }
}
