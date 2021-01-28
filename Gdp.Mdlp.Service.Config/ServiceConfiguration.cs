using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service
{
    /// <summary>
    /// Конфигурация службы
    /// </summary>
    public class ServiceConfiguration
    {
        public Uri MdlpApi { get; set; }
        public class EventManagerConfig
        {
            public int RefreshInterval { get; set; }
            public int EventsTile { get; set; }
            public int MinPriority { get; set; }
            public string ConnectionString { get; set; }
        }
        public class MdlpAccountConfig
        {
            public string[] Workflows { get; set; }
            public AccountSystemConfiguration AccountSystem { get; set; }
            public bool Disabled { get; set; } = false;
        }
        public class MdlpManagerConfig
        {
            public bool CacheTickets { get; set; }
            public bool CacheIncomes { get; set; }
            public int RefreshInterval { get; set; }
            public int ApiErrorRecoverInterval { get; set; }
            public int ReloginInterval { get; set; }
            public int RequestsTile { get; set; }
            public int MinPriority { get; set; }
            public MdlpAccountConfig[] Connections { get; set; }
        }
        public class Addon
        {
            public string Id { get; set; }
            public string Type { get; set; }
        }
        public class AddonFactory
        {
            public string Id { get; set; }
            public Addon[] Addons { get; set; }
        }
        public class WebApiConfig
        {
            public string Uri { get; set; } 
        }
        public WebApiConfig WebApi { get; set; }
        public int BussyQueueWaitInterval { get; set; }
        public EventManagerConfig Event { get; set; }
        public MdlpManagerConfig Mdlp { get; set; }
        public AddonFactory[] AddonFactories { get; set; }
        #region Serialization
        /// <summary>
        /// Load configuration from JSON file
        /// </summary>
        /// <param name="fileName">JSON format file name</param>
        /// <returns></returns>
        public static ServiceConfiguration Load(string fileName)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceConfiguration>
                (
                    System.IO.File.ReadAllText(fileName, Encoding.UTF8)
                );
        }
        /// <summary>
        /// Save configuration to JSON format file
        /// </summary>
        /// <param name="fileName">File name/ This wil be owerriten, if exists</param>
        public void Save(string fileName)
        {
            System.IO.File.WriteAllText(
                fileName,
                Newtonsoft.Json.JsonConvert.SerializeObject(this),
                Encoding.UTF8);
        }
        #endregion
    }
}
