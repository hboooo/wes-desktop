using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Mcc.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C04319")]
    public class C04319LabelingAction : LabelingAction
    {
        //採集順序：(Q)QTY(PC) -> (M)MFR P/N -> (D)Date Code
        protected override string PnPropertyName => "(M)MFR P/N";
        protected override string DcPropertyName => "(D)Date Code";
        protected override string QtyPropertyName => "(Q)QTY(PC)";

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, PnPropertyName); //掃完QTY后掃PN 
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName); //掃完PN后掃DC
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal); //掃完DO后Save
            this.Save(scanVal);
        }
    }
}