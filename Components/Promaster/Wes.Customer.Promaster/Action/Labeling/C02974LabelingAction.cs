using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Promaster.Action.Labeling
{
    [VirtualAction("LabelingAction", Type = "C02974")]
    public class C02974LabelingAction : LabelingAction
    {
        private static string PnOrQrCodePropertyName = "QrCode or P/N";
        protected override string DcPropertyName => "D/C";

        public override void ScanQrCode(string qrCode)
        {
            base.ScanQrCode(qrCode);
            Save(qrCode);
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

        public override void ScanDcNo(string dc)
        {
            base.ScanDcNo(dc);
            Save(dc);
        }

        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrCodePropertyName);
        }

        [AbilityAble(true, KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "consignee")]
        public override bool Save(string scanVal)
        {
            base.ScanCoo(scanVal);
            return base.Save(scanVal);
        }
    }
}