using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Collecting
{
     
    [VirtualAction(("CollectingAction"), Type = "C04315")]
    public class C04315CollectingAction : CollectingAction
    {
        protected override string PnPropertyName => "Part No";

        protected override string QtyPropertyName => "Q'ty";
        protected override string DcPropertyName => "Date Code";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, PnPropertyName);
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
            
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            this.Save(scanVal);
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "supplier")]
        public override bool Save(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
               .AddUri("collecting")
               .AddJsonBody(base.Vm.GetSelfInfo())
               .Execute();

            this.CommonLoadPackageInfo(base.Vm.SelfInfo.pid);

            base.Vm.SelfInfo.integralPid = (string)base.Vm.SelfInfo.pid;
            base.Vm.SelfInfo.dimGw = this.GetPartNo((string)base.Vm.SelfInfo.pn);
            #if !DEBUG
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);            
            #endif
            return true;
        }
    }
}