using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WesWebBrowser
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Dependency Properties

        public string WindowTitle
        {
            get { return (string)GetValue(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WindowTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register("WindowTitle", typeof(string), typeof(MainWindow), new PropertyMetadata("WES Web"));


        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Url.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(string), typeof(MainWindow), new PropertyMetadata("http://wms.spreadlogistics.com"));
        #endregion

        #region Private Properties
        //是否最大化
        bool _isMax = false;

        public const int WM_COPYDATA = 0x004A;

        private HwndSourceHook _hook;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebBrowser();
        }

        private void InitializeWebBrowser()
        {
            this.Closed += MainWindow_Closed;
            InitializeParams();
            this.webBrowser.LifeSpanHandler = new OpenHtmlSelf();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                IntPtr handle = hwndSource.Handle;
                if (_hook != null)
                {
                    hwndSource.RemoveHook(_hook);
                }
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                IntPtr handle = hwndSource.Handle;
                _hook = new HwndSourceHook(WndProc);
                hwndSource.AddHook(_hook);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                COPYDATASTRUCT copydata = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
                string message = copydata.lpData;
                if (!string.IsNullOrEmpty(message))
                {
                    string[] mess = message.Split(' ');
                    Startup.BuildCommandLineArgs(mess);
                    InitializeParams();
                }
            }
            return hwnd;
        }

        private void InitializeParams()
        {
            var param = Startup.Parameters;
            if (param == null) return;
            foreach (var item in param)
            {
                if (string.IsNullOrEmpty(item.Value)) continue;
                if (item.Key == Commands.TitleCommand)
                    Title = item.Value;
                else if (item.Key == Commands.UrlCommand)
                    Url = item.Value;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.F11)
            {
                if (_isMax)
                {
                    this.WindowStyle = WindowStyle.ThreeDBorderWindow;
                    this.WindowState = WindowState.Normal;
                    _isMax = false;
                }
                else
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                    _isMax = true;
                }
            }
        }
    }

    public struct COPYDATASTRUCT
    {

        public IntPtr dwData;
        public int cbData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }
}
