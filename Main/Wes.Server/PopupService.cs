using System;
using System.Windows;
using forms = System.Windows.Forms;

namespace Wes.Server
{
    public class PopupService
    {
        public static void ShowBalloonTip(string message)
        {
            ShowBalloonTip(message, forms.ToolTipIcon.Info);
        }

        public static void ShowBalloonTip(string message, forms.ToolTipIcon icon)
        {
            GetMainWindow((win) =>
            {
                if (win != null) win.ShowBalloonTip(message);
            });
        }

        public static void GetMainWindow(Action<MessageQueueWindow> action)
        {
            if (System.Threading.Thread.CurrentThread != Application.Current.Dispatcher.Thread)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageQueueWindow win = GetMessageQueueWindow();
                    action?.Invoke(win);
                }));
            }
            else
            {
                MessageQueueWindow win = GetMessageQueueWindow();
                action?.Invoke(win);
            }
        }

        private static MessageQueueWindow GetMessageQueueWindow()
        {
            var windows = Application.Current.Windows;
            if (windows != null && windows.Count > 0)
            {
                foreach (Window win in windows)
                {
                    if (win is MessageQueueWindow)
                    {
                        return win as MessageQueueWindow;
                    }
                }
            }
            return null;
        }
    }
}
