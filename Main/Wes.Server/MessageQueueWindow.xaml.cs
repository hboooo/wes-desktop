using System;
using System.Threading;
using System.Windows.Forms;
using Wes.Desktop.Windows;
using Wes.Server.Listener;
using Wes.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using Wes.Print;

namespace Wes.Server
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageQueueWindow : BaseWindow
    {
        public ObservableCollection<RequestParams> RequestItems
        {
            get { return (ObservableCollection<RequestParams>)GetValue(RequestItemsProperty); }
            set { SetValue(RequestItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RequestItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RequestItemsProperty =
            DependencyProperty.Register("RequestItems", typeof(ObservableCollection<RequestParams>), typeof(MessageQueueWindow), new PropertyMetadata(null));


        private NotifyIcon _notifyIcon = null;
        private string _port = null;

        public MessageQueueWindow(string port)
        {
            InitializeComponent();
            RequestItems = new ObservableCollection<RequestParams>();
            _port = port;
            Init();
        }

        private void Init()
        {
            this.Closing += MessageQueueWindow_Closing;

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = Properties.Resources.listener;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenu = CreateMainMenu();

            ControllerService.RequestEvent += ControllerService_RequestEvent;
            KernelHttpListener.RequestEvent += KernelHttpListener_RequestEvent;
            bool res = KernelHttpListener.Listen(_port);
            if (res)
                ShowBalloonTip("服务启动成功。");
            else
                ShowBalloonTip("服务启动失败，请确保端口未被使用。");

            DownloadLabel();
        }

        private void MessageQueueWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;
        }

        private void ControllerService_RequestEvent(object sender, RequestEventArgs e)
        {
            if (!e.IsValied)
                ShowBalloonTip(e.Message, ToolTipIcon.Warning);
            else
                ShowBalloonTip(e.Message);
        }

        private void KernelHttpListener_RequestEvent(object sender, RequestEventArgs e)
        {
            if (e.Action == RequestActionType.Add)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RequestItems.Add(e.Params);
                }));
            }
            else if (e.Action == RequestActionType.Delete)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RequestItems.Remove(e.Params);
                }));
            }
        }

        private void DownloadLabel()
        {
            Thread thread = new Thread(() =>
            {
                Wes.Print.WesPrint.Engine.UpdateTemplates((total, file) =>
                {
                    LoggingService.InfoFormat("Update label, path:{0}", file);
                }, (list) =>
                {
                    ShowBalloonTip("所有标签模板更新完成。");
                });
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private ContextMenu CreateMainMenu()
        {
            var contextMenu = new ContextMenu();

            MenuItem messageItem = new MenuItem();
            messageItem.Text = "请求队列";
            messageItem.Click += MessageItem_Click;
            contextMenu.MenuItems.Add(messageItem);

            MenuItem restart = new MenuItem();
            restart.Text = "重启";
            restart.Click += Restart_Click;
            contextMenu.MenuItems.Add(restart);

            MenuItem menuItem = new MenuItem();
            menuItem.Text = "退出";
            menuItem.Click += MenuItem_Click;
            contextMenu.MenuItems.Add(menuItem);
            return contextMenu;
        }

        private void MessageItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
            this.ShowInTaskbar = true;
            this.Visibility = Visibility.Visible;
        }

        private void Restart_Click(object sender, EventArgs e)
        {
            LoggingService.Info("正在重启http服务...");
            KernelHttpListener.Close();
            KernelHttpListener.Listen();
            LoggingService.Info("重启http服务成功...");
            ShowBalloonTip("重启成功");
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            CloseServer();
        }

        public void CloseServer()
        {
            if (_notifyIcon != null)
            {
                LoggingService.Info("关闭wes服务");
                _notifyIcon.Dispose();
                WesPrint.Engine.Dispose();
                _notifyIcon = null;
                this.Close();
                Environment.Exit(0);
            }
        }

        public void ShowBalloonTip(string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            LoggingService.Debug(message);
            switch (icon)
            {
                case ToolTipIcon.None:
                case ToolTipIcon.Info:
                    _notifyIcon.ShowBalloonTip(3, "提示信息", message, icon);
                    break;
                case ToolTipIcon.Warning:
                case ToolTipIcon.Error:
                    _notifyIcon.ShowBalloonTip(3, "错误信息", message, icon);
                    break;
                default:
                    break;
            }
        }
    }
}
