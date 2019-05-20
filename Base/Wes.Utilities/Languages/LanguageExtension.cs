using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Wes.Utilities.Languages
{
    [MarkupExtensionReturnType(typeof(BindingExpression))]
    public class LanguageExtension : MarkupExtension, INotifyPropertyChanged
    {
        [ConstructorArgument("Key")]
        public string Key
        {
            get;
            set;
        }

        private string _defaultValue;
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        private string _value;
        public string Value
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Key))
                {
                    string lang = string.Empty;
                    lang = LanguageService.GetLanguages(Key);
                    if (string.IsNullOrWhiteSpace(lang)) lang = _defaultValue;
                    return lang;
                }
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public LanguageExtension()
        {
        }

        public LanguageExtension(string key) : this()
        {
            Key = key;
            LanguageService.LanguageChanged -= WesLanguages_LanguageChanged;
            LanguageService.LanguageChanged += new EventHandler<EventArgs>(WesLanguages_LanguageChanged);
        }

        public LanguageExtension(string key, string defaultValue) : this(key)
        {
            DefaultValue = defaultValue;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            Setter setter = target.TargetObject as Setter;
            if (setter != null)
            {
                return new Binding(nameof(Value)) { Source = this, Mode = BindingMode.OneWay };
            }
            else
            {
                Binding bingding = new Binding(nameof(Value)) { Source = this, Mode = BindingMode.OneWay };
                return bingding.ProvideValue(serviceProvider);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly PropertyChangedEventArgs valueChangedEventArgs = new PropertyChangedEventArgs(nameof(Value));

        protected void NotifyValueChanged()
        {
            if (PropertyChanged != null) PropertyChanged(this, valueChangedEventArgs);
        }

        private void WesLanguages_LanguageChanged(object sender, EventArgs e)
        {
            //通知Value值已经改变，需重新获取
            NotifyValueChanged();
        }
    }
}
