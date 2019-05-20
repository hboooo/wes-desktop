using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Promaster.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C0000")]
    public class C0000CollectingAction:CollectingAction
    {
        public override void ScanQrCode(string qrCode)
        {
            base.ScanQrCode(qrCode);
            Save(qrCode);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN,PnOrQrPropertyName);
        }

        protected override void ScanPartNo(string partNo)
        {
            base.ScanPartNo(partNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO,LotPropertyName);
        }

        public override void ScanLotNo(string lot)
        {
            base.ScanLotNo(lot);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO,DcPropertyName);
        }

        public override void ScanDcNo(string dc)
        {
            base.ScanDcNo(dc);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY,QtyPropertyName);
        }

        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            Save(qty);
        }
    }
}