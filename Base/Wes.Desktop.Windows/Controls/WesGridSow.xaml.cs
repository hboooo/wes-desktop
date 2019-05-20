using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace Wes.Desktop.Windows.Controls
{
    /// <summary>
    /// WesGridSow.xaml 的交互逻辑
    /// </summary>
    public partial class WesGridSow
    {
        public WesGridSow()
        {
            InitializeComponent();
        }

        int clickCount = 0; //鼠標左鍵點擊次數
        public event EventHandler Cell_DlbClick;
        private void SP_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickCount += 1;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; clickCount = 0; };

            timer.IsEnabled = true;
            if (clickCount % 2 == 0)
            {
                timer.IsEnabled = false;
                clickCount = 0;
                Cell_DlbClick?.Invoke(sender, e);
            }
        }

    }
}
