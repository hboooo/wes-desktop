using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wes.Addins.Addin;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Core.Service;
using Wes.Desktop.Windows.Listener;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Languages;

namespace Wes.Desktop.Windows.View
{
    public class LoadFlowMudule
    {
        private static LoadFlowMudule _loadFlowMudule;

        private static readonly object _locker = new object();
        /// <summary>
        /// 保存当前已加载模块
        /// </summary>
        public Dictionary<ICommandMetaData, object> MuduleViews { get; private set; } = new Dictionary<ICommandMetaData, object>();
        /// <summary>
        /// 保存当前模块对应KPI Command
        /// </summary>
        public Dictionary<IKPICommand, ICommandMetaData> KPICommands { get; private set; } = new Dictionary<IKPICommand, ICommandMetaData>();

        private delegate void InitAddInHandler(List<object> objectParams, string name, Action action);
        private string _loadedAddIn = string.Empty;

        public static LoadFlowMudule Instance
        {
            get
            {
                if (_loadFlowMudule == null)
                {
                    lock (_locker)
                    {
                        if (_loadFlowMudule == null)
                        {
                            _loadFlowMudule = new LoadFlowMudule();
                        }
                    }
                }
                return _loadFlowMudule;
            }
        }

        public LoadFlowMudule()
        {
            Addins.Addin.AddinExecute.Instance.CommandLoadProgress -= Instance_CommandLoadProgress;
            Addins.Addin.AddinExecute.Instance.CommandLoadProgress += Instance_CommandLoadProgress;
            WS.Services.RemoveService(typeof(IActionNotity));
            WS.Services.AddService(typeof(IActionNotity), new ActionFlowActionNotify());
            LoggingService.Debug("Loading mudules...");
        }

        private void Instance_CommandLoadProgress(object sender, CommandLoadEventArgs e)
        {
            LoggingService.Debug(string.Format("CommandName:{0} Index:{1}", e.CommandName, e.CommandIndex));
            LoadFlowEventArgs args = new LoadFlowEventArgs();
            if (e.CommandMetaData != null)
            {
                args.Description = string.Format(LanguageService.GetLanguages("Loading"), e.CommandName.GetLanguage());
                args.MuduleName = e.CommandName;
            }

            OnLoadFlowMuduleProgress(args);
        }

        public List<KeyValuePair<ICommandMetaData, object>> GetMuduleViews(int flowId)
        {
            List<KeyValuePair<ICommandMetaData, object>> mudules = new List<KeyValuePair<ICommandMetaData, object>>();
            foreach (var item in MuduleViews)
            {
                int commandId = WesFlow.Instance.GetFlowID(item.Key.CommandName);
                if ((commandId & flowId) == flowId)
                {
                    mudules.Add(new KeyValuePair<ICommandMetaData, object>(item.Key, item.Value));
                }
            }
            return mudules;
        }

        private List<object> PrepareParams(string addinName)
        {
            List<object> addinParams = new List<object>();
            if (!string.IsNullOrEmpty(addinName))
            {
                addinParams.Add(addinName);
            }
            return addinParams;
        }

        public void LoadMudules(Action action, string addinName)
        {
            if (string.Compare(addinName, _loadedAddIn, true) == 0)
            {
                action?.Invoke();
                return;
            }

            LoadFlowMudule.Instance.Release();
            Task.Factory.StartNew(() =>
            {
                List<object> addinParams = PrepareParams(addinName);
                WesApp.Current.Dispatcher.BeginInvoke(new InitAddInHandler(InitAddIn), addinParams, addinName, action);
            });
        }

        private void InitAddIn(List<object> addinParams, string addinName, Action action)
        {
            try
            {
                if (addinParams.Count > 0)
                {
                    using (DebugTimer.Time("Initialize Addins"))
                    {
                        _loadedAddIn = addinName;
                        MuduleViews = AddinExecute.Instance.MainExecute(addinParams.ToArray());
                    }
                    using (DebugTimer.Time("Initialize action listen"))
                    {

                        KPICommands = AddinExecute.Instance.KPIInitialize(addinParams.ToArray());
                        WS.GetService<IStartup>().RegisterListenInvoker(addinName);
                    }
                    LoggingService.InfoFormat("Addin [{0}] VERSION:{1}", addinName, DesktopVersion.GetAddinFullVersionNo(addinName));
                }
                else
                {
                    LoggingService.Fatal("Addin name not set default value, ignore");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Fatal(ex);
            }
            finally
            {
                action?.Invoke();
            }
        }

        private bool IsFlowLoaded(WesFlowID flow)
        {
            var query = MuduleViews.Where(kvp => kvp.Key.CommandName == flow.ToString());
            if (query.Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据单号打开默认窗口
        /// </summary>
        /// <param name="workNo"></param>
        /// <returns></returns>
        public WesFlowID GetFlowIDByWorkNo(string workNo)
        {
            HashSet<WesFlowID> items = WorkNoFlowService.GetWorkNoFlow(workNo);
            foreach (var item in items)
            {
                if (IsFlowLoaded(item))
                    return item;
            }
            return WesFlowID.FLOW_IN_PALLET_TO_CARTON;
        }

        public void Clear()
        {
            if (MuduleViews != null) MuduleViews.Clear();
            if (KPICommands != null) KPICommands.Clear();
        }

        public void Release()
        {
            this.Clear();
            Core.Bean.BeanFactory.Clear();
            this._loadedAddIn = string.Empty;
            AddinExecute.Instance.ReleaseAddins();
        }

        public event EventHandler<LoadFlowEventArgs> LoadFlowMuduleProgress;

        private void OnLoadFlowMuduleProgress(LoadFlowEventArgs args)
        {
            LoadFlowMuduleProgress?.Invoke(this, args);
        }
    }

    public class LoadFlowEventArgs : EventArgs
    {
        public string Description { get; set; }

        public string MuduleName { get; set; }

        public int Total { get; set; }

        public int Progress { get; set; }
    }
}
