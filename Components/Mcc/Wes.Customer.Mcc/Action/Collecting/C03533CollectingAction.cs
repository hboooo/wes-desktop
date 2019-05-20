using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C03533")]
    public class C03533CollectingAction : CollectingAction
    {
        //先扫pid，再扫cno，根据cno带出相关数据
        protected string CartonNo => "CNO";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, PnPropertyName);
        }

        public override void ScanBranch(string scanVal)
        {
            ScanSpn(scanVal);
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanLotNo(string scanVal)
        {
            base.ScanLotNo(scanVal);
            Save(scanVal);
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "supplier")]
        public override bool Save(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUri("collecting")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute();

            this.CommonLoadPackageInfo(Vm.SelfInfo.pid);

            Vm.SelfInfo.integralPid = (string) Vm.SelfInfo.pid;
            Vm.SelfInfo.dimGw = GetPartNo((string) Vm.SelfInfo.pn);
#if !DEBUG
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);            
#endif
            return true;
        }

        protected override dynamic CommonLoadPackageInfo(string scanVal)
        {
            dynamic result = base.CommonLoadPackageInfo(scanVal);
            if (result.spn != null)
            {
                Vm.SelfInfo.spn = result.spn;
            }

            if (result.pn != null)
            {
                Vm.SelfInfo.pn = result.pn;
            }

            if (result.lotNo != null)
            {
                Vm.SelfInfo.lot = result.lotNo;
            }

            if (result.lot != null)
            {
                Vm.SelfInfo.dt = result.dt;
            }

            if (result.dateCode != null)
            {
                Vm.SelfInfo.dc = result.dateCode;
            }

            if (result.originDc != null)
            {
                Vm.SelfInfo.originDc = result.originDc;
            }

            if (result.qty != null)
            {
                Vm.SelfInfo.qty = result.qty;
            }


            return result;
        }
    }
}