using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Gdp.Mdlp.Service.Extensibility
{
    public class ProcessRequestFactory: BaseAddonFactory<IProcessRequest, string>
    {
        public ProcessRequestFactory(ServiceConfiguration config)
            :base()
        {
            Register(config.AddonFactories.First(x => x.Id == "ProcessRequest").Addons);
        }
    }
}
