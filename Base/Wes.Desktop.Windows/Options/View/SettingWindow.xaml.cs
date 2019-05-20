using System.Windows;
using System.Windows.Controls;

namespace Wes.Desktop.Windows.Options.View
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : BaseWindow
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in tabControl.Items)
            {
                IOptionControl optionControl = (item as TabItem).Content as IOptionControl;
                if (optionControl != null)
                    if (!optionControl.SaveOption())
                        return;
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
