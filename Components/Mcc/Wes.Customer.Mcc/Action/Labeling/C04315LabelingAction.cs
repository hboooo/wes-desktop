using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Mcc.Action.Labeling


{
    [VirtualAction(("LabelingAction"), Type = "C04315")]
    public class C04315LabelingAction : LabelingAction
    {
        protected override string PnPropertyName => "Part No";
        protected override string QtyPropertyName => "Q'ty";
        protected override string DcPropertyName => "Date Code";

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            this.Save(scanVal);
        }
    }
}