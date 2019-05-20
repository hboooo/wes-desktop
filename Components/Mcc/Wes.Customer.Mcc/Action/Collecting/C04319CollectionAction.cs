using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Collecting
{
    [VirtualAction(("CollectingAction"), Type = "C04319")]
    public class C04319CollectionAction : CollectingAction
    {
        //採集順序：(Q)QTY(PC) -> (M)MFR P/N -> (D)Date Code
        protected override string PnPropertyName => "(M)MFR P/N";
        protected override string DcPropertyName => "(D)Date Code";
        protected override string QtyPropertyName => "(Q)QTY(PC)";


        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, PnPropertyName);//掃描箱號后掃描QTY
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);//掃完QTY后掃PN 
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);//掃完PN后掃DC
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);//掃完DO后Save
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