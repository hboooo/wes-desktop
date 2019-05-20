using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Promaster.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C00000")]
    public class C00000LabelingAction : LabelingAction
    {
        public override void ScanQrCode(string qrCode)
        {
            base.ScanQrCode(qrCode);
            base.Save(qrCode);
        }

        public override void ScanPartNo(string partNo)
        {
            base.ScanPartNo(partNo);
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
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            base.Save(qty);
        }
    }
}