using System.Collections.Generic;
using Wes.Component.Widgets.Action;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Flow;

namespace Wes.Component.Widgets.ViewModel
{
    public  class PickedInfoReportViewModel : ScanViewModelBase<WesFlowID, PickedInfoReportAction>, IViewModel
    {
        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_PICKING_NO;
        }

        protected override PickedInfoReportAction GetAction()
        {
            var pickedInfoAction = ViewModelFactory.CreateActoin<PickedInfoReportAction>() as PickedInfoReportAction;
      
            return pickedInfoAction;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO, new { tooltip = "Picking NO" });
        }
    }
}
