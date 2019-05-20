using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Mcc.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C04314")]
    public class C04314LabelingAction : LabelingAction
    {
        protected override string PnOrQrPropertyName => "Product Number OR QrCode";
        protected override string QtyPropertyName => "Q'ty";
        protected override string DcPropertyName => "Date Code";

        public override void ScanQrCode(string scanVal)
        {
            base.ScanQrCode(scanVal);
            this.Save(scanVal);
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            this.Save(scanVal);
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanBranch(string scanVal)
        {
            if (scanVal.Length > 30)
            {
                this.ScanQrCode(scanVal);
            }
            else
            {
                this.ScanSpn(scanVal);
            }
        }
    }
}