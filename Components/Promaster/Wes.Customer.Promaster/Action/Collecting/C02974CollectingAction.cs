using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C02974")]
    public class C02974CollectingAction : CollectingAction
    {
        private static string PnOrQrCodePropertyName = "QrCode or P/N";
        protected override string DcPropertyName => "D/C";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrCodePropertyName);
        }

        protected override void ScanPartNo(string partNo)
        {
            Vm.SelfInfo.spn = partNo;

            dynamic partResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/spn")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.spn = partResult.spn.ToString();
            Vm.SelfInfo.pn = partResult.partNo.ToString();
            Vm.SelfInfo.isNeedGw = (bool) partResult.isNeedGw;
            Vm.SelfInfo.qty = partResult.qty;
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
            Save(dc);
        }

        public override void ScanQrCode(string qrCode)
        {
            base.ScanQrCode(qrCode);
            Save(qrCode);
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
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrCodePropertyName);
        }
    }
}