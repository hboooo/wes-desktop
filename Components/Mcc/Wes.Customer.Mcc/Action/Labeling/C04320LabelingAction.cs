using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Mcc.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C04320")]
    public class C04320LabelingAction : LabelingAction
    {
        //採集順序：P/N -> Q'TY(PCS) ->LOT NO
        protected override string PnPropertyName => "P/N";
        protected override string DcPropertyName => "手key DateCode";
        protected override string QtyPropertyName => "Q'TY(PCS)";
        protected override string LotPropertyName => "LOT NO or Date Code";

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanLotNo(string scanVal)
        {
            base.ScanLotNo(scanVal);
            if (scanVal.Length == 15)
            {
                this.Save(scanVal);
            }
            else
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
            }
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            this.Save(scanVal);
        }
    }
}