using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Wes.Desktop.Windows;
using Wes.Print;
using Wes.Utilities;

namespace Wes.Desktop
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
            BeginInvoke(e.Exception);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            WesPrint.Engine.Dispose();
            LoggingService.Debug("Exit WES 2");
        }
        /// <summary>
        /// UI线程抛出全局异常事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            BeginInvoke(e.Exception);
        }

        /// <summary>
        /// 非UI线程抛出全局异常事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            BeginInvoke(exception);
        }

        private void BeginInvoke(Exception e)
        {
            if (System.Threading.Thread.CurrentThread == WesApp.Current.Dispatcher.Thread)
                WesModernDialog.ShowMessage(e);
            else
                WesApp.UiActionInvoke(() => WesModernDialog.ShowMessage(e));
        }
    }


}
