using System.IO;
using Wes.Utilities;
using Wes.Wrapper;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace Wes.Desktop.Windows.DebugCommand
{
    [ExportDebugCommand(DebugID = "spread-info", Description = "show system infomation")]
    public class SysInfoCommand : BaseDebugCommand
    {
        private string _infoFile = Path.Combine(AppPath.DataPath, "info.txt");

        public override void Execute(object parameter)
        {
            if (WesDesktop.Instance.Authority.IsDelete != 1)
            {
                LoggingService.InfoFormat("無權限,當前用戶{0}不能查看系統信息,Version:{1}",
                    WesDesktop.Instance.User.Code, WesDesktop.Instance.Version);
                return;
            }

            byte[] bytes = Encoding.Default.GetBytes(WesApp.SystemInfomation);
            FileStream fs = File.Create(_infoFile);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
            Process.Start("notepad.exe", _infoFile);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                Utils.DeleteFile(_infoFile);
            });
        }
    }
}
