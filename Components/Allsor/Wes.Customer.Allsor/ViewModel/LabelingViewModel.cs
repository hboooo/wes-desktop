using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Allsor.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Allsor.ViewModel
{
    public class LabelingViewModel : ScanViewModelBase<WesFlowID, LabelingAction>, IViewModel
    {
        public ICommand DeleteReelCommand { get; protected set; }
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
            return labelAction;
        }

        protected override string GetActionType(string scanValue)
        {
            if (scanValue.IsPackageID())
            {
                var supplier = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/shipping/single-supplier-by-pid")
                    .AddQueryParameter("pid", scanValue)
                    .Execute()
                    .To<string>();
                this.SelfInfo.supplier = supplier;
            }

            return this.SelfInfo.supplier;
        }

        protected override bool VirtualActionEnabled()
        {
            return true;
        }

        protected override void ResetAction(LabelingAction scanAction)
        {
            this.DeleteReelCommand = new RelayCommand<dynamic>(scanAction.DeleteReelLabel);
            this.DeleteBoxCommand = new RelayCommand<dynamic>(scanAction.DeleteBoxLabel);
            this.ReprintCommand = new RelayCommand<long>(scanAction.Reprint);
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