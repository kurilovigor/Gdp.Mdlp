using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public class ProcessIncomesFactory: BaseAddonFactory<IProcessIncome, string>
    {
        public ProcessIncomesFactory(ServiceConfiguration config)
            : base()
        {
            Register(config.AddonFactories.First(x=>x.Id == "ProcessIncomes").Addons);
        }
    }
}
