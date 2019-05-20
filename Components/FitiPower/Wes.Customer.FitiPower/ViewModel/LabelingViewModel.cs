using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.FitiPower.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities.Extends;

namespace Wes.Customer.FitiPower.ViewModel
{
    public class LabelingViewModel : ScanViewModelBase<WesFlowID, LabelingAction>, IViewModel
    {
        public ICommand DeletePlateCommand { get; protected set; }
        public ICommand DeleteBoxCommand { get; protected set; }
        public ICommand ReprintCommand { get; protected set; }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_LOADING_NO;
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_LABELING);
            teamSupport.ShowDialog();
        }

        protected override LabelingAction GetAction()
        {
            var labelAction = ViewModelFactory.CreateActoin<LabelingAction>() as LabelingAction;
            this.DeletePlateCommand = new RelayCommand<dynamic>(labelAction.DeletePlateLabel);
            this.DeleteBoxCommand = new RelayCommand<dynamic>(labelAction.DeleteBoxLabel);
            this.ReprintCommand = new RelayCommand<long>(labelAction.Reprint);

            return labelAction;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_LOADING_NO, new { tooltip = "Sxt" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new { tooltip = "Package Id" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_MPN, new { tooltip = "Mpn", tip = "Pn Or QrCode" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QRCODE, new { tooltip = "QrCode", tip = "QrCode" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QTY, new { tooltip = "Qty", tip = "Qty" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, new { tooltip = "Lot", tip = "Lot No" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_DC_NO, new { tooltip = "Dc", tip = "Date Code" });
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            base.SelfInfo.txt = null;
            base.SelfInfo.pid = null;
        }

        protected override WesFlowID ScanInterceptor(string scanVal, WesFlowID scanTarget)
        {
            if (scanVal.IsPackageID())
            {
                return WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID;
            }
            else if (scanVal.IsSxt())
            {
                return WesFlowID.FLOW_ACTION_SCAN_LOADING_NO;
            }
            else
            {
                return scanTarget;
            }
        }
    }
}
