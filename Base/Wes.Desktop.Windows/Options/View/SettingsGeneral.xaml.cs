using System;
using System.Collections.Generic;
using System.Reflection;
using Wes.Utilities;

namespace Wes.Desktop.Windows.Options.View
{
    /// <summary>
    /// SettingsGeneral.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsGeneral : OptionTabControlBase, IOptionControl
    {
        public SettingsGeneral()
        {
            InitializeComponent();
            this.DataContext = new SettingsGeneralViewModel(this);
            this.LoadOption();
        }

        protected override string ID => "General";

        public object Control => this;

        public void LoadOption()
        {
            LoadPropertyValue();
        }

        public bool SaveOption()
        {
            return SavePropertyValue();
        }

        protected void LoadPropertyValue()
        {
            try
            {
                Dictionary<string, object> values = OptionConfigureService.GetProperties(this.ID);
                if (values == null || values.Count == 0) return;

                List<PropertyInfo> properties = GetProperties(this.DataContext);
                foreach (var item in properties)
                {
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
