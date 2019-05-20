using System.Collections.Generic;
using System.Reflection;

namespace Wes.Utilities
{
    public static class DynamicUtil
    {
        public static object GetValue<T>(dynamic dnc, string property)
        {
            if (dnc != null)
            {
                PropertyInfo[] pros = dnc.GetType().GetProperties();
                foreach (var propertyInfo in pros)
                {
                    var pn = propertyInfo.Name;

                    if (pn.ToUpper() == property.ToUpper())
                    {
                        var pv = propertyInfo.GetValue(dnc, null);
                        return (T)pv;
                    }
                }
            }

            return null;
        }

        public static bool IsExist(DynamicObjectClass dnc, string propName)
        {
            IEnumerable<string> pros = dnc.GetDynamicMemberNames();
            foreach (var propertyInfo in pros)
            {
                var pn = propertyInfo;
                if (pn == propName)
                {
                    return true;
                }
            }

            return false;
        }

        public static Dictionary<string, object> AppendDictionary(object target, dynamic source)
        {
            Dictionary<string, object> dicTarget = new Dictionary<string, object>();
            Dictionary<string, object> dicSource = new Dictionary<string, object>();
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (target != null)
            {
                dicTarget = DynamicJson.DeserializeObject<Dictionary<string, object>>(DynamicJson.SerializeObject(target));
            }
            if (source != null)
            {
                dicSource = DynamicJson.DeserializeObject<Dictionary<string, object>>(DynamicJson.SerializeObject(source));
            }

            foreach (var item in dicTarget)
            {
                result.Add(item.Key, item.Value);
            }

            foreach (var item in dicSource)
            {

                if (result.ContainsKey(item.Key))
                {
                    if (result[item.Key] == null || string.IsNullOrEmpty(result[item.Key].ToString())
                        || (result[item.Key].ToString() == "0" && item.Value.ToString() != "0"))
                    {
                        result[item.Key] = item.Value;
                    }
                }
                else
                {
                    result.Add(item.Key, item.Value);
                }

            }

            return result;

        }
    }
}