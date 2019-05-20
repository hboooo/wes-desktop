using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.FitiPower.Action;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.FitiPower.ViewModel
{
    public class StorageMoveCollectingViewModel : ScanViewModelBase<WesFlowID, StorageMoveCollectingAction>, IViewModel
    {
        public ICommand DeleteCommand { get; private set; }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO;
        }

        protected override StorageMoveCollectingAction GetAction()
        {
            var action = ViewModelFactory.CreateActoin<StorageMoveCollectingAction>() as StorageMoveCollectingAction;
            if (DeleteCommand == null)
            {
                DeleteCommand = new RelayCommand<dynamic>(action.DeletePackage);
            }

            return action;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO,
                new {tooltip = "Receiving No", tip = "Receiving No"});
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new {tooltip = "Package Id", tip = "Package Id"});
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            base.SelfInfo.rxt = null;
            base.SelfInfo.pid = null;
        }

        protected override WesFlowID ScanInterceptor(string scanVal, WesFlowID scanTarget)
        {
            if (scanVal.IsRxt())
            {
                return WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO;
            }
            else if (scanVal.IsPackageID())
            {
                return WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID;
            }
            else
            {
                return scanTarget;
            }
        }
       
       
    }
}