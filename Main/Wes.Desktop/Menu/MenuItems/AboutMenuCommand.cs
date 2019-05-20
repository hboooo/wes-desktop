using Wes.Desktop.Windows.View;

namespace Wes.Desktop.Menu
{
    [ExportMenuCommand(Header = "About", Tooltip = "About", Order = 0x0012)]
    public class AboutMenuCommand : CommandWrapper
    {
        public override void Execute(object parameter)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}
