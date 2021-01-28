using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public class BaseAddonFactory<TAddon, TKey>
        where TAddon: class
    {
        protected Dictionary<TKey, TAddon> addons = new Dictionary<TKey, TAddon>();
        public BaseAddonFactory()
        {
        }
        public void Register(ServiceConfiguration.Addon[] addons)
        {
            foreach (var addon in addons)
                Register((TKey)Convert.ChangeType(addon.Id, typeof(TKey)), addon.Type);
        }
        public void Register(TKey key, TAddon addon)
        {
            addons.Add(key, addon);
        }
        public void Register(TKey key, string typeName)
        {
            Type type = Type.GetType(typeName);
            var addon = Activator.CreateInstance(type) as TAddon;
            addons.Add(key, addon);
        }
        public TAddon this[TKey index]
        {
            get
            {
                return addons.ContainsKey(index) ? addons[index] : null;
            }
        }
        public bool Contains(TKey index)
        {
            return addons.ContainsKey(index);
        }
    }
}
