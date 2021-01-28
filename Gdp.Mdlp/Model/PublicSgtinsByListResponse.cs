using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class PublicSgtinsByListResponse: BaseEntriesResponse<PublicSgtin>
    {
        public int failed { get; set; }
        public FailedSgtin[] failed_entries { get; set; }
    }
}
