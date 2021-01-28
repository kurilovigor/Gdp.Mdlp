using Gdp.Mdlp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public class ProcessEventsFactory: BaseAddonFactory<IProcessEvent, string>
    {
        public ProcessEventsFactory(ServiceConfiguration config)
            : base()
        {
            Register(config.AddonFactories.First(x => x.Id == "ProcessEvents").Addons);
        }
    }
}
