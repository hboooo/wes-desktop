using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C04460")]
    public class C04460CollectingAction : CollectingAction
    {
        private static string Code = "C04460";
        private static string FWPropertyName = "FW";
        private static string Origin = "Origin";
        private static string TraceCodePropertyName => "TRACE CODE";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO, Origin);
        }

        public virtual void ScanCoo(string coo)
        {
            base.ScanCoo(coo);

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrPropertyName);
        }

        protected override void ScanPartNo(string partNo)
        {
            Vm.SelfInfo.spn = partNo;
            dynamic partResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/spn")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.fw = "";
            Vm.SelfInfo.seriesNO = partResult.seriesNO;
            Vm.SelfInfo.spn = partResult.spn.ToString();
            Vm.SelfInfo.pn = partResult.partNo.ToString();
            Vm.SelfInfo.isNeedGw = (bool) partResult.isNeedGw;
            Vm.SelfInfo.qty = partResult.qty;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, TraceCodePropertyName);
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
            if (Vm.SelfInfo.seriesNO.ToString() == "Y")
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_FW, FWPropertyName);
            }
            else
            {
                Save(qty);
            }
        }

        public virtual void ScanFw(string fw)
        {
            Vm.SelfInfo.fw = fw;
            Vm.SelfInfo.supplier = Code;
            Vm.SelfInfo.isShipping = false;
            dynamic dcResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/fw")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.fw = dcResult.fw;
            Save(fw);
        }
    }
}