using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsci
{
    public static class ExtensionMethods
    {
        public static T[] Array<T>(this SettingsBase settings, string name)
        {
            name += "_";
            var list = new List<T>();
            foreach (SettingsPropertyValue p in settings.PropertyValues)
            {
                if (p.Name.StartsWith(name)) list.Add((T)p.PropertyValue);
            }
            return list.ToArray();
        }
    }
}
