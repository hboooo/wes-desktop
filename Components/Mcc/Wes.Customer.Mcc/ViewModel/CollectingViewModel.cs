using System.Text.RegularExpressions;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Mcc.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.ViewModel
{
    public class CollectingViewModel : ScanViewModelBase<WesFlowID, CollectingAction>, IViewModel
    {
        public ICommand DeleteCommand { get; private set; }
        public ICommand PrintCommand { get; private set; }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_IN_GATHER);
            teamSupport.ShowDialog();
        }

        protected override bool VirtualActionEnabled()
        {
            return true;
        }

        /**
         * 初始化第一次扫描目标
         */
        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID;
        }

        /**
         * 初始化扫描处理函数类
         */
        protected override CollectingAction GetAction()
        {
            return ViewModelFactory.CreateActoin<CollectingAction>() as CollectingAction;
        }

        protected override string GetActionType(string scanValue)
        {
            if (DynamicUtil.IsExist(SelfInfo, "_routeParams"))
            {
                SelfInfo.supplier = SelfInfo._routeParams["supplier"];
            }
            else if (scanValue.IsPackageID())
            {
                RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/receiving/check-receiving")
                    .AddQueryParameter("pid", scanValue)
                    .Execute();
                var supplier = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/receiving/supplier-by-pid")
                    .AddQueryParameter("pid", scanValue)
                    .Execute()
                    .To<string>();
                SelfInfo.supplier = supplier;
            }

            return SelfInfo.supplier;
        }

        protected override void ResetAction(CollectingAction scanAction)
        {
            DeleteCommand = new RelayCommand<long>(scanAction.DeleteCartonPart);
        }

        /**
         * 扫描拦截器
         * scanval: 扫描值
         * scanTarget: 计划扫描目标
         */
        protected override WesFlowID ScanInterceptor(string scanVal, WesFlowID scanTarget)
        {
            if (scanVal.IsPackageID())
            {
                return WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID;
            }
            else if (IsScanCollectingCnoForFlow(scanVal))
            {
                if (DynamicUtil.IsExist(SelfInfo, "pid") && SelfInfo.pid != null &&
                    !string.IsNullOrEmpty(SelfInfo.pid.ToString()))
                {
                    return WesFlowID.FLOW_ACTION_SCAN_CARTON_NO;
                }
                else
                {
                    throw new WesException("无法导入CNO, 因为还没有扫描箱号");
                }
            }
            else
            {
                return scanTarget;
            }
        }

        public bool IsScanCollectingCnoForFlow(string scanVal)
        {
            return Regex.IsMatch(scanVal, @"^COLLECTING:CNO$", RegexOptions.IgnoreCase);
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            base.SelfInfo.pid = null;
            base.SelfInfo.qrCode = null;
            base.SelfInfo.spn = null;
            base.SelfInfo.dc = null;
            base.SelfInfo.lot = null;
            base.SelfInfo.qty = null;
        }
    }
}