using System;
using System.Diagnostics;
using System.Windows;
using Wes.Core.Service;
using Wes.Desktop.Windows;
using Wes.Utilities;

namespace Wes.Desktop
{
    /// <summary>
    /// 主程序入口
    /// </summary>
    static class WesMain
    {
        /// <summary>
        /// 当前是否为调试模式
        /// </summary>
        public static bool UseDebugAttach
        {
            get
            {
#if DEBUG
                if (Debugger.IsAttached) return true;
#endif
                return false;
            }
        }

        /// <summary>
        /// 当前插件功能是否可用
        /// </summary>
        public static bool IsEnabled
        {
            get;
            set;
        }

        static App app;

        [STAThread()]
        public static void Main(string[] args)
        {
            try
            {
                Process proc = Utils.GetProcess(WesApp.WES_DESKTOP);
                Process curr = Process.GetCurrentProcess();
                if (proc.Id == curr.Id)
                {
                    bool run = false;
                    System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Wes.Desktop", out run);
                    if (run)
                    {
                        mutex.ReleaseMutex();
                        Run();
                    }
                }
                else
                {
                    WindowsAPI.SwitchToThisWindow(proc.MainWindowHandle, true);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    WesModernDialog.ShowMessage(ex);
                }
                catch (Exception loadError)
                {
                    MessageBox.Show(loadError.ToString(), "Critical error (Logging service defect?)");
                }
            }
        }

        /// <summary>
        /// 启动WES
        /// </summary>
        static void Run()
        {
            using (DebugTimer.Time("Start WES2.0"))
            {
                app = new App();
                WS.GetService<IStartup>().Run();
            }
            app.Run(new LoginWindow());
        }

    }
}
