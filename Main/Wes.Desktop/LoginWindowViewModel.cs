using FirstFloor.ModernUI.Presentation;
using System;
using System.Configuration;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Wes.Core.Service;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Desktop
{
    class LoginWindowViewModel : NotifyPropertyChanged
    {
        #region Private Properties

        private string serverUrl = ConfigurationManager.AppSettings["WMS_REST_SERVER_URL"].ToString();

        private LoginWindow _loginWindow = null;
        private bool _isAutoClose = false;

        private string _loginId;
        private string _versionInfo;
        private string _station;
        private Visibility _progressRingVisibility = Visibility.Hidden;
        private bool _progressRingActive = false;
        private bool _loginEnabled = true;

        #endregion

        #region Dependency Properties
        public string LoginId
        {
            get { return _loginId; }
            set
            {
                if (_loginId != value)
                {
                    _loginId = value;
                    OnPropertyChanged(nameof(LoginId));
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

        public string Station
        {
            get { return _station; }
            set
            {
                if (_station != value)
                {
                    _station = value;
                    OnPropertyChanged(nameof(Station));
                }
            }
        }

        public Visibility ProgressRingVisibility
        {
            get { return _progressRingVisibility; }
            set
            {
                if (_progressRingVisibility != value)
                {
                    _progressRingVisibility = value;
                    OnPropertyChanged(nameof(ProgressRingVisibility));
                }
            }
        }

        public bool ProgressRingActive
        {
            get { return _progressRingActive; }
            set
            {
                if (_progressRingActive != value)
                {
                    _progressRingActive = value;
                    OnPropertyChanged(nameof(ProgressRingActive));
                }
            }
        }

        public bool LoginEnabled
        {
            get { return _loginEnabled; }
            set
            {
                if (_loginEnabled != value)
                {
                    _loginEnabled = value;
                    OnPropertyChanged(nameof(LoginEnabled));
                }
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return new WindowRelayCommand(this, Login);
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new WindowRelayCommand(this, Cancel);
            }
        }

        #endregion

        public LoginWindowViewModel(LoginWindow loginWindow)
        {
            InializeWindow(loginWindow);
        }

        private void InializeWindow(LoginWindow loginWindow)
        {
            this.VersionInfo = "MainVersion".GetLanguage() + DesktopVersion.GetVersionNo();
            this._loginWindow = loginWindow;
            this._loginWindow.txtLoginId.Focus();
            this._loginWindow.Closed += _loginWindow_Closed;
            this._loginWindow.txtLoginId.KeyDown += TxtLoginId_KeyDown;
            this._loginWindow.pwd.KeyDown += Pwd_KeyDown;
        }

        private void Pwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this._loginWindow.txtStation.Focus();
                e.Handled = true;
            }
        }

        private void TxtLoginId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this._loginWindow.pwd.Focus();
                e.Handled = true;
            }
        }

        private void _loginWindow_Closed(object sender, EventArgs e)
        {
            if (this._isAutoClose == false)
            {
                WS.GetService<IMainWindow>().Close();
            }
        }

        private void Cancel()
        {
            WS.GetService<IMainWindow>()?.Close();
            this._loginWindow.Close();
        }

        private void Login()
        {
            if (WesMain.UseDebugAttach)
            {
                serverUrl = "wms-server";
                IPHostEntry hostEntry = Dns.GetHostEntry(serverUrl);
                IPEndPoint ipEndPoint = new IPEndPoint(hostEntry.AddressList[0], 0);
                string ip = ipEndPoint.Address.ToString();

                WesModernDialog.ShowWesMessage("当前wms-server 地址: " + ip);
                if (ip.Contains("172.16"))
                {
                    WesModernDialog.ShowWesMessage("当前生产环境: " + ip + "请谨慎操作");
                }
            }

            Authorize();
        }

        private void UpdateLoginStatus(bool status = true)
        {
            this.ProgressRingVisibility = status == true ? Visibility.Visible : Visibility.Hidden;
            this.ProgressRingActive = status;
            this.LoginEnabled = !status;
        }

        private void Authorize()
        {
            if (!IsInput()) return;

            UpdateLoginStatus();
            LoginAsync((res, err) =>
            {
                if (res)
                {
                    WesApp.UiActionInvoke(() =>
                    {
                        UpdateLoginStatus(false);
                        if (res)
                        {
                            WesMain.IsEnabled = true;
                            var mainWindow = WS.GetService<IMainWindow>();
                            WS.GetService<IStartup>().InstallAddIn();
                            LoggingService.Info($"Version:{WesDesktop.Instance.Version};Mac:{WesDesktop.Instance.MacAddress}");
                            this._isAutoClose = true;
                            this._loginWindow.Close();
                            mainWindow.Show();
                        }
                        else
                        {
                            WesModernDialog.ShowWesMessage(err);
                        }
                    });
                }
                else
                {
                    WesApp.UiActionInvoke(() =>
                    {
                        UpdateLoginStatus(false);
                        WesModernDialog.ShowWesMessage(err);
                    });
                }
            });
        }

        private bool IsInput()
        {
            if (string.IsNullOrWhiteSpace(LoginId))
            {
                WesModernDialog.ShowWesMessage("AccountNull".GetLanguage());
                return false;
            }
            else if (string.IsNullOrWhiteSpace(this._loginWindow.pwd.Password))
            {
                WesModernDialog.ShowWesMessage("PasswordNull".GetLanguage());
                this._loginWindow.pwd.Focus();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(Station))
            {
                WesModernDialog.ShowWesMessage("StationNull".GetLanguage());
                this._loginWindow.txtStation.Focus();
                return false;
            }
            return true;
        }

        private void LoginAsync(Action<bool, string> action)
        {
            WorkStationVaild((res, error) =>
            {
                if (res)
                    UsercodeValid(action);
                else
                    action(res, error);
            });
        }

        private void WorkStationVaild(Action<bool, string> action)
        {
            RestApi.NewInstance(Method.POST)
            .AddUriParam(RestUrlType.WmsServer, ScriptID.CHECK_WORKSTATION, false)
            .AddJsonBody("device", Station)
            .ExecuteAsync((res, exp, restApi) =>
            {
                if (restApi != null && restApi.ToInt() > 0)
                    action?.Invoke(true, null);
                else if (exp != null)
                    action?.Invoke(false, exp.Message);
                else
                    action?.Invoke(false, "DeviceError".GetLanguage());
            });
        }

        private void UsercodeValid(Action<bool, string> action)
        {
            RestApi.NewInstance(Method.POST)
            .AddUriParam(RestUrlType.WmsServer, ScriptID.CHECK_LOGIN, false)
            .AddJsonBody("loginId", LoginId)
            .AddJsonBody("password", this._loginWindow.pwd.Password)
            .ExecuteAsync((res, exp, restApi) =>
            {
                dynamic resObj = restApi != null ? restApi.To<object>() : null;
                if (resObj != null && resObj.Count > 0)
                {
                    foreach (var item in resObj)
                    {
                        WesDesktop.Instance.LogIn(LoginId, item.Password.ToString(), Station, item.UserName.ToString(), item.Uid.ToString());
                        WesDesktop.Instance.UpdateAuthority((int)item.LSDelete, (int)item.LSPrint, (int)item.LSNotCheckLotNo);
                        action?.Invoke(true, null);
                        break;
                    }
                }
                else if (exp != null)
                    action?.Invoke(false, exp.Message);
                else
                    action?.Invoke(false, "AccountError".GetLanguage());
            });
        }

    }
}
