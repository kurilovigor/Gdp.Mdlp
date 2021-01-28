using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;
using Topshelf;

namespace Gdp.Mdlp.Service
{
    class Service: IDisposable
    {
        MdlpAggregateManager mdlpManager = null;
        EventManager eventManager = null;
        HostControl hostControl = null;
        WebApiManager webApi = null;
        public ServiceConfiguration Configuration { get; private set; }
        static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(Service));
        public static Service Create(string configFileName)
        {
            return new Service(Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceConfiguration>
                (
                    System.IO.File.ReadAllText(configFileName, Encoding.UTF8)
                ));
        }
        public Service(ServiceConfiguration configuration)
        {
            Configuration = configuration;
            mdlpManager = new MdlpAggregateManager(this);
            eventManager = new EventManager(this);
            webApi = new WebApiManager(this);
        }
        ~Service()
        {
            Cleanup();
        }
        public async Task Start(HostControl hostControl)
        {
            Log.Info("Starting service");
            this.hostControl = hostControl;
            await Task.WhenAll(new Task[] 
            {
                mdlpManager.Start(),
                eventManager.Start(),
                webApi.Start()
            });
        }
        public async Task Stop(HostControl hostControl)
        {
            Log.Info("Stopping service");
            await Task.WhenAll(new Task[]
            {
                mdlpManager.Stop(),
                eventManager.Stop(),
                webApi.Stop()
            });
            Cleanup();
        }
        private void Cleanup()
        {
            if (mdlpManager != null)
            {
                mdlpManager.Dispose();
                mdlpManager = null;
            }
            if (eventManager != null)
            {
                eventManager.Dispose();
                eventManager = null;
            }
            if (webApi != null)
            {
                webApi.Dispose();
                webApi = null;
            }
        }

        public void Halt()
        {
            Task.Run(new Action(() => 
            {
                Log.Fatal("Fatal error!");
                if (hostControl != null)
                    hostControl.Stop();
            }));
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
