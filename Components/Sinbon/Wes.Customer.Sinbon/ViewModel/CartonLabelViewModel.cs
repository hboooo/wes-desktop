using System.Collections.Generic;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Sinbon.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;

namespace Wes.Customer.Sinbon.ViewModel
{
    public class CartonLabelViewModel : ScanViewModelBase<WesFlowID, CartonLabelAction>, IViewModel
    {
        protected override CartonLabelAction GetAction()
        {
            return ViewModelFactory.CreateActoin<CartonLabelAction>() as CartonLabelAction;
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_CARTON_LABELING);
            teamSupport.ShowDialog();
        }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_LOADING_NO;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_LOADING_NO, new { tooltip = "Truck Order" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new { tooltip = "Package ID" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_ENTRY_GW, new { tooltip = "GW" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_ENTRY_DIM, new { tooltip = "Dimension" });
        }
    }
}
