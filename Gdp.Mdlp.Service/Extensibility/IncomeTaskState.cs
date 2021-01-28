using Gdp.Mdlp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public class IncomeTaskState: XmlTaskState
    {
        public IncomeDocument Income { get; set; }
    }
}
