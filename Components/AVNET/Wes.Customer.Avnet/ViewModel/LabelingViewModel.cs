using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Avnet.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Avnet.ViewModel
{
    public class LabelingViewModel : ScanViewModelBase<WesFlowID, LabelingAction>, IViewModel
    {
        public ICommand DeletePlateCommand { get; protected set; }
        public ICommand DeleteBoxCommand { get; protected set; }
        public ICommand ReprintCommand { get; protected set; }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_TRUCK_NO;
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_LABELING);
            teamSupport.ShowDialog();
        }

        protected override string GetActionType(string scanValue)
        {
            if (scanValue.IsPackageID())
            {
                var supplierList = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/shipping/supplier-by-pid")
                    .AddQueryParameter("pid", scanValue)
                    .Execute()
                    .To<List<dynamic>>();
                SelfInfo.supplier = supplierList[0].shipper.ToString();
            }

            return SelfInfo.supplier;
        }

        protected override LabelingAction GetAction()
        {
            var labelAction = ViewModelFactory.CreateActoin<LabelingAction>() as LabelingAction;
            DeletePlateCommand = new RelayCommand<dynamic>(labelAction.DeletePlateLabel);
            DeleteBoxCommand = new RelayCommand<dynamic>(labelAction.DeleteBoxLabel);
            ReprintCommand = new RelayCommand<long>(labelAction.Reprint);
            return labelAction;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_TRUCK_NO, new {tooltip = "Truck Order"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new {tooltip = "Package ID"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_MPN, new {tooltip = "MPN"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QRCODE, new {tooltip = "QrCode"});
        }

        protected override void ResetAction(LabelingAction scanAction)
        {
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            SelfInfo.txt = null;
            SelfInfo.pid = null;
        }

        protected override WesFlowID ScanInterceptor(string scanVal, WesFlowID scanTarget)
        {
            if (scanVal.IsPackageID())
            {
                return WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID;
            }
            else if (scanVal.IsTxt())
            {
                return WesFlowID.FLOW_ACTION_SCAN_TRUCK_NO;
            }
            else
            {
                return scanTarget;
            }
        }
    }
}