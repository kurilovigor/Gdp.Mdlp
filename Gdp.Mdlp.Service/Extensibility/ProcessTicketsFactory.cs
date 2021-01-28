using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public class ProcessTicketFactory: BaseAddonFactory<IProcessTicket, string>
    {
        public ProcessTicketFactory(ServiceConfiguration config)
            : base()
        {
            Register(config.AddonFactories.First(x => x.Id == "ProcessTicket").Addons);
        }
    }
}
