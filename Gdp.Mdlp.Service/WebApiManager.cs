using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;

namespace Gdp.Mdlp.Service
{
    class WebApiManager: IDisposable
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(WebApiManager));
        NancyHost host = null;
        public Service Service { get; private set; }
        public WebApiManager(Service service)
        {
            Service = service;
        }
        public void Dispose()
        {
            if (host != null)
            {
                host.Dispose();
                host = null;
            }
        }

        public async Task Start()
        {
            if (host == null)
            {
                var hostConfigs = new HostConfiguration();
                var uri = new Uri(Service.Configuration.WebApi.Uri);
                host = new NancyHost(hostConfigs, uri);
                host.Start();
                log.Info($"WebApi started at {uri}");
            }
        }
        public async Task Stop()
        {
            if (host != null)
            {
                host.Stop();
                host.Dispose();
                host = null;
            }
        }
    }
}
