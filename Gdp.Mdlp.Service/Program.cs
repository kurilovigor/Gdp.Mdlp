using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Gdp.Mdlp.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x=>
            {
                x.UseLog4Net("log.config");
                x.Service<Service>(s=> 
                {
                    s.ConstructUsing(name => Service.Create(Properties.Settings.Default.ConfigName));
                    s.WhenStarted((tc, hc) => { tc.Start(hc).Wait(); return true; });
                    s.WhenStopped((tc, hc)=> { tc.Stop(hc).Wait(); return true; });
                });
                x.RunAsLocalSystem();
                x.SetDescription(@"Интеграция с MDLP");
                x.SetDisplayName(@"GDP MDPL Service");
                x.SetServiceName(@"gdpmdlpsvc");
            });

        }
    }
}
