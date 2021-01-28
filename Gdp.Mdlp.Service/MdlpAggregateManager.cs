using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service
{
    class MdlpAggregateManager: IDisposable
    {
        Dictionary<string, MdlpManager> mdlps = new Dictionary<string, MdlpManager>();
        public Service Service { get; private set; }
        public MdlpAggregateManager(Service service)
        {
            Service = service;
            foreach (var config in service.Configuration.Mdlp.Connections.Where(x=>x.Disabled == false))
            {
                var sys = new MdlpManager(Service, service.Configuration.MdlpApi, config.AccountSystem);
                mdlps.Add(config.AccountSystem.Name, sys);
            }
        }
        public void Dispose()
        {
            foreach (var mdlp in mdlps)
                mdlp.Value.Dispose();
            mdlps.Clear();
        }
        public async Task Start()
        {
            await Task.WhenAll(mdlps.Select(x => x.Value.Start()));
        }
        public async Task Stop()
        {
            await Task.WhenAll(mdlps.Select(x=>x.Value.Stop()));
        }
    }
}
