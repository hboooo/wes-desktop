using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Desktop.Windows
{
    /// <summary>
    /// Represents a Modern UI styled dialog window.
    /// </summary>
    public class WesModernDialog : ModernDialog
    {
        private Button okButton;
        private Button cancelButton;
        private Button yesButton;
        private Button noButton;
        private Button closeButton;
        private Button reportErrorButton;

        private ICommand reportErrorCommand;
        private static Exception reportError;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModernDialog"/> class.
        /// </summary>
        public WesModernDialog()
        {
            this.reportErrorCommand = new RelayCommand(o =>
            {
                if (reportError != null)
                {
                    reportErrorButton.IsEnabled = false;
                    WesApp.DoEvents();
                    try
                    {
                        string path = WindowHelper.CreateImageFile();
                        LoggingService.DebugFormat("Inform IT,Error message:{0}", reportError.Message);
                        Utilities.WXAPI.WXHelper.SendExceptionWXMessage(WesDesktop.Instance.User.Code, WesDesktop.Instance.User.UserName, reportError, path);
                    }
                    finally
                    {
                        //reportErrorButton.IsEnabled = true;
                    }
                }
            });
        }

        private Button CreateCloseDialogButton(string content, bool isDefault, bool isCancel, MessageBoxResult result)
        {
            return new Button
            {
                Content = content,
                Command = this.CloseCommand,
                CommandParameter = result,
                IsDefault = isDefault,
                IsCancel = isCancel,
                MinHeight = 21,
                MinWidth = 65,
                Margin = new Thickness(4, 0, 0, 0)
            };
        }

        private Button CreateReportErrorButton(string content)
        {
            return new Button
            {
                Content = content,
                Command = this.reportErrorCommand,
                ToolTip = "立即通知错误信息至企业微信群",
                MinHeight = 21,
                MinWidth = 65,
                Margin = new Thickness(4, 0, 0, 0)
            };
        }

        /// <summary>
        /// Gets the Ok button.
        /// </summary>
        public new Button OkButton
        {
            get
            {
                if (this.okButton == null)
                {
                    this.okButton = CreateCloseDialogButton("OkButton".GetLanguage(), true, false, MessageBoxResult.OK);
                }
                return this.okButton;
            }
        }

        /// <summary>
        /// Gets the Cancel button.
        /// </summary>
        public new Button CancelButton
        {
            get
            {
                if (this.cancelButton == null)
                {
                    this.cancelButton = CreateCloseDialogButton("CancelButton".GetLanguage(), false, true, MessageBoxResult.Cancel);
                }
                return this.cancelButton;
            }
        }

        /// <summary>
        /// Gets the Yes button.
        /// </summary>
        public new Button YesButton
        {
            get
            {
                if (this.yesButton == null)
                {
                    this.yesButton = CreateCloseDialogButton("YesButton".GetLanguage(), true, false, MessageBoxResult.Yes);
                }
                return this.yesButton;
            }
        }

        /// <summary>
        /// Gets the No button.
        /// </summary>
        public new Button NoButton
        {
            get
            {
                if (this.noButton == null)
                {
                    this.noButton = CreateCloseDialogButton("NoButton".GetLanguage(), false, true, MessageBoxResult.No);
                }
                return this.noButton;
            }
        }

        /// <summary>
        /// Gets the Close button.
        /// </summary>
        public new Button CloseButton
        {
            get
            {
                if (this.closeButton == null)
                {
                    this.closeButton = CreateCloseDialogButton("CloseButton".GetLanguage(), true, false, MessageBoxResult.None);
                }
                return this.closeButton;
            }
        }

        public Button ReportErrorButton
        {
            get
            {
                if (this.reportErrorButton == null)
                {
                    this.reportErrorButton = CreateReportErrorButton("微信通知");
                }
                return this.reportErrorButton;
            }
        }

        /// <summary>
        /// Displays a messagebox.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="title">The title.</param>
        /// <param name="button">The button.</param>
        /// <param name="owner">The window owning the messagebox. The messagebox will be located at the center of the owner.</param>
        /// <returns></returns>
        private static MessageBoxResult ShowMessage(string text, string title, MessageBoxButton button, Window owner = null, bool showReportErrorButton = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return MessageBoxResult.Cancel;
            }

            var dlg = new WesModernDialog
            {
                Title = title,
                Content = new BBCodeBlock { BBCode = text, Margin = new Thickness(0, 0, 0, 8) },
                MinHeight = 0,
                MinWidth = 320,
                MaxHeight = 480,
                MaxWidth = 640,
            };
            if (owner != null)
            {
                dlg.Owner = owner;
            }
            else
            {
                var parent = WindowHelper.GetActivedWindow();
                dlg.Owner = parent;
            }
            dlg.Buttons = GetWesButtons(dlg, button, showReportErrorButton);
            dlg.ShowDialog();
            reportError = null;
            return dlg.MessageBoxResult;
        }

        public static MessageBoxResult ShowMessage(Exception exp)
        {
            //TODO:ElasticSearch Exception中Data字段無法上傳，Exception中InnerException中Data可以上传，此处作转换处理
            WesException wesException = null;
            if (exp.InnerException is WesException)
                wesException = exp.InnerException as WesException;
            else
                wesException = new WesException(exp.Message, null, exp);

            var ex = new WesException(wesException.Message, wesException);
            reportError = ex;
            WesDesktopSounds.Failed();

            if (wesException is ScanInterceptorException)
            {
                LoggingService.Warn(ex);
                return ShowMessage(ex.Message, "Wes Interceptor", MessageBoxButton.OK, null, true);
            }
            else if (wesException is WesRestException)
            {
                LoggingService.Warn(ex);
                return ShowMessage(ex.Message, "Web API Message", MessageBoxButton.OK, null, true);
            }
            else if (wesException.InnerException != null &&
               string.Compare(wesException.InnerException.GetType().Name, "TargetInvocationException") == 0)
            {
                LoggingService.Error(ex);
                return ShowMessage("啊呀~，数据转换错误。\r\n详细信息:" + ex.Message, "System_Message".GetLanguage(), MessageBoxButton.OK, null, true);
            }
            else if (wesException is WesException)
            {
                LoggingService.Warn(ex);
                return ShowMessage(ex.Message, "WES_Message".GetLanguage(), MessageBoxButton.OK, null, true);
            }
            LoggingService.Error(ex);
            return ShowMessage(ex.Message, "System_Message".GetLanguage(), MessageBoxButton.OK);
        }

        public static MessageBoxResult ShowWesMessage(string text, string title, MessageBoxButton button, SoundType sound = SoundType.Hand, Window owner = null)
        {
            LoggingService.Info("Wes System:" + text);
            WesDesktopSounds.Play(sound);
            return ShowMessage(text, title, button, owner = null);
        }

        public static MessageBoxResult ShowWesMessage(string text)
        {
            LoggingService.Info("WES_Message:" + text);
            WesDesktopSounds.Play(SoundType.Hand);
            return ShowMessage(text, "WES_Message".GetLanguage(), MessageBoxButton.OK);
        }

        public static void ShowWesMessageAsyc(string text)
        {
            WesApp.UiActionInvoke(() =>
            {
                LoggingService.Info("WES_Message:" + text);
                WesDesktopSounds.Play(SoundType.Hand);
                ShowMessage(text, "WES_Message".GetLanguage(), MessageBoxButton.OK);
            });
        }

        public static MessageBoxResult ShowInfoMessage(string text, string title = null)
        {
            string messageTitle = string.Empty;
            if (String.IsNullOrEmpty(title))
                messageTitle = "WES_Message".GetLanguage();
            else
                messageTitle = title;
            return ShowMessage(text, messageTitle, MessageBoxButton.OK);
        }

        private static IEnumerable<Button> GetWesButtons(WesModernDialog owner, MessageBoxButton button, bool showReportErrorButton)
        {
            if (showReportErrorButton) yield return owner.ReportErrorButton;  //联系IT按钮

            if (button == MessageBoxButton.OK)
            {
                yield return owner.OkButton;
            }
            else if (button == MessageBoxButton.OKCancel)
            {
                yield return owner.OkButton;
                yield return owner.CancelButton;
            }
            else if (button == MessageBoxButton.YesNo)
            {
                yield return owner.YesButton;
                yield return owner.NoButton;
            }
            else if (button == MessageBoxButton.YesNoCancel)
            {
                yield return owner.YesButton;
                yield return owner.NoButton;
                yield return owner.CancelButton;
            }

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Utilities.WXAPI.WXHelper.ClearSendMessage();
        }
    }
}
