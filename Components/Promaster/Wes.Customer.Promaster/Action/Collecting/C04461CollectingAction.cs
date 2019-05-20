using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Collecting
{
    /// 針對supplier = C04461，正確順序應該是COO、Model Name、LOT、DC、QTY
    /// 
    [VirtualAction("CollectingAction", Type = "C04461")]
    public class C04461CollectingAction : CollectingAction
    {
        protected virtual string PnPropertyName => "Model Name";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
//            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO, CooPropertyName);
        }

        protected override void ScanPartNo(string partNo)
        {
            base.ScanPartNo(partNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
//            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
            Save(qty);
        }

        public override void ScanLotNo(string lot)
        {
            base.ScanLotNo(lot);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanDcNo(string dc)
        {
            base.ScanDcNo(dc);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanCoo(string coo)
        {
            base.ScanCoo(coo);
//            Save(coo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
        }
    }
}