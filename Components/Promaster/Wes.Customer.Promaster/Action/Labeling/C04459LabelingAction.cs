using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Labeling
{
    [VirtualAction("LabelingAction", Type = "C04459")]
    public class C04459LabelingAction : LabelingAction
    {
        private static string QrCodeOrDrvicePropertyName = "QrCode or DRVICE";
        protected override string DcPropertyName => "Date Code";
        protected override string LotPropertyName => "LOT ID";

        public override void ScanQrCode(string qrCode)
        {
            base.ScanQrCode(qrCode);
            Save(qrCode);
            //Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN,QrCodeOrDrvicePropertyName);
        }

        public override void ScanPartNo(string partNo)
        {
            base.ScanPartNo(partNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanLotNo(string lot)
        {
            base.ScanLotNo(lot);
            dynamic cooResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/coo")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.coo = cooResult.coo;
            Save(lot);
        }

        public override void ScanDcNo(string dc)
        {
            base.ScanDcNo(dc);
//            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY,QtyPropertyName);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, QrCodeOrDrvicePropertyName);
        }

        [AbilityAble(true, KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "consignee")]
        public override bool Save(string scanVal)
        {
            base.ScanCoo(scanVal);
            return base.Save(scanVal);
        }
    }
}