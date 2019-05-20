using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.FitiPower.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities.Extends;

/**
 * 采集
 */
namespace Wes.Customer.FitiPower.ViewModel
{
    public class CollectingViewModel : ScanViewModelBase<WesFlowID, CollectingAction>, IViewModel
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
        protected override CollectingAction GetAction()
        {
            var gatherAction = ViewModelFactory.CreateActoin<CollectingAction>() as CollectingAction;
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
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new {tooltip = "Package Id", tip = "Package Id" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_MPN, new { tooltip = "Mpn", tip = "Product Name" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QRCODE, new { tooltip = "QrCode", tip = "QrCode" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QTY, new { tooltip = "Qty", tip = "Qty" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, new { tooltip = "Lot", tip = "Lot No" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_DC_NO, new { tooltip = "Dc", tip = "Date Code" });
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
            base.SelfInfo.isQrcodeQty = false;
        }
    }
}