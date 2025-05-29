using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ERad5TestGUI.Helpers
{
    public class ResourceHelper
    {
        public static string GetXamlStringByKey(string key)
        {
            try
            {
                return (string)Application.Current.Resources[key];
            }
            catch (Exception)
            {
                return $"Not Found String By Key :{key}";
                //throw;
            }
        }
    }
}
