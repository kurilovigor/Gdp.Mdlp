using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Console
{
    public static class Tests
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Tests));
        public static Connection CreateConnection()
        {
            var config = Gdp.Mdlp.Service.ServiceConfiguration.Load("config.test.json");
            return new Connection(config.MdlpApi, config.Mdlp.Connections.First().AccountSystem);
        }
        public static async Task Execute()
        {
            using (var api = CreateConnection())
            {
                try
                {
                    Log.Debug("Login ...");
                    await api.LoginAsync(false);

                    foreach (var c in api.CryptographyManager.GetValidCertificates())
                        Log.Info($"{c.SubjectName.Name} {c.Thumbprint}");
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                finally
                {
                    Log.Debug("Logout ...");
                    await api.LogoutAsync();
                }
            }
        }
    }
}
