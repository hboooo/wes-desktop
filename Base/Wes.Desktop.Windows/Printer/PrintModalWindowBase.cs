using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wes.Desktop.Windows.Controls;
using Wes.Utilities;

namespace Wes.Desktop.Windows.Printer
{
    public class PrintModalWindowBase
    {
        /// <summary>
        /// 关闭Modern Mask回调
        /// </summary>
        private Action _callback;
        /// <summary>
        /// 是否已经触发回调
        /// </summary>
        private bool _isInvoked = false;

        private UIElement _textBoxUiElement = null;

        public void ShowMaskModal(string message, Action action)
        {
            BaseWindow window = WindowHelper.GetActivedWindow() as BaseWindow;
            if (window == null) return;

            _callback = action;
            CloseInput(window);
            window.KeyDown -= Window_KeyDown;
            window.KeyDown += Window_KeyDown;
            window.MaskVisibility = System.Windows.Visibility.Visible;
            //2018-11-15 去掉打印提示自定義，統一輸出內容
            //if (!string.IsNullOrEmpty(message))
            //    window.MaskContent = message;
            WesApp.DoEvents();
        }

        public void ShowMaskModal(string message, string printLabelCount, Action action)
        {
            BaseWindow window = WindowHelper.GetActivedWindow() as BaseWindow;
            if (window == null) return;

            _callback = action;
            CloseInput(window);
            window.KeyDown -= Window_KeyDown;
            window.KeyDown += Window_KeyDown;
            window.MaskVisibility = System.Windows.Visibility.Visible;
            //if (!string.IsNullOrEmpty(message))
            //{
            //window.MaskContent = message;
            window.PrintLabelCount = printLabelCount;
            //}
            WesApp.DoEvents();
        }

        protected void UpdateLabelCount(string count)
        {
            BaseWindow window = WindowHelper.GetMaskActivedWindow() as BaseWindow;
            if (window == null) return;

            window.PrintLabelCount = count;
            WesApp.DoEvents();
        }

        protected void UpdateLabelImage(BitmapImage bitmapImage)
        {
            BaseWindow window = WindowHelper.GetMaskActivedWindow() as BaseWindow;
            if (window == null) return;

            window.LabelImageSource = bitmapImage;
            WesApp.DoEvents();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            BaseWindow window = sender as BaseWindow;
#if DEBUG
            if (e.Key == Key.Escape)
#else
            if (e.Key == Key.Escape && Keyboard.IsKeyDown(Key.U) && Keyboard.IsKeyDown(Key.L))
#endif
            {
                CloseMaskLayer(window);
            }

            if (e.Key == Key.F2)
            {
                CloseMaskLayer(window);
            }
        }

        private void CloseMaskLayer(BaseWindow window)
        {
            try
            {
                LoggingService.InfoFormat("{0} window mask Visible collapsed", window.GetType().Name);
                OpenInput(window);
                CleanWindowData(window);
                if (!_isInvoked)
                {
                    _isInvoked = true;
                    _callback?.Invoke();
                    _callback = null;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        public void HideMaskModal()
        {
            BaseWindow window = WindowHelper.GetMaskActivedWindow() as BaseWindow;
            if (window == null) return;

            OpenInput(window);
            CleanWindowData(window);
        }

        private void CleanWindowData(BaseWindow window)
        {
            window.MaskVisibility = System.Windows.Visibility.Collapsed;
            //window.MaskContent = "";
            window.LabelImageSource = null;
            Task.Factory.StartNew(() =>
            {
                Utils.DeleteFiles(AppPath.LabelTemplateImagePath);
            });
        }

        private void CloseInput(Window window)
        {
            if (_textBoxUiElement == null)
            {
                Control control = VisualTreeHelper.GetChildObject<BarCodeScanFrame>(window, null) as Control;
                if (control != null)
                    _textBoxUiElement = control.FindName("TextScan") as UIElement;
            }

            if (_textBoxUiElement != null)
            {
                if (_textBoxUiElement is AutoCompleteBox)
                {
                    LoggingService.InfoFormat("{0} window scan box intput enabled {1}", window.GetType().Name, false);
                    AutoCompleteBox autoCompleteBox = _textBoxUiElement as AutoCompleteBox;
                    autoCompleteBox.Focus();
                    autoCompleteBox.IsEnabled = false;
                    _textBoxUiElement = null;
                }
            }
        }

        private void OpenInput(Window window)
        {
            if (_textBoxUiElement == null)
            {
                Control control = VisualTreeHelper.GetChildObject<BarCodeScanFrame>(window, null) as Control;
                if (control != null)
                    _textBoxUiElement = control.FindName("TextScan") as UIElement;
            }
            if (_textBoxUiElement == null)
            {
                if (Application.Current != null && Application.Current.Windows != null)
                {
                    for (int i = 1; i < Application.Current.Windows.Count; i++)
                    {
                        Window tempWindow = Application.Current.Windows[Application.Current.Windows.Count - i];
                        if (tempWindow.GetType().Name != "AdornerLayerWindow")
                        {
                            Control control = VisualTreeHelper.GetChildObjectByTypeName(tempWindow, "BarCodeScanFrame") as Control;
                            if (control != null)
                                _textBoxUiElement = control.FindName("TextScan") as UIElement;
                            break;
                        }
                    }
                }
            }

            if (_textBoxUiElement != null)
            {
                if (_textBoxUiElement is AutoCompleteBox)
                {
                    LoggingService.InfoFormat("{0} window scan box intput enabled {1}", window.GetType().Name, true);
                    AutoCompleteBox autoCompleteBox = _textBoxUiElement as AutoCompleteBox;
                    autoCompleteBox.IsEnabled = true;
                    autoCompleteBox.Focus();
                }
            }
        }

        public void MaskCloseCallback(Action<bool> action, bool isReceived)
        {
            WesApp.Current.Dispatcher.BeginInvoke(new Action(HideMaskModal));
            if (!_isInvoked)
            {
                _isInvoked = true;
                action?.Invoke(isReceived);
            }
        }
    }
}
