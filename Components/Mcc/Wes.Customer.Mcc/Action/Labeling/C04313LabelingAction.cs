using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Mcc.Action.Labeling

{
    [VirtualAction(("LabelingAction"), Type = "C04313")]
    public class C04313LabelingAction : LabelingAction
    {
        protected override string PnPropertyName => "DVC";
        protected override string QtyPropertyName => "Q'ty";
        protected override string LotPropertyName => "LOT No";

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            this.Save(scanVal);
        }

        public override void ScanLotNo(string scanVal)
        {
            base.ScanLotNo(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }
    }
}