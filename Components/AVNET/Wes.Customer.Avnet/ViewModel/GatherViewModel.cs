using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Avnet.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities.Extends;

/**
 * 采集
 */
namespace Wes.Customer.Avnet.ViewModel
{
    public class GatherViewModel : ScanViewModelBase<WesFlowID, GatherAction>, IViewModel
    {
        public ICommand DeleteCommand { get; private set; }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_IN_GATHER);
            teamSupport.ShowDialog();
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
        protected override GatherAction GetAction()
        {
            var gatherAction = ViewModelFactory.CreateActoin<GatherAction>() as GatherAction;
            if (DeleteCommand == null)
            {
                DeleteCommand = new RelayCommand<long>(gatherAction.DeleteData);
            }

            return gatherAction;
        }

        /**
         * 初始化所有扫描目标
         */
        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new {tooltip = "Package ID"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_MPN, new {tooltip = "MPN"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QRCODE, new {tooltip = "QrCode"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_ENTRY_DIM, new {tooltip = "Dimension"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_ENTRY_GW, new {tooltip = "GW"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_NIPPON, new {tooltip = "Nippon"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_BIN_NO, new {tooltip = "Bin No"});
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
            else
            {
                return scanTarget;
            }
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            base.SelfInfo.pid = null;
            base.SelfInfo.dim = null;
            base.SelfInfo.gw = null;
            base.SelfInfo.prePackageIsComplate = false;
        }
    }
}