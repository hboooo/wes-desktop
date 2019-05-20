using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FirstFloor.ModernUI.Presentation;
using Wes.Core.Service;
using Wes.Desktop.Menu;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Controls;
using Wes.Desktop.Windows.View;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Extends;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Desktop
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {
        #region Dependency Properties

        private ICommand _scanCommand;
        private ICommand _leftMenuCommand;

        private Visibility _progressVisibility;
        private string _progressText;
        private string _versionInfo;
        private string _barcodeTooltip;
        private string _scanValue;
        private string _outputMessage;
        private double _leftMenuWidth = 90;

        public Visibility ProgressVisibility
        {
            get { return _progressVisibility; }
            set
            {
                if (_progressVisibility != value)
                {
                    _progressVisibility = value;
                    OnPropertyChanged(nameof(ProgressVisibility));
                }
            }
        }

        public string ProgressText
        {
            get { return _progressText; }
            set
            {
                if (_progressText != value)
                {
                    _progressText = value;
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }


        public string VersionInfo
        {
            get { return _versionInfo; }
            set
            {
                if (_versionInfo != value)
                {
                    _versionInfo = value;
                    OnPropertyChanged(nameof(VersionInfo));
                }
            }
        }


        public string BarcodeTooltip
        {
            get { return _barcodeTooltip; }
            set
            {
                if (_barcodeTooltip != value)
                {
                    _barcodeTooltip = value;
                    OnPropertyChanged(nameof(BarcodeTooltip));
                }
            }
        }

        public string ScanValue
        {
            get { return _scanValue; }
            set
            {
                if (_scanValue != value)
                {
                    _scanValue = value;
                    OnPropertyChanged(nameof(ScanValue));
                }
            }
        }

        public string OutputMessage
        {
            get { return _outputMessage; }
            set
            {
                if (_outputMessage != value)
                {
                    _outputMessage = value;
                    OnPropertyChanged(nameof(OutputMessage));
                }
            }
        }

        public double LeftMenuWidth
        {
            get { return _leftMenuWidth; }
            set
            {
                if (_leftMenuWidth != value)
                {
                    _leftMenuWidth = value;
                    OnPropertyChanged(nameof(LeftMenuWidth));
                }
            }
        }

        #endregion

        #region Private Properties
        private MainWindow _mainWindow = null;
        private WrapPanel _navigatorPanel = null;
        private BarCodeScanFrame _barCodeScanFrame = null;

        private DoubleAnimation _sliderMenuAnimation;
        private bool _sliderState = true;
        #endregion

        /// <summary>
        /// 掃描命令
        /// </summary>
        public ICommand ScanCommand
        {
            get
            {
                if (_scanCommand == null)
                {
                    _scanCommand = new WindowRelayCommand<KeyEventArgs>((kea) =>
                    {
                        if (kea.Key != Key.Enter) return;
                        if (kea.Source is Control)
                        {
                            Type type = kea.Source.GetType();
                            MethodInfo methodInfo = type.GetMethod("SelectAll");
                            if (methodInfo != null) methodInfo.Invoke(kea.Source, null);

                            if (!string.IsNullOrWhiteSpace(ScanValue))
                            {
                                WesApp.CurrentWorkNo = ScanValue;
                                var workNos = QrCodeFilterUtils.ResolverQrCode(ScanValue);
                                this.Scan(workNos);
                            }
                        }
                    });
                }

                return _scanCommand;
            }
        }

        public ICommand LeftMenuCommand
        {
            get
            {
                if (_leftMenuCommand == null)
                {
                    _leftMenuCommand = new WindowRelayCommand<KeyEventArgs>((kea) =>
                    {
                        ShowSliderMenu();
                    });
                }

                return _leftMenuCommand;
            }
        }


        public MainWindowViewModel(MainWindow mainWindow)
        {
            this._mainWindow = mainWindow;
            this._navigatorPanel = _mainWindow.navigatorWrapPanel;
            this._barCodeScanFrame = _mainWindow.barCodeFrame;
            WesDesktop.Instance.PropertyChanged = (mode) =>
            {
                if (mode == 1) UpdateViewDisplay();
            };
            InializeWindow();
        }

        public void UpdateViewDisplay()
        {
            BaseWindowViewModel viewMode = this._mainWindow.DataContext as BaseWindowViewModel;
            if (viewMode != null)
            {
                string userCode = WesDesktop.Instance.User.Code;
                string endCustomerName = WesDesktop.Instance.AddIn.EndCustomerName;
                string endCustomer = WesDesktop.Instance.AddIn.EndCustomer;
                if (!string.IsNullOrEmpty(userCode)) viewMode.UserID = userCode;
                if (!string.IsNullOrEmpty(endCustomerName)) viewMode.EndCustomerName = endCustomerName;
                if (!string.IsNullOrEmpty(endCustomer)) viewMode.EndCustomerDetails = $"客户名称：{endCustomerName}，编码：{endCustomer}";
            }
        }

        private void InializeWindow()
        {
            LoadFlowMudule.Instance.LoadFlowMuduleProgress += Instance_LoadFlowMuduleProgress;
            this._mainWindow.Loaded += MainWindow_Loaded;
            this._mainWindow.Closing += MainWindow_Closing;
            this._mainWindow.txtVersion.MouseDown += TxtVersion_MouseDown;
            WS.GetService<IMenu>().AddinChangedCallback = AddinChangedAction;
            InitializeHook();
            InitializeAnimation();
        }

        private void InitializeAnimation()
        {
            _sliderMenuAnimation = new DoubleAnimation();
            _sliderMenuAnimation.BeginTime = TimeSpan.FromSeconds(0);
            _sliderMenuAnimation.FillBehavior = FillBehavior.HoldEnd;
            _sliderMenuAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.25));
        }

        public void ShowSliderMenu()
        {
            if (_sliderState)
            {
                _sliderState = false;
                _sliderMenuAnimation.From = 0;
                _sliderMenuAnimation.To = -LeftMenuWidth;
            }
            else
            {
                _sliderState = true;
                _sliderMenuAnimation.From = -LeftMenuWidth;
                _sliderMenuAnimation.To = 0;
            }
            _mainWindow.gridTranslateTransform.BeginAnimation(TranslateTransform.XProperty, _sliderMenuAnimation);
        }

        private void TxtVersion_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this._mainWindow.Loaded -= MainWindow_Loaded;
            VersionInfo = "MainVersion".GetLanguage() + DesktopVersion.GetVersionNo();
            CreateLeftNavigators();
            InitNavigator(() =>
            {
                WesMain.IsEnabled = false;
                this.UpdateNavButtonEnable();
            });
            this._barCodeScanFrame.Focus();
        }

        private void InitializeHook()
        {
            WindowKeyboardHook.OnKeyUp -= WindowKeyboardHook_OnKeyUp;
            WindowKeyboardHook.OnKeyUp += WindowKeyboardHook_OnKeyUp;
            WindowKeyboardHook.AddHook();
        }

        private void WindowKeyboardHook_OnKeyUp(object sender, KeyPressEventArgs e)
        {
            if (e.Key == Key.PrintScreen)
            {
                LoggingService.DebugFormat("Press key {0} down", e.Key);
                string path = WindowHelper.CreateImageFile();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    var uploadLogWindow = WindowHelper.GetOpenedWindow<UploadLogWindow>();
                    if (uploadLogWindow == null)
                    {
                        UploadLogWindow uploadWindow = new UploadLogWindow();
                        uploadWindow.UploadFile = path;
                        uploadWindow.MessageVisbility = Visibility.Visible;
                        uploadWindow.Show();
                    }
                    else
                    {
                        uploadLogWindow.WindowState = WindowState.Normal;
                        uploadLogWindow.UploadFile = path;
                        uploadLogWindow.MessageVisbility = Visibility.Visible;
                        uploadLogWindow.Activate();
                    }
                }
            }
        }

        private void AddinChangedAction()
        {
            if (!WesMain.UseDebugAttach)
                UpdateNavButtonEnable(false);
            else
                UpdateNavButtonEnable(true);
            InitNavigator();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            string message = "正在作業中，無法關閉主窗口，請先完成當前作業";
            if (WindowHelper.IsWorking(message)) e.Cancel = true;
            if (e.Cancel == false) WindowKeyboardHook.RemoveHook();
        }

        private void Instance_LoadFlowMuduleProgress(object sender, LoadFlowEventArgs e)
        {
            ShowProgressMessage(e.Description);
        }

        private void ShowProgressMessage(string progressMessage, bool isShow = true)
        {
            if (isShow)
            {
                ProgressVisibility = Visibility.Visible;
                ProgressText = progressMessage;
            }
            else
            {
                ProgressVisibility = Visibility.Hidden;
                ProgressText = "";
            }
        }

        /// <summary>
        /// 處理掃描結果
        /// </summary>
        /// <param name="scanValue"></param>
        protected void Scan(List<string> scanWorkNos)
        {
            if (scanWorkNos == null || scanWorkNos.Count == 0 || !scanWorkNos[0].IsWorkNo())
            {
                UpdateAddinStatus("工作單號格式錯, 請重新掃描");
                return;
            }

            if (WindowHelper.IsWorking("正在作業中，无法切换客户，請先完成當前作業"))
            {
                return;
            }

            this._barCodeScanFrame.IsEnabled = false;

            dynamic customerObj = RestApi.NewInstance(Method.POST)
                                    .AddUriParam(RestUrlType.WmsServer, ScriptID.GET_ENDCUSTOMER_BY_WORKNO, false)
                                    .AddJsonBody("workNo", scanWorkNos[0])
                                    .Execute()
                                    .To<object>();
            if ((int)customerObj.errorCode != 0)
            {
                UpdateAddinStatus(customerObj.errorMsg.ToString());
                this._barCodeScanFrame.IsEnabled = true;
                return;
            }

            string code = customerObj.customer.ToString();
            string name = customerObj.customerName.ToString();
            string addInName = PrepareAddInName(code, scanWorkNos[0]);

            if (!string.IsNullOrEmpty(addInName))
            {
                WesDesktop.Instance.UpdateAddIn(addInName, code, name);
                InitNavigator(() => OpenFlowWindow(scanWorkNos));
                this._barCodeScanFrame.IsEnabled = true;
            }
            else
            {
                UpdateAddinStatus(string.Format("Addin_Unrealize".GetLanguage(), string.Format("{0}[{1}]", customerObj.customerName, code)));
                this._barCodeScanFrame.IsEnabled = true;
            }
        }

        /// <summary>
        /// 獲取單號使用的插件
        /// </summary>
        /// <param name="endCustomerCode"></param>
        /// <param name="scanVal"></param>
        /// <returns></returns>
        private string PrepareAddInName(string endCustomerCode, string scanVal)
        {
            string addInName = WS.GetService<IStartup>().GetAddInFromCode(endCustomerCode);
            if (string.IsNullOrEmpty(addInName))
            {
                //如果為txt則查找是否有空運貼標的單
                //空運單打開通用插件
                if (scanVal.IsTxt() && IsAirTransportation(scanVal))
                {
                    addInName = WesApp.GeneralAddIn;
                }
            }
            return addInName;
        }

        /// <summary>
        /// 验证是否为空运单
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool IsAirTransportation(string val)
        {
            return RestApi.NewInstance(Method.GET)
                        .AddCommonUri(RestUrlType.WmsServer, "air-shipping/verification-txt-exist")
                        .AddQueryParameter("txt", val)
                        .Execute()
                        .ToBoolean();
        }

        private void OpenFlowWindow(List<string> workNos = null)
        {
            this.BarcodeTooltip = "BarcodeConfirmTooltip".GetLanguage() + WesDesktop.Instance.AddIn.EndCustomerName;
            UpdateNavButtonEnable(true);

            int flowId = (int)LoadFlowMudule.Instance.GetFlowIDByWorkNo(workNos[0]);
            MenuServiceHelper.OpenFlowWindow(flowId, workNos);
        }

        private void InitNavigator(Action callback = null)
        {
            LoggingService.Debug("Initialize addin modules...");
            ShowProgressMessage("准备数据中...");
            this.UpdateNavButtonEnable();
            LoadFlowMudule.Instance.LoadMudules(() =>
            {
                WesApp.UiActionInvoke(() =>
                {
                    CreateNavigators();
                    LoadMudulesComplate();
                    callback?.Invoke();
                });
            }, WesDesktop.Instance.AddIn.Name);

        }

        private void CreateNavigators()
        {
            this._navigatorPanel.Children.Clear();
            var menuItems = WS.GetService<IMenu>().InitDesktopNavigator();
            foreach (var item in menuItems)
                this._navigatorPanel.Children.Add(item);
        }

        private void CreateLeftNavigators()
        {
            this._mainWindow.leftMenuPanel.Children.Clear();
            var leftMenuItems = WS.GetService<IMenu>().InitLeftNavigator();
            foreach (var item in leftMenuItems)
            {
                this._mainWindow.leftMenuPanel.Children.Add(item);
            }
        }

        private void UpdateAddinStatus(string message)
        {
            this.UpdateNavButtonEnable(false);
            WesDesktop.Instance.ResetAddIn();
            this.BarcodeTooltip = message;
        }

        private void UpdateNavButtonEnable()
        {
            foreach (var child in this._navigatorPanel.Children)
            {
                NavButton navButton = child as NavButton;
                if (navButton != null) navButton.IsEnabled = WesMain.IsEnabled;
            }
        }

        private void UpdateNavButtonEnable(bool isEnabled)
        {
            WesMain.IsEnabled = isEnabled;
            this.UpdateNavButtonEnable();
        }

        private void LoadMudulesComplate()
        {
            this.UpdateNavButtonEnable();
            this.ShowProgressMessage("", false);
            BarcodeTooltip = "BarcodeTooltip".GetLanguage();
            this._barCodeScanFrame.IsEnabled = true;
            this._barCodeScanFrame.SelectAll();
            this._barCodeScanFrame.Focus();
            LoggingService.Debug("Initialize addin modules completed...");
        }

    }
}
