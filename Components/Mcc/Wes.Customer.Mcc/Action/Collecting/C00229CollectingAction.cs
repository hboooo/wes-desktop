using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C00229")]
    public class C00229CollectingAction : CollectingAction
    {
        protected override string PnPropertyName => "Part No";
        protected override string PnOrQrPropertyName => "Product Number OR QrCode";
        protected override string QtyPropertyName => "Q'ty";
        protected override string DcPropertyName => "Date Code";
        protected override string LotPropertyName => "LOT或LOT/QTY";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrPropertyName);
        }

        public override void ScanQrCode(string scanVal)
        {
            base.ScanQrCode(scanVal);
            this.Save(scanVal);
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.Vm.SelfInfo.lot = base.Vm.SelfInfo.lot + "/" + scanVal;
            this.ScanLotNo(base.Vm.SelfInfo.lot);
        }

        public override void ScanLotNo(string scanVal)
        {
            if (scanVal.Contains("/"))
            {
                base.ScanLotNo(scanVal);
                this.Save(scanVal);
            }
            else if (base.Vm.SelfInfo.qty == null)
            {
                base.Vm.SelfInfo.lot = scanVal;
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
            }
            else
            {
                base.ScanLotNo(scanVal);
                this.Save(scanVal);
            }
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "supplier")]
        public override bool Save(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUri("collecting")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute();

            this.CommonLoadPackageInfo(base.Vm.SelfInfo.pid);
            base.Vm.SelfInfo.integralPid = (string) base.Vm.SelfInfo.pid;
            base.Vm.SelfInfo.dimGw = this.GetPartNo((string) base.Vm.SelfInfo.pn);
            #if !DEBUG
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrPropertyName);
            #endif
            return true;
        }

        public override void ScanBranch(string scanVal)
        {
            if (scanVal.Contains(";"))
            {
                this.ScanQrCode(scanVal);
            }
            else
            {
                this.ScanSpn(scanVal);
            }
        }
    }
}