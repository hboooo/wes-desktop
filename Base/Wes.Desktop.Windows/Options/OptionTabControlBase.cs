using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Wes.Utilities;

namespace Wes.Desktop.Windows.Options
{
    public class OptionTabControlBase : UserControl
    {
        public const string PROPERTY_PREFIX = "Selected";

        /// <summary>
        /// option control id
        /// </summary>
        protected virtual string ID { get; }

        protected Dictionary<string, object> GetPropertyValue(object obj)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            List<PropertyInfo> properties = GetProperties(obj);
            foreach (var item in properties)
            {
                var value = item.GetValue(obj, null);
                if (value != null)
                    values[item.Name.Replace(PROPERTY_PREFIX, "")] = value;
            }
            return values;
        }

        protected List<PropertyInfo> GetProperties(object obj)
        {
            return obj.GetType().GetProperties().Where(p => p.Name.StartsWith(PROPERTY_PREFIX)).ToList<PropertyInfo>();
        }

        protected virtual bool SavePropertyValue()
        {
            try
            {
                var properties = GetPropertyValue(this.DataContext);
                return OptionConfigureService.SetProperties(this.ID, properties);
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return false;
        }

        protected virtual void LoadPropertyValue(List<string> ignoreList = null)
        {
            try
            {
                Dictionary<string, object> values = OptionConfigureService.GetProperties(this.ID);
                if (values == null) return;

                List<PropertyInfo> properties = GetProperties(this.DataContext);
                foreach (var item in properties)
                {
                    if (ignoreList != null && ignoreList.Contains(item.Name)) continue;

                    string key = item.Name.Replace(PROPERTY_PREFIX, "");
                    object temp = null;

                    if (values.TryGetValue(key, out object value))
                    {
                        string type = item.PropertyType.Name;
                        if (type == "String")
                            temp = value;
                        else if (type == "Double")
                            temp = Convert.ToDouble(value);
                        else if (type == "Boolean")
                            temp = Convert.ToBoolean(value);
                        else
                            temp = value;
                        item.SetValue(this.DataContext, temp, null);
                    }
                    else
                    {
                        item.SetValue(this.DataContext, null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

    }
}
