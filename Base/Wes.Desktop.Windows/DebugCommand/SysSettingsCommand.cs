using System.Diagnostics;
using System.IO;
using Wes.Utilities;

namespace Wes.Desktop.Windows.DebugCommand
{
    [ExportDebugCommand(DebugID = "spread-conf", Description = "show system settings")]
    public class SysSettingsCommand : BaseDebugCommand
    {
        public override void Execute(object parameter)
        {
            string file = System.IO.Path.Combine(AppPath.DataPath, "wes.db");
            if (File.Exists(file))
            {
                Process.Start("notepad.exe", file);
            }
        }
    }
}
