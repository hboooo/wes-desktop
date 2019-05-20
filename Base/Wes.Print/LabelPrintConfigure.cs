using System;
using System.Collections.Generic;
using System.Linq;
using Wes.Utilities;
using Wes.Utilities.Xml;

namespace Wes.Print
{
    public class LabelPrintConfigure
    {
        private static readonly string printerConfigPrefix = "Printer";
        private static readonly string workStationPrefix = "General";

        public static dynamic GetPrintConfig()
        {
            List<string> values = EnvironmetService.GetValues(printerConfigPrefix);
            if (values != null && values.Count == 1)
                return DynamicJson.DeserializeObject<Dictionary<string, dynamic>>(values[0]);
            return null;
        }

        public static Dictionary<string, int> GetPrintTimes()
        {
            var value = EnvironmetService.GetWesValue("PrintTimes");
            if (!string.IsNullOrEmpty(value))
                return DynamicJson.DeserializeObject<Dictionary<string, int>>(value);
            return null;
        }

        public static void SavePrintTimes(Dictionary<string, int> values)
        {
            EnvironmetService.AddWesValue("PrintTimes", DynamicJson.SerializeObject(values));
        }

        public static bool GetDisplayLabelImage()
        {
            List<string> values = EnvironmetService.GetValues(workStationPrefix);
            if (values != null && values.Count > 0)
                return Convert.ToBoolean(values.ElementAt(0));
            return true;
        }
    }
}
