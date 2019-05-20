using System.Windows;

namespace Wes.Desktop.Menu
{
    [ExportMenuCommand(Header = "Exit", Tooltip = "Exit", Icon = "Menu_Exit", Order = 0x0014)]
    public class ExitMenuCommand : CommandWrapper
    {
        public override void Execute(object parameter)
        {
            Application.Current.Shutdown(0);
        }
    }
}
