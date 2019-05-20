using System.Collections.Generic;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Desktop.Windows
{
    public class WorkNoFlowService
    {
        /// <summary>
        /// 作業單號對應的流程ID
        /// </summary>
        /// <param name="workNo"></param>
        /// <returns></returns>
        public static HashSet<WesFlowID> GetWorkNoFlow(string workNo)
        {
            HashSet<WesFlowID> items = new HashSet<WesFlowID>();

            if (string.Compare(WesDesktop.Instance.AddInName, "avnet", true) == 0)
            {
                if (workNo.IsPackageID())
                {
                    items.Add(WesFlowID.FLOW_IN_GATHER);
                }
                else if (workNo.IsRxt())
                {
                    items.Add(WesFlowID.FLOW_IN_PALLET_TO_CARTON);
                }
                else if (workNo.IsTxt())
                {
                    items.Add(WesFlowID.FLOW_OUT_LABELING);
                    items.Add(WesFlowID.FLOW_OUT_CARTON_LABELING);
                    items.Add(WesFlowID.FLOW_OUT_BOARDING);
                    items.Add(WesFlowID.FLOW_OUT_TRANSPORTATION_LABELING);
                }
                else if (workNo.IsPxt())
                {
                    items.Add(WesFlowID.FLOW_OUT_PICKING);
                    items.Add(WesFlowID.FLOW_OUT_PICKING_AND_SOW);
                    items.Add(WesFlowID.FLOW_OUT_SOW);
                    items.Add(WesFlowID.FLOW_OUT_PICKEDINFO_REPORT);
                }
            }
            else
            {
                if (workNo.IsPackageID())
                {
                    items.Add(WesFlowID.FLOW_IN_GATHER);
                }
                else if (workNo.IsRxt())
                {
                    items.Add(WesFlowID.FLOW_IN_PALLET_TO_CARTON);
                    items.Add(WesFlowID.FLOW_IN_STORAGE_MOVE_COLLECTING);
                }
                else if (workNo.IsSxt())
                {
                    items.Add(WesFlowID.FLOW_OUT_LABELING);
                    items.Add(WesFlowID.FLOW_OUT_CARTON_LABELING);
                    items.Add(WesFlowID.FLOW_OUT_BOARDING);
                }
                else if (workNo.IsTxt())
                {
                    items.Add(WesFlowID.FLOW_OUT_BOARDING);
                    items.Add(WesFlowID.FLOW_OUT_TRANSPORTATION_LABELING);
                }
                else if (workNo.IsPxt())
                {
                    items.Add(WesFlowID.FLOW_OUT_PICKING);
                    items.Add(WesFlowID.FLOW_OUT_PICKING_AND_SOW);
                    items.Add(WesFlowID.FLOW_OUT_SOW);
                    items.Add(WesFlowID.FLOW_OUT_PICKEDINFO_REPORT);
                }
            }
            return items;
        }

    }
}
