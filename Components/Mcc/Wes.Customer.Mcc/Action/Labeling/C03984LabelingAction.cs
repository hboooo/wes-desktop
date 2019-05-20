using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Mcc.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C03984")]
    public class C03984LabelingAction : LabelingAction
    {
        //採集順序 P/N -> D/C -> QTY  無QRCode
        protected override string PnPropertyName => "P/N";
        protected override string DcPropertyName => "D/C";
        protected override string QtyPropertyName => "QTY";

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            this.Save(scanVal);
        }
    }
}