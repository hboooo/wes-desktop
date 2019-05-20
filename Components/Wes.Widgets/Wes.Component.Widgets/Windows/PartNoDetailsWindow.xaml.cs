using Wes.Desktop.Windows;

namespace Wes.Component.Widgets.Windows
{
    /// <summary>
    /// PartNoDetailsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PartNoDetailsWindow : BaseWindow
    {
        public PartNoDetailsWindow()
        {
            InitializeComponent();
        }

        public PartNoDetailsWindow(dynamic infoObject) : this()
        {
            this.dataGridDetails.ItemsSource = infoObject;
        }
    }

}
