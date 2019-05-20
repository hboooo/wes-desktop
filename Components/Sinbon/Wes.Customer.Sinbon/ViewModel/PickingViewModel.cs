using System.Collections.Generic;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Sinbon.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;

namespace Wes.Customer.Sinbon.ViewModel
{
    /// <summary>
    /// 拣货
    /// </summary>
    public class PickingViewModel : ScanViewModelBase<WesFlowID, PickingAction>, IViewModel
    {
        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_PICKING_NO;
        }

        protected override PickingAction GetAction()
        {
            return ViewModelFactory.CreateActoin<PickingAction>() as PickingAction;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO, new { tooltip = "Picking NO" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, new { tooltip = "Package ID" });
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PART_NO, new { tooltip = "PartNo Or QRCode Or CLCode" });
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_PICKING);
            teamSupport.ShowDialog();
        }
    }
}
