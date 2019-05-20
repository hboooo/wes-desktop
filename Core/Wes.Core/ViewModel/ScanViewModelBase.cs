using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Wes.Core.Api;
using Wes.Core.Base;
using Wes.Core.Service;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Utilities.Languages;

namespace Wes.Core.ViewModel
{
    /**
     * 采集, 贴标等View Model的基类, 它是一个抽象的类, 请按照约定正确使用
     * T: 当前扫描目标, 初始化状态时根据实现类FirstScan()方法返回确定第一个扫描对象
     * A: 扫描动作处理集, 以AVNET1.0为例, 采集, 贴标等
     */
    public abstract class ScanViewModelBase<T, A> : ViewModelBase
        where T : struct
        where A : IScanAction
    {
        private ICommand _clearCommond;

        protected T _currentScanTarget;
        private HashSet<string> _exceptionIgnoreProperties = new HashSet<string> {"IntelligentItems", "UseIntelligent"};
        private string _flowName;

        private List<Tuple<int, T, string>> _flows;
        private ICommand _handPrintCommand;
        private DynamicObjectClass _info = new DynamicObjectClass();
        private ICommand _loadCommond;

        /// <summary>
        /// 默認Action
        /// </summary>
        private A _mainActionCopy = default(A);

        private Dictionary<T, dynamic> _nextTargets = new Dictionary<T, dynamic>();
        protected A _scanAction;
        private ICommand _scanCommand;
        private string _scanValue;
        private ICommand _supportCommand;
        private string _tooltip;
        private Dictionary<string, A> _virtualScanActions;

        protected ScanViewModelBase()
        {
            InitNextTargets(_nextTargets);
            Init();
            ResetUiStatus();
        }

        /// <summary>
        /// 是否啟用VirtualAction
        /// </summary>
        public virtual bool _isVirtualActionEnabled { get; set; } = false;

        /**
         * 扫描计划字典, key=扫描计划枚举标识, value=一个可动态扩展的对象
         */
        public Dictionary<T, dynamic> NextTargets
        {
            get { return _nextTargets; }
        }

        public List<Tuple<int, T, string>> Flows
        {
            get
            {
                if (_flows == null)
                {
                    _flows = new List<Tuple<int, T, string>>();
                    for (int i = 0; i < _nextTargets.Count; i++)
                    {
                        var tooltip = DynamicUtil.GetValue<string>(_nextTargets.ElementAt(i).Value, "tip");
                        if (string.IsNullOrEmpty(tooltip))
                        {
                            tooltip = _nextTargets.ElementAt(i).Key.ToString().GetLanguage();
                        }

                        _flows.Add(new Tuple<int, T, string>(i + 1, _nextTargets.ElementAt(i).Key, tooltip));
                    }
                }

                return _flows;
            }
        }

        /**
         * 获取第一个扫描计划
         */
        protected abstract T GetFirstScan();

        /**
         * 返回扫描动作集
         */
        protected abstract A GetAction();

        /// <summary>
        /// 返回擴展Action
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<string, A> GetExtendActions()
        {
            LoggingService.InfoFormat("VirtualAction Enabled is {0}", _isVirtualActionEnabled);
            if (_isVirtualActionEnabled)
            {
                Dictionary<string, A> scanActions = new Dictionary<string, A>();
                Dictionary<string, IScanAction> actions =
                    ActionService.GetVirtualActoins(Assembly.GetAssembly(typeof(A)), typeof(A).Name);
                foreach (var item in actions)
                {
                    scanActions[item.Key] = (A) item.Value;
                    scanActions[item.Key].SetContext(this);
                }

                return scanActions;
            }

            return null;
        }

        protected virtual bool VirtualActionEnabled()
        {
            return false;
        }

        /// <summary>
        /// 返回指定扩展Action
        /// 此Action需要继承A
        /// </summary>
        /// <param name="type">与VirtualActionAttribute中的Type值对应</param>
        /// <returns></returns>
        protected virtual A GetExtendAction(string type)
        {
            if (_isVirtualActionEnabled)
            {
                if (_virtualScanActions != null && _virtualScanActions.ContainsKey(type))
                {
                    return _virtualScanActions[type];
                }
            }

            return default(A);
        }

        /**
         * 初始化扫描对象字典, 该字典KEY必须使用枚举, Value必须为dynamic对象且包含tooltip属性, 例如nextTargets.Add(ScanType.PID, new {tooltip = "pid"});
         * ScanType.PID: 扫描计划的枚举标识
         * new {tooltip = "pid"}: 当进入到该计划动作中时, 给用户的提示, 拼接方式 Please scan $tooltip$. ScanViewModeBase.Next会将该值转换为全大写
         */
        protected virtual void InitNextTargets(Dictionary<T, dynamic> nextTargets)
        {
        }

        protected virtual void Load(dynamic model)
        {
        }

        protected virtual T ScanInterceptor(string scanVal, T cst)
        {
            throw new ScanInterceptorException();
        }

        protected virtual string GetCommandName(string scanVal)
        {
            return WesScanCommand.GetCommandName(scanVal);
        }

        protected virtual bool BeforeScanInvoke(string val)
        {
            if (NextTargets.First().Key.Equals(CurrentScanTarget))
            {
                ActionFlowEventArgs args =
                    new ActionFlowEventArgs(QrCodeFilterUtils.ResolverCommand(val, CurrentScanTarget, true));
                WS.ActionNotityService.Execute(args);
#if !DEBUG
                if (args.Handled)  return false; 
#endif
            }

            return true;
        }

        /**
         * 一个简单的调度函数方法, 根据子类的T值来确定下一步要调用的函数, 默认的调用时根据GetFirstScan()的返回值来确定
         * 当扫描到数据且在回车 KeyUp 时执行
         */
        protected virtual void Scan(string scanVal, bool isShortcut = false)
        {
            ReinitializeAction(GetActionType(scanVal));

            //扫描拦截
            try
            {
                _currentScanTarget = ScanInterceptor(scanVal, _currentScanTarget);
            }
            catch (ScanInterceptorException)
            {
                // ignore
            }

            var methodName = "Scan" + _currentScanTarget.ToString().Underline2Hump();
            var commandName = GetCommandName(scanVal);
            if (!string.IsNullOrEmpty(commandName))
            {
                methodName = commandName.Underline2Hump();
            }

            if (!BeforeScanInvoke(scanVal)) return;

            //尝试找完整名称的方法
            var method = _scanAction.GetType().GetMethod(methodName);
            
            //如果没有找到完整名称的方法则找简写
            if (method == null)
            {
                method = _scanAction.GetType().GetMethod(methodName.Replace("ScanFlowAction", ""));
            }

            if (method != null)
            {
                try
                {
                    using (DebugTimer.InfoTime($"Invoke {_scanAction.GetType().Name} method:{method.Name}"))
                    {
                        _scanAction.BeginScan(scanVal);
                        method.Invoke(_scanAction, new object[] {scanVal});
                    }
                }
                catch (Exception e)
                {
                    var ex = ThrowException(e);
                    if (isShortcut)
                    {
                        ActionFlowEventArgs args = new ActionFlowEventArgs(ex);
                        WS.ActionNotityService.Execute(args);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                //throw new WesException($"在 {ScanAction.GetType()} 中没有找到签名 {methodName}() 的方法");
                LoggingService.Debug($"在 {ScanAction.GetType()} 中没有找到签名 {methodName}() 的方法");
            }
        }

        protected Exception ThrowException(Exception e)
        {
            if (e.InnerException is WesException)
            {
                if (e.InnerException is WesRestException)
                {
                    var reqException = e.InnerException as WesRestException;
                    var ex = new WesException(e.InnerException.Message, GetExceptionData(), e.InnerException);
                    ex.AddCustomerInfo("000_scanValue", ScanValue);
                    ex.AddCustomerInfo("001_messageId", reqException.MessageId.ToString());
                    ex.AddCustomerInfo("002_code", reqException.MessageCode.ToString());
                    return ex;
                }
                else
                {
                    var ex = new WesException(e.InnerException.Message, GetExceptionData(), e.InnerException);
                    return ex;
                }
            }
            else
            {
                var ex = new WesException(e.Message, GetExceptionData(), e);
                return ex;
            }
        }

        /**
         * 进入下一个扫描计划
         */
        public virtual void Next(T scanType)
        {
            Next(scanType, null);
        }

        public virtual void Next(T scanType, String tooltip)
        {
            if (string.IsNullOrEmpty(tooltip))
            {
                if (_nextTargets.ContainsKey(scanType))
                {
                    tooltip = DynamicUtil.GetValue<string>(_nextTargets[scanType], "tip");
                }

                if (string.IsNullOrEmpty(tooltip))
                {
                    tooltip = scanType.ToString().GetLanguage();
                }
            }

            string append = GetTooltipAppend().Trim();
            Tooltip = $"{"Please_Scan".GetLanguage()}:{tooltip}" +
                           (string.IsNullOrEmpty(append) ? "" : $"  ({append})");
            ScanValue = string.Empty;
            _currentScanTarget = scanType;

            RaisePropertyChanged<T>(nameof(CurrentScanTarget));
        }

        protected virtual string GetTooltipAppend()
        {
            string append = string.Empty;
            var attrs = _scanAction.GetType().GetCustomAttributes(typeof(VirtualActionAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                VirtualActionAttribute actionAttr = attrs[0] as VirtualActionAttribute;
                if (actionAttr != null) append = " " + actionAttr.Type;
            }

            return append;
        }

        /**
         * 进入下一个扫描计划
         */
        public virtual void Next()
        {
            Next(GetFirstScan());
        }

        /**
         * 重置UI状态
         */
        public virtual void ResetUiStatus()
        {
            Info = new DynamicObjectClass();
            ScanValue = null;
            Next(GetFirstScan());
        }

        public virtual void CleanScanValue()
        {
            ScanValue = null;
        }

        /**
         * 初始化
         */
        private void Init()
        {
            if (_nextTargets.Count == 0)
            {
                T firstScan = GetFirstScan();
                _nextTargets.Add(firstScan, firstScan);
            }

            _currentScanTarget = GetFirstScan();
            _isVirtualActionEnabled = VirtualActionEnabled();
            _virtualScanActions = GetExtendActions();
            LoggingService.DebugFormat("Initialize virtual Action complated, count:{0}",
                _virtualScanActions == null ? 0 : _virtualScanActions.Count);

            _scanAction = GetAction();
            _mainActionCopy = _scanAction;
            if (_scanAction == null)
            {
                throw new WesException("Action() 返回值不能为空");
            }

            _scanAction.SetContext(this);
            Next(_currentScanTarget);
        }

        /// <summary>
        /// 重置Action
        /// </summary>
        /// <param name="type"></param>
        public virtual void ReinitializeAction(string type = null)
        {
            if (string.IsNullOrEmpty(type))
            {
                _scanAction = _mainActionCopy;
                _scanAction.SetContext(this);
                LoggingService.DebugFormat("Reinitialize Action :{0}", nameof(ScanAction));
            }
            else
            {
                var extend = GetExtendAction(type);
                if (extend != null)
                {
                    _mainActionCopy = _scanAction;
                    _scanAction = extend;
                    ResetAction(extend);
                    LoggingService.DebugFormat("Reinitialize Action :{0}", type);
                }
            }
        }

        public virtual void ReinitializeAction(string type = null, params string[] routeParams)
        {
            Dictionary<string, object> routeParamsMap = new Dictionary<string, object>();
            routeParamsMap.Add("routeIsHandled", false);
            if (routeParams != null)
            {
                if (routeParams.Length % 2 != 0)
                {
                    throw new WesException("Action路由器参数数量必须为偶数");
                }
                else
                {
                    for (int i = 0; i < routeParams.Length; i++)
                    {
                        if (i % 2 == 0)
                        {
                            routeParamsMap[routeParams[i]] = routeParams[i + 1];
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(type))
            {
                _scanAction = _mainActionCopy;
                _scanAction.SetContext(this);
                LoggingService.DebugFormat("Reinitialize Action :{0}", nameof(ScanAction));
            }
            else
            {
                var extend = GetExtendAction(type);
                if (extend == null) return;
                if (extend is IScanActionContext actionContext && routeParams != null)
                {
                    dynamic context = actionContext.getContext();
                    context._routeParams = routeParamsMap;
                }

                var routeMeMethod = extend.GetType().GetMethod("RouteMe");
                if (routeMeMethod != null)
                {
                    if (_currentScanTarget.ToString() == "FLOW_ACTION_SCAN_PACKAGE_ID")
                    {
                        GetActionType(routeParamsMap["pid"].ToString());
                    }
                    else
                    {
                        GetActionType("FLOW_ROUTE");
                    }

                    routeMeMethod.Invoke(extend, new object[] {routeParamsMap});
                }

                _mainActionCopy = _scanAction;
                _scanAction = extend;
                ResetAction(extend);
                LoggingService.DebugFormat("Reinitialize Action :{0}", type);
            }
        }

        protected virtual string GetActionType(string scanValue)
        {
            return null;
        }

        protected virtual void ResetAction(A scanAction)
        {
        }

        public IDictionary<string, object> GetSelfInfo()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            IEnumerable<string> pros = Info.GetDynamicMemberNames();
            foreach (var name in pros)
            {
                var value = Info.GetMember(name);
                if (value is IConvertible || value is List<string>)
                {
                    dictionary.Add(name, value);
                }
            }

            return dictionary;
        }

        /// <summary>
        /// 获取异常时Info时时数据（排除不必要的数据，智能提示数据源默认排除，原因：数据过大）
        /// 子类可重写GetExceptionIgnoreProperties增加过滤不需要的属性
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, object> GetExceptionData()
        {
            GetExceptionIgnoreProperties(_exceptionIgnoreProperties);
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            IEnumerable<string> pros = Info.GetDynamicMemberNames();
            foreach (var name in pros)
            {
                var query = _exceptionIgnoreProperties.Where(s => string.Compare(s, name, true) == 0);
                if (query.Count() > 0) continue;
                var value = Info.GetMember(name);
                if (value != null)
                    dictionary.Add(name, value);
            }

            return dictionary;
        }

        public virtual void GetExceptionIgnoreProperties(HashSet<string> exceptionIgnoreProperties)
        {
        }

        #region DynamicObjectClass 适配器

        /**
         * DynamicObjectClass 适配器
         */
        public dynamic SelfInfo
        {
            get { return _info.Self; }
        }

        /// <summary>
        /// 流程ID
        /// </summary>
        public string FlowName
        {
            get { return _flowName; }
            set { _flowName = value; }
        }

        #endregion

        #region UI Model with Command

        /**
         * 当前实际待扫描目标
         */
        public T CurrentScanTarget
        {
            get { return _currentScanTarget; }
        }

        /**
         * 获取扫描动作集
         */
        public A ScanAction
        {
            get { return _scanAction; }
        }

        /**
         * UI Mode对象, 该对象是动态的
         */
        public DynamicObjectClass Info
        {
            get { return _info; }
            set
            {
                _info = value;
                RaisePropertyChanged<string>(nameof(Info));
            }
        }

        /**
         * 扫描值
         */
        public String ScanValue
        {
            get { return _scanValue; }
            set
            {
                if (_scanValue != value)
                {
                    _scanValue = value;
                    RaisePropertyChanged<string>(nameof(ScanValue));
                }
            }
        }

        /**
         * 给用户的提示
         */
        public String Tooltip
        {
            get { return _tooltip; }
            set
            {
                if (_tooltip != value)
                {
                    _tooltip = value;
                    RaisePropertyChanged<string>(nameof(Tooltip));
                }
            }
        }

        /**
         * 扫描(KeyUp)命令
         */
        public ICommand ScanCommand
        {
            get
            {
                if (_scanCommand == null)
                {
                    _scanCommand = new RelayCommand<KeyEventArgs>((kea) =>
                    {
                        if (kea.Key == Key.Enter)
                        {
                            if (kea.Source is Control)
                            {
                                Type type = kea.Source.GetType();
                                MethodInfo methodInfo = type.GetMethod("SelectAll");
                                if (methodInfo != null) methodInfo.Invoke(kea.Source, null);

                                if (!string.IsNullOrWhiteSpace(ScanValue))
                                {
                                    Scan(QrCodeFilterUtils.ResolverCommand(ScanValue, CurrentScanTarget).Trim());
                                }
                            }
                        }
                        else
                        {
#if !DEBUG
                            if (ConfigurationMapping.Instance.Shortcut.TryGetValue((int) kea.Key, out string cmd))
                            {
                                kea.Handled = true;
                                Scan(cmd, true);
                            }
#endif
                        }
                    });
                }

                return _scanCommand;
            }
        }

        /**
         * 清空(clear)命令
         */
        public ICommand ClearCommand
        {
            get
            {
                return _clearCommond ?? (
                           _clearCommond = new RelayCommand(() =>
                           {
                               ResetUiStatus();

                               Window window = null;
                               if (Application.Current != null && Application.Current.Windows != null)
                               {
                                   for (int i = 1; i < Application.Current.Windows.Count; i++)
                                   {
                                       window = Application.Current.Windows[Application.Current.Windows.Count - i];
                                       if (window.GetType().Name != "AdornerLayerWindow")
                                       {
                                           break;
                                       }
                                   }
                               }

                               Control control =
                                   VisualTreeHelper.GetChildObjectByTypeName(window, "BarCodeScanFrame") as Control;
                               if (control != null)
                               {
                                   Type type = control.GetType();
                                   MethodInfo methodInfo = type.GetMethod("Focus");
                                   if (methodInfo != null)
                                   {
                                       methodInfo.Invoke(control, null);
                                   }
                               }
                           })
                       );
            }
        }

        public ICommand SupportCommand
        {
            get
            {
                return _supportCommand ?? (
                           _supportCommand = new RelayCommand(() => TeamSupport())
                       );
            }
        }

        protected virtual void TeamSupport()
        {
        }

        public ICommand HandPrintCommand
        {
            get
            {
                return _handPrintCommand ?? (
                           _handPrintCommand = new RelayCommand(() => HandPrintSupport())
                       );
            }
        }

        protected virtual void HandPrintSupport()
        {
        }

        public ICommand LoadedCommand
        {
            get
            {
                return _loadCommond ?? (
                           _loadCommond = new RelayCommand<Object>((sender) =>
                           {
                               FrameworkElement fe = null;
                               if (sender is FrameworkElement)
                               {
                                   fe = sender as FrameworkElement;
                               }
                               else if (sender is RoutedEventArgs)
                               {
                                   fe = (sender as RoutedEventArgs).Source as FrameworkElement;
                               }

                               if (fe != null)
                               {
                                   Control control =
                                       VisualTreeHelper.GetChildObjectByTypeName(fe, "BarCodeScanFrame") as Control;
                                   Type type = control?.GetType();
                                   MethodInfo methodInfo = type?.GetMethod("Focus");
                                   methodInfo?.Invoke(control, null);
                               }

                               Load(SelfInfo);
                           })
                       );
            }
        }

        #endregion
    }
}