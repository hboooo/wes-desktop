using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Options.View;

namespace Wes.Desktop.Menu
{
    [ExportMenuCommand(Header = "Settings", Tooltip = "Settings",Icon = "Menu_Setting", Order = 0x0011, AppendSeparator = MenuAppendType.Front)]
    public class SettingMenuCommand : CommandWrapper
    {
        public override void Execute(object parameter)
        {
            var settingWindow = WindowHelper.GetOpenedWindow<SettingWindow>();
            if (settingWindow == null)
            {
                SettingWindow setting = new SettingWindow();
                setting.Show();
            }
            else
            {
                settingWindow.WindowState = System.Windows.WindowState.Normal;
                settingWindow.Activate();
            }
        }
    }
}
