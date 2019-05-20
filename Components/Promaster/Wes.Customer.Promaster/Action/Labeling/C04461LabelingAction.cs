using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Promaster.Action.Labeling
{
    [VirtualAction("LabelingAction", Type = "C04461")]
    public class C04461LabelingAction : LabelingAction
    {
        private static string PnPropertyName => "Model Name";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
        }

        public override void ScanPartNo(string partNo)
        {
            base.ScanPartNo(partNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanLotNo(string lot)
        {
            base.ScanLotNo(lot);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanDcNo(string dc)
        {
            base.ScanDcNo(dc);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO, CooPropertyName);
        }

        public override void ScanCoo(string coo)
        {
            base.ScanCoo(coo);
            Save(coo);
        }
    }
}