using Gdp.Mdlp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public class TicketTaskState: XmlTaskState
    {
        public Request Request { get; set; }
    }
}
