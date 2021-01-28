using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gdp.Mdlp
{
    public static class Cache
    {
        static string _CacheFolder = string.Empty;
        public static string CacheFolder
        {
            get
            {
                if (_CacheFolder == string.Empty)
                    lock (typeof(Cache))
                        if (_CacheFolder == string.Empty)
                        {
                            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                            UriBuilder uri = new UriBuilder(codeBase);
                            string path = Uri.UnescapeDataString(uri.Path);
                            _CacheFolder = Path.Combine(Path.GetDirectoryName(path), "cache");
                            if (!Directory.Exists(_CacheFolder))
                                Directory.CreateDirectory(_CacheFolder);
                        }
                return _CacheFolder;
            }
        }
        public static void Save(string fileName, object data)
        {
            File.WriteAllText(Path.Combine(CacheFolder, fileName), JsonConvert.SerializeObject(data));
        }
        public static void Save(string fileName, string text)
        {
            File.WriteAllText(Path.Combine(CacheFolder, fileName), text);
        }
        public static T Load<T>(string fileName)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(Path.Combine(CacheFolder, fileName)));
        }
        public static bool Exists(string fileName)
        {
            return File.Exists(Path.Combine(CacheFolder, fileName));
        }
    }
}
