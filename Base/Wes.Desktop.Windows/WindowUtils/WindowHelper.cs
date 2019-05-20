using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Wes.Desktop.Windows.View;
using Wes.Utilities;
using Wes.Wrapper;

namespace Wes.Desktop.Windows
{
    public class WindowHelper
    {
        /// <summary>
        /// 获取指定类型窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flowId"></param>
        /// <returns></returns>
        public static T GetOpenedWindow<T>() where T : Window
        {
            if (Application.Current != null && Application.Current.Windows != null)
            {
                foreach (Window item in Application.Current.Windows)
                {
                    if (item is T)
                    {
                        return item as T;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定类型的窗体集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetOpenedWindows<T>() where T : Window
        {
            List<T> windows = new List<T>();
            if (Application.Current != null && Application.Current.Windows != null)
            {
                foreach (Window item in Application.Current.Windows)
                {
                    if (item is T)
                    {
                        windows.Add(item as T);
                    }
                }
            }
            return windows;
        }

        /// <summary>
        /// 获取当前活跃窗体
        /// </summary>
        /// <returns></returns>
        public static Window GetActivedWindow()
        {
            if (Application.Current != null && Application.Current.Windows != null)
            {
                for (int i = 1; i < Application.Current.Windows.Count; i++)
                {
                    Window window = Application.Current.Windows[Application.Current.Windows.Count - i];
                    if (window is BaseWindow && window.Visibility == Visibility.Visible)
                    {
                        return window;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取当前活跃窗体
        /// </summary>
        /// <returns></returns>
        public static Window GetMaskActivedWindow()
        {
            if (Application.Current != null && Application.Current.Windows != null)
            {
                for (int i = 1; i < Application.Current.Windows.Count; i++)
                {
                    Window window = Application.Current.Windows[Application.Current.Windows.Count - i];
                    if (window is BaseWindow && window.Visibility == Visibility.Visible && (window as BaseWindow).MaskVisibility == Visibility.Visible)
                    {
                        return window;
                    }
                }
            }
            return null;
        }

        public static void CloseWindows(Type windowType)
        {
            if (Application.Current != null && Application.Current.Windows != null)
            {
                foreach (Window item in Application.Current.Windows)
                {
                    if (item.GetType() == windowType) continue;
                    item.Close();
                }
            }
        }

        /// <summary>
        /// 流程窗口是否打开,标识是否正在作业
        /// </summary>
        /// <returns></returns>
        public static bool IsWorking(string message = "正在作業中,無法更換客戶,請先完成當前作業")
        {
            var windows = WindowHelper.GetOpenedWindows<WesFlowWindow>();
            if (windows != null && windows.Count > 0)
            {
                WesModernDialog.ShowWesMessage(message);
                return true;
            }
            return false;
        }

        public static string CreateImageFile()
        {
            string path = null;
            try
            {
                path = Path.Combine(AppPath.LogPath, string.Format("{0}_screenshot_{1}.png", DateTime.Now.ToString("yyyyMMdd_HHmmssfff"), WesDesktop.Instance.User.Code));
                Bitmap bitmap = new Bitmap(System.Windows.Forms.SystemInformation.VirtualScreen.Width, System.Windows.Forms.SystemInformation.VirtualScreen.Height);
                Graphics g = Graphics.FromImage(bitmap);
                g.CopyFromScreen(System.Windows.Forms.SystemInformation.VirtualScreen.X, System.Windows.Forms.SystemInformation.VirtualScreen.Y, 0, 0, System.Windows.Forms.SystemInformation.VirtualScreen.Size);
                g.Dispose();
                bitmap.Save(path);
                LoggingService.DebugFormat("Save screen shot success,file:{0}", path);
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return path;
        }
    }
}
