using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wes.Addins.Addin;
using Wes.Core;
using Wes.Core.Service;
using Wes.Desktop.Windows.DebugCommand;
using Wes.Desktop.Windows.Listener;
using Wes.Desktop.Windows.Options;
using Wes.Desktop.Windows.Options.View;
using Wes.Desktop.Windows.Printer;
using Wes.Desktop.Windows.Updater;
using Wes.Desktop.Windows.View;
using Wes.Print;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Desktop.Windows
{
    public sealed class Startup : IStartup
    {
        public event EventHandler ActiveContentChanged;

        public void Run()
        {
            try
            {
                Initialize();
                ThreadPool.QueueUserWorkItem((obj) => { DownloadLauncher(); });
                ThreadPool.QueueUserWorkItem((obj) => { InitializePrinter(); });
                ThreadPool.QueueUserWorkItem((obj) => { UpdateLabelTemplate(); });
                ThreadPool.QueueUserWorkItem((obj) => { OpenAllPort(); });
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private void Initialize()
        {
            //TODO: 升级稳定后此处打开，Desktop也支持升级
#if !DEBUG
            CheckWesVersion();
#endif
            LoggingService.InitializeLogService();
            WesDesktop.Instance.OnPropertyChanged();
            LoggingService.InfoFormat("Initialize wes");
            Utils.KillProcess(WesApp.BARTENDER);
            InitializeLanguage();
            InitializeSettings();
        }

        private void CheckWesVersion()
        {
            WesUpdater updater = new WesUpdater();
            updater.WesUpdate();
        }

        private void InitializeLanguage()
        {
            using (DebugTimer.Time("Initialize wes language"))
            {
                string language = OptionConfigureService.GetLanguageValue();
                LanguageService.ChangeLanguages(language);
                ActiveContentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void InitializeSettings()
        {
            Utils.CheckCreateDirectoroy(AppPath.DataPath, true);
            using (DebugTimer.Time("Initialize wes config"))
            {
                try
                {
                    string theme = OptionConfigureService.GetTheme("Appearance");
                    AppearanceManager.Current.ThemeSource = new Uri(theme, UriKind.Relative);
                    AppearanceManager.Current.AccentColor = OptionConfigureService.GetAccentColor("Appearance");
                    FontSizeAppearance.SetFontSize(OptionConfigureService.GetFontSize("Appearance"));
                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                }
            }
        }

        public void InitializePrinter()
        {
            WS.GetService<IPrintWindowProvider>().Initialize();
            if (!WesPrint.Engine.IsInitialed)
            {
                WesApp.UiActionInvoke(() => { WesModernDialog.ShowWesMessage("打印机模块加载失败，这会导致打印标签失败，请重启工作站"); });
            }
        }


        public void DownloadLauncher()
        {
#if !DEBUG
            Task.Factory.StartNew(() =>
            {
                HttpHelper.DownloadLauncher((res) =>
                {
                    LoggingService.DebugFormat("Update file launcher.exe result:{0}", res.ToString());
                });
            });
#endif
        }


        public void UpdateLabelTemplate()
        {
            Wes.Print.WesPrint.Engine.UpdateTemplates((total, file) =>
            {
                LoggingService.InfoFormat("Update label, path:{0}", file);
            }, (list) =>
            {
                LoggingService.InfoFormat($"Update label complated. total count:{list.Count}");
            });
#if DEBUG
            LoggingService.Debug(DebugCommandService.GetDebugHelpInfo());
#endif
        }

        /// <summary>
        /// 打開所有已配置紅外線串口
        /// 測試未能打開的串口
        /// </summary>
        public void OpenAllPort()
        {
            HashSet<string> errorPorts = SerialPortProvider.GetInvalidPortConfig();
            if (errorPorts.Count > 0)
            {
                WesDesktopSounds.Failed();
                string coms = string.Join(",", errorPorts);
                string message = string.Format("{0}配置錯誤,請重新配置打印機紅外感應串口!", coms);
                throw new WesException(message);
            }

            HashSet<string> ports = SerialPortProvider.GetAllPortConfig();
            LoggingService.InfoFormat("配置的串口:{0}", string.Join(",", ports));
            bool isFinished = false;
            HashSet<string> openFailedComs = new HashSet<string>();
            int openComCount = 0;
            foreach (var item in ports)
            {
                LoggingService.InfoFormat("打開已配置串口:{0}", item);
                SerialPortManager.OpenSerialPortByCom(item, (res, com) =>
                {
                    if (!res)
                    {
                        openFailedComs.Add(com);
                    }
                    openComCount++;
                    if (openComCount == ports.Count) isFinished = true;
                }, null);
            }
            WesApp.UiThreadAlive(ref isFinished, 1000 * 10);
            if (openFailedComs.Count > 0)
            {
                WesDesktopSounds.Failed();
                string coms = string.Join(",", openFailedComs);
                string message = string.Format("{0}打開失敗,防呆功能可能將會失效,請檢查串口設備是否正常!", coms);
                WesApp.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    WesModernDialog.ShowWesMessage(message);
                }));
            }
        }

        #region static method

        /// <summary>
        /// 初始化默认插件，获取第一个
        /// </summary>
        /// <returns></returns>
        public void InstallAddIn()
        {
            var addin = ConfigurationMapping.Instance.GetAddInByIndex(1);  //默認avnet
            if (addin != null) UpdateAddIn(addin.name.ToString());
        }

        public bool UpdateAddIn(string addinName)
        {
            var addin = ConfigurationMapping.Instance.GetAddInByName(addinName);
            if (addin != null)
            {
                string name = addin.name;
                string code = string.Empty;
                if (addin.endCustomer != null)
                {
                    code = ConfigurationMapping.Instance.ConvertToAddInCode(addin.endCustomer.code);
                }
                WesDesktop.Instance.UpdateAddIn(name, code, name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 獲取客戶對應插件
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetAddInFromCode(string code)
        {
            string name = string.Empty;
            var addin = ConfigurationMapping.Instance.GetAddInByCode(code);
            if (addin != null) name = addin.name;
            return name;
        }

        #endregion

        public void RegisterListenInvoker(string addinName)
        {
            WS.Services.RemoveService(typeof(IActionAvnetListenInvoker));
            WS.Services.RemoveService(typeof(IActionListenInvoker));

            if (string.Equals(addinName, "avnet", StringComparison.OrdinalIgnoreCase))
                WS.Services.AddService(typeof(IActionAvnetListenInvoker), new AvnetKPIListenInvoker());
            else
                WS.Services.AddService(typeof(IActionListenInvoker), new KPIListenInvoker());
        }
    }
}
