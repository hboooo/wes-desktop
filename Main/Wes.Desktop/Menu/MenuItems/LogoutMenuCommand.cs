using System;
using Wes.Desktop.Windows;
using Wes.Utilities;
using Wes.Wrapper;

namespace Wes.Desktop.Menu
{
    [ExportMenuCommand(Header = "Logout", Tooltip = "Logout", Order = 0x0013)]
    public class LogoutMenuCommand : CommandWrapper
    {
        public override void Execute(object parameter)
        {
            try
            {
                WesDesktop.Instance.LogOut();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            finally
            {
                WindowHelper.CloseWindows(typeof(LoginWindow));
            }
        }
    }
}
