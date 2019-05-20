using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Promaster.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;

namespace Wes.Customer.Promaster.ViewModel
{
    public class PickDispatchingViewModel:ScanViewModelBase<WesFlowID, PickDispatchingAction>, IViewModel
    {
        public ICommand DeleteCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_PICKING_NO;
        }

        protected override PickDispatchingAction GetAction()
        {
            var sowAction = ViewModelFactory.CreateActoin<PickDispatchingAction>() as PickDispatchingAction;
            if (DeleteCommand == null)
            {
                DeleteCommand = new RelayCommand<long>(sowAction.DeleteData);
            }

            if (SearchCommand == null)
            {
                SearchCommand = new RelayCommand(sowAction.SearchData);
            }
            return sowAction;
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_PICKING_AND_SOW);
            teamSupport.ShowDialog();
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO, new { tooltip = "Picking NO" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO, new { tooltip = "Carton NO" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_COMMAND_OR_PN, new { tooltip = "Command OR PartNo" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_COMMAND, new { tooltip = "Command" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PN_OR_QRCODE, new { tooltip = "PN Or QrCode" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_DC_NO, new { tooltip = "Date Code" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, new { tooltip = "Lot NO" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QTY, new { tooltip = "Qty" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_FW, new { tooltip = "FW" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO, new { tooltip = "New Carton NO" });
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            if (this.ScanAction != null)
            {
                this.ScanAction.Command = WesScanCommand.NONE;
            }
        }

        public string _searchNPid;
        public string SearchNPid
        {
            get { return _searchNPid; }
            set
            {
                if (_searchNPid != value)
                {
                    _searchNPid = value;
                    RaisePropertyChanged<string>(nameof(SearchNPid));
                }
            }
        }

        private string _searchPN;
        public string SearchPN
        {
            get { return _searchPN; }
            set
            {
                if (_searchPN != value)
                {
                    _searchPN = value;
                    RaisePropertyChanged<string>(nameof(SearchPN));
                }
            }
        }
    }
}