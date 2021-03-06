using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Promaster.Action.Labeling
{
    [VirtualAction("LabelingAction", Type = "C04464")]
    public class C04464LabelingAction : LabelingAction
    {
        private static string PnOrQrPropertyName => "PN or QrCode";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrPropertyName);
        }

        public override void ScanPartNo(string partNo)
        {
            base.ScanPartNo(partNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanLotNo(string lot)
        {
            base.ScanLotNo(lot);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanDcNo(string dc)
        {
            base.ScanDcNo(dc);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO, CooPropertyName);
        }

        public override void ScanCoo(string coo)
        {
            base.ScanCoo(coo);
            Save(coo);
        }

        public override void ScanQrCode(string qrCode)
        {
            base.ScanQrCode(qrCode);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }
    }
}