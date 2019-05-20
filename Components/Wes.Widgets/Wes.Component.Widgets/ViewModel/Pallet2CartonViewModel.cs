using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;
using Wes.Component.Widgets.Action;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows.Common;
using Wes.Flow;

namespace Wes.Component.Widgets.ViewModel
{
    public class Pallet2CartonViewModel : ScanViewModelBase<WesFlowID, Pallet2CartonAction>, IViewModel
    {
        public ICommand DeleteCommand { get; private set; }
        public ICommand PrintCommand { get; private set; }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO;
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_IN_PALLET_TO_CARTON);
            teamSupport.ShowDialog();
        }
        protected override Pallet2CartonAction GetAction()
        {
            var pallet2CartonAction = ViewModelFactory.CreateActoin<Pallet2CartonAction>() as Pallet2CartonAction;

            if (DeleteCommand == null)
            {
                DeleteCommand = new RelayCommand<string>(pallet2CartonAction.DeleteData);
            }
            if (PrintCommand == null)
            {
                PrintCommand = new RelayCommand<string>(pallet2CartonAction.ManualPrint);
            }
            return pallet2CartonAction;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO, new { tooltip = "Receiving NO" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PALLET_ID, new { tooltip = "Pallet ID" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_TOTAL_RECEIVE, new { tooltip = "Total of Carton" });
        }
        
    }
}
