using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace dsci
{
    public static class DsConfig
    {
        public static string[] ContentDirectories
        {
            get
            {
                var key = Properties.Settings.Default.DsRegistryKey;
                var value_prefix = Properties.Settings.Default.DsContentDirValuePrefix;
                var dir_max = Properties.Settings.Default.DsContentDirMax;
                var list = new List<string>(dir_max);
                for (int i = 0; i < dir_max; i++)
                {
                    var s = Registry.GetValue(key, value_prefix + i, null);
                    if (s == null) break;
                    list.Add((string)s);
                }
                return list.ToArray();
            }
        }
    }
}
