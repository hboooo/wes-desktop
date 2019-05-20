using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C04459")]
    public class C04459CollectingAction : CollectingAction
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

        protected override void ScanPartNo(string partNo)
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
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrPropertyName);
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "supplier")]
        public override void Save(string scanVal)
        {
            base.ScanCoo(scanVal);
            RestApi.NewInstance(Method.POST)
                .AddUri("collecting")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute();

            CommonLoadPackageInfo(Vm.SelfInfo.pid);

            Vm.SelfInfo.integralPid = (string) Vm.SelfInfo.pid;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrPropertyName);
        }
    }
}