using System.Windows;
using System.Windows.Input;
using Wes.Core.Service;
using Wes.Desktop.Menu;
using Wes.Desktop.Windows;

namespace Wes.Desktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : BaseWindow, IMainWindow
    {
        private MainWindowViewModel _viewModel = null;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel(this);
            this.mainContent.DataContext = _viewModel;
        }

        protected override void MenuCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var contextMenu = WS.GetService<IMenu>().CreateContextMenu(e.OriginalSource as UIElement);
            if (contextMenu != null) contextMenu.IsOpen = true;
        }

    }
}
