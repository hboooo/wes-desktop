using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Avnet.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;

namespace Wes.Customer.Avnet.ViewModel
{
    public class SowViewModel : ScanViewModelBase<WesFlowID, SowAction>, IViewModel
    {
        public ICommand DeleteCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_PICKING_NO;
        }

        protected override SowAction GetAction()
        {
            var sowAction = ViewModelFactory.CreateActoin<SowAction>() as SowAction;
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
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_SOW);
            teamSupport.ShowDialog();
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO, new { tooltip = "Picking NO" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_MPN, new { tooltip = "MPN" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_QRCODE, new { tooltip = "QrCode" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO, new { tooltip = "New Carton NO" });
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
