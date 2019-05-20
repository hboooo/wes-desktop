using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Wes.Desktop.Windows;
using Wes.Print;
using Wes.Server.Listener;
using Wes.Utilities;

namespace Wes.Server
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += App_OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            WesPrint.Engine.Dispose();
            KernelHttpListener.Close();
            LoggingService.Info($"Close listening and exit.");
        }
        /// <summary>
        /// UI线程抛出全局异常事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            PopupService.ShowBalloonTip(e.Exception.Message);
        }

        /// <summary>
        /// 非UI线程抛出全局异常事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            PopupService.ShowBalloonTip((e.ExceptionObject as Exception).Message);
        }

    }

}
