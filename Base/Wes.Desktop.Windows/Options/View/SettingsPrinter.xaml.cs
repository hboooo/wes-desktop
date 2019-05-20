using System.Windows.Input;
using Wes.Wrapper;

namespace Wes.Desktop.Windows.Options.View
{
    /// <summary>
    /// SettingsPrinter.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPrinter : OptionTabControlBase, IOptionControl
    {
        public SettingsPrinter()
        {
            InitializeComponent();
            this.DataContext = new SettingsPrinterViewModel();
            this.LoadOption();
        }

        protected override string ID => "Printer" + WesDesktop.Instance.AddIn.EndCustomer;

        public object Control => this;

        public void LoadOption()
        {
            LoadPropertyValue();
        }

        public bool SaveOption()
        {
            if (!string.IsNullOrEmpty((this.DataContext as SettingsPrinterViewModel).CurrentConsigneeItem))
            {
                return SavePropertyValue();
            }
            return true;
        }
        
        private void DataGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            SettingsPrinterViewModel viewModel = this.DataContext as SettingsPrinterViewModel;
            viewModel.UpdatePrinterConfig();
        }
        
    }
}
