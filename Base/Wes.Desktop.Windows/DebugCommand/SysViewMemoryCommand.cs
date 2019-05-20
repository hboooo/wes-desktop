using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Wes.Desktop.Windows.View;
using Wes.Utilities;

namespace Wes.Desktop.Windows.DebugCommand
{
    [ExportDebugCommand(DebugID = "spread-dgview", Description = "view flow memory data")]
    public class SysViewMemoryCommand : BaseDebugCommand
    {
        private string _infoFile = Path.Combine(AppPath.DataPath, "view_memory.txt");

        public override void Execute(object parameter)
        {
            try
            {
                Window window = WindowHelper.GetActivedWindow();
                if (window != null && window is WesFlowWindow)
                {
                    WesFlowWindow flowWindow = window as WesFlowWindow;
                    object viewModel = flowWindow.ActionViewModel;
                    if (viewModel != null)
                    {
                        PropertyInfo prop = viewModel.GetType().GetProperty("SelfInfo");
                        dynamic dynamicObject = prop.GetValue(viewModel, null) as dynamic;
                        string content = DynamicJson.SerializeObject(dynamicObject);

                        byte[] bytes = Encoding.Default.GetBytes(content);
                        FileStream fs = File.Create(_infoFile);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();

                        Process pro = null;
                        try
                        {
                            pro = Process.Start("notepad++.exe", _infoFile);
                        }
                        catch { }
                        if (pro == null)
                        {
                            Process.Start("notepad.exe", _infoFile);
                        }

                        Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(1000);
                            Utils.DeleteFile(_infoFile);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Warn("view memory data reset failed. message:" + ex.Message);
            }
        }
    }
}
