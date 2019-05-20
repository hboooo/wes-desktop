using System.Collections.Generic;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Allsor.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities.Extends;

namespace Wes.Customer.Allsor.ViewModel
{
    public class LabelingCartonViewModel : ScanViewModelBase<WesFlowID, LabelingCartonAction>, IViewModel
    {
        protected override LabelingCartonAction GetAction()
        {
            return ViewModelFactory.CreateActoin<LabelingCartonAction>() as LabelingCartonAction;
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
