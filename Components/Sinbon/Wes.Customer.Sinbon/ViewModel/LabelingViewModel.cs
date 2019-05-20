using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Sinbon.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities.Extends;

namespace Wes.Customer.Sinbon.ViewModel
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
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_LOADING_NO, new { tooltip = "Loading No"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new { tooltip = "Package ID" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PN_OR_QRCODE, new { tooltip = "QrCode ,ClCode or PN", tip = "QrCode ,ClCode or PN" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_COO, new { tooltip = "COO" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, new { tooltip = "Lot" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QTY, new { tooltip = "QTY" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_DC_NO, new { tooltip = "DC" });
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            base.SelfInfo.sxt = null;
            base.SelfInfo.pid = null;
            base.SelfInfo.shipper = null;
            base.SelfInfo.clCode = "";
            base.SelfInfo.lot = "";
            base.SelfInfo.lot = "";
            base.SelfInfo.printCount = "1";
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