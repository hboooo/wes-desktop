using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Wes.Utilities;

namespace Wes.Desktop.Windows.Options.View
{
    /// <summary>
    /// SettingsSerialPort4Print.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsSerialPort4Print : OptionTabControlBase, IOptionControl
    {
        public SettingsSerialPort4Print()
        {
            InitializeComponent();
            this.DataContext = new SettingsSerialPort4PrintModel();
            this.LoadOption();
        }

        protected override string ID => "SerialPort" + (this.DataContext as SettingsSerialPort4PrintModel).SelectedPrinter;

        public object Control => this;

        public void LoadOption()
        {
            LoadPropertyValue(new List<string>() { "SelectedPrinter" });
        }

        public bool SaveOption()
        {
            SettingsSerialPort4PrintModel port4PrintModel = this.DataContext as SettingsSerialPort4PrintModel;
            if (!string.IsNullOrWhiteSpace(port4PrintModel.SelectedSerialItem)
                && !string.IsNullOrWhiteSpace(port4PrintModel.SelectedDataBitsItem)
                && !string.IsNullOrWhiteSpace(port4PrintModel.SelectedBaudRateItem)
                && !string.IsNullOrWhiteSpace(port4PrintModel.SelectedParityItem)
                && !string.IsNullOrWhiteSpace(port4PrintModel.SelectedStopBitsItem))
            {
                return SavePropertyValue();
            }
            return true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                try
                {
                    SettingsSerialPort4PrintModel port4PrintModel = this.DataContext as SettingsSerialPort4PrintModel;
                    if (!string.IsNullOrWhiteSpace(port4PrintModel.SelectedSerialItem)
                        && !string.IsNullOrWhiteSpace(port4PrintModel.SelectedDataBitsItem)
                        && !string.IsNullOrWhiteSpace(port4PrintModel.SelectedBaudRateItem)
                        && !string.IsNullOrWhiteSpace(port4PrintModel.SelectedParityItem)
                        && !string.IsNullOrWhiteSpace(port4PrintModel.SelectedStopBitsItem))
                    {
                        var properties = GetPropertyValue(this.DataContext);
                        OptionConfigureService.SetProperties("SerialPort" + ((KeyValuePair<string, string>)e.RemovedItems[0]).Key, properties);
                    }
                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                }
            }

            LoadOption();
        }
    }
}
