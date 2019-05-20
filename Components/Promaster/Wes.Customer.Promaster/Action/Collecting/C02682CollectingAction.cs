using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C02682")]
    public class C02682CollectingAction : CollectingAction
    {
        public static string MpnOrQrCodePropertyName = "MPN or QrCode";
        public static string CooPropertyName = "COO";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, MpnOrQrCodePropertyName);
        }

        protected override void ScanPartNo(string partNo)
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

        public virtual void ScanCoo(string coo)
        {
            base.ScanCoo(coo);
            Save(coo);
        }


        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO, CooPropertyName);
        }

        [AbilityAble(true, KPIActionType.LSDataCollection , "shipper")]
        public override void Save(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUri("collecting")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute();

            CommonLoadPackageInfo(Vm.SelfInfo.pid);

            Vm.SelfInfo.integralPid = (string) Vm.SelfInfo.pid;
            Vm.SelfInfo.dimGw = GetPartNo((string) Vm.SelfInfo.pn);

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, MpnOrQrCodePropertyName);
        }


        public override void ScanQrCode(string scanVal)
        {
            Vm.SelfInfo.qrCode = scanVal;
            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/qc")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.pn = qtyResult.pn;
            Vm.SelfInfo.dc = qtyResult.dc;
            Vm.SelfInfo.dt = qtyResult.dt;
            Vm.SelfInfo.lot = qtyResult.lot;
            Vm.SelfInfo.qty = qtyResult.qty;
            Vm.SelfInfo.coo = qtyResult.coo;
            Vm.SelfInfo.originDc = qtyResult.originDc;
            Vm.SelfInfo.fw = qtyResult.fw;
            isScanQrCode = true;
            Save(scanVal);
        }
    }
}