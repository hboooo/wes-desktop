using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Customer.Mcc.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C04316")]
    public class C04316LabelingAction : LabelingAction
    {
        //掃描順序：箱號->PN->DC->QTY
        protected override string PnPropertyName => "Part:No";
        protected override string DcPropertyName => "DateCode";
        protected override string QtyPropertyName => "Q'tY";

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName); //掃完PN掃DC
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName); //掃完DC掃QTY
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            this.Save(scanVal);
        }
    }
}