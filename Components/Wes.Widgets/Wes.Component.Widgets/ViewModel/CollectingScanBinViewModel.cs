using System;
using Wes.Core;
using Wes.Core.Base;
using Wes.Core.Service;
using Wes.Core.ViewModel;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Component.Widgets.ViewModel
{
    /**
     * name: 自定义参数
     * __name: 内置参数
     * #sid#xxx: 内置常量
     */
    public abstract class CollectingScanBinViewModel<A> : ScanViewModelBase<WesFlowID, A>
        where A : IScanAction
    {
        protected override void Scan(string scanVal, bool isShortcut = false)
        {
            //多个action处理逻辑
            ReinitializeAction(GetActionType(scanVal));

            #region 扫描拦截

            try
            {
                _currentScanTarget = ScanInterceptor(scanVal, _currentScanTarget);
            }
            catch (ScanInterceptorException)
            {
                // ignore
            }

            #endregion

            #region 根据scan_target查找对应action的方法

            var methodName = "Scan" + _currentScanTarget.ToString().Underline2Hump();
            var commandName = GetCommandName(scanVal);
            if (!string.IsNullOrEmpty(commandName))
            {
                methodName = commandName.Underline2Hump();
            }

            if (!BeforeScanInvoke(scanVal)) return;
            var method = _scanAction.GetType().GetMethod(methodName);
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
                LoggingService.Debug($"在 {ScanAction.GetType()} 中没有找到签名 {methodName}() 的方法");
            }

            #endregion

            //入侵扫描流程
            ScanBinNo(scanVal);
        }

        private void ScanBinNo(string scanValue)
        {
            var flowId = WesFlow.Instance.GetFlowID(FlowName);
            var wesFlowId = WesFlow.Instance.GetFlow(flowId);
            if (wesFlowId != WesFlowID.FLOW_IN_GATHER) return;
            //扫描储位
            if (WesFlowID.FLOW_ACTION_SCAN_BIN_NO == CurrentScanTarget)
            {
                RestApi.Of(Method.PUT)
                    .SetUrl(RestUrlType.WmsServer, "/receiving/update-location")
                    .AddQueryParameter("binNo", scanValue)
                    .AddQueryParameter("packageId", SelfInfo.__pid)
                    .Execute();
            }
            else
            {
                if (!scanValue.IsReceivingPackageID()) return;
                //当前上下文中有板号
                if (DynamicUtil.IsExist(Info, "__palletId"))
                {
                    if (scanValue.ToPalletNo().ToUpper() != SelfInfo.__palletId.ToUpper())
                    {
                        SelfInfo.__currentScanTarget = CurrentScanTarget;
                        SelfInfo.__tooltip = Tooltip;
                        Next(WesFlowID.FLOW_ACTION_SCAN_BIN_NO, "BinNo");
                    }
                }
                //当前上下文中没有板号
                else
                {
                    SelfInfo.__currentScanTarget = CurrentScanTarget;
                    SelfInfo.__tooltip = Tooltip;
                    Next(WesFlowID.FLOW_ACTION_SCAN_BIN_NO, "BinNo");
                }

                SelfInfo.__pid = scanValue;
                //当前箱类型是板箱或散箱
                SelfInfo.__palletId = scanValue.IsReceivingPalletPackageID()
                    ? scanValue.ToPalletNo()
                    : "#6491123946098688#DEFAULT_PALLET";
            }
        }
    }
}