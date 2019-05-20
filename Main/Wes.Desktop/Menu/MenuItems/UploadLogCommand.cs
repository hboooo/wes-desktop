using System;
using System.IO;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.View;

namespace Wes.Desktop.Menu
{
    [ExportMenuCommand(Header = "UploadLog", Tooltip = "UploadLog", Icon = "Menu_Debug", Order = 0x0001, AppendSeparator = MenuAppendType.Front)]
    public class UploadLogCommand : CommandWrapper
    {
        public override void Execute(object parameter)
        {
            var uploadLogWindow = WindowHelper.GetOpenedWindow<UploadLogWindow>();
            if (uploadLogWindow == null)
            {
                UploadLogWindow uploadWindow = new UploadLogWindow();
                string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
                string fileName = Directory.GetCurrentDirectory() + "\\Data\\logs\\" + dateTime + ".log";

                if (File.Exists(fileName)) //调用File.Exists(string)方法判断是否存在
                {
                    uploadWindow.UploadFile = fileName;
                }
                uploadWindow.Show();
            }
            else
            {
                uploadLogWindow.WindowState = System.Windows.WindowState.Normal;
                uploadLogWindow.Activate();
            }

        }
    }
}
