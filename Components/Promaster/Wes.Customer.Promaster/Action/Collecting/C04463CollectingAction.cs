using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C04463")]
    public class C04463CollectingAction : CollectingAction
    {
        protected virtual string PnPropertyName => "Model";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
        }

        protected override void ScanPartNo(string partNo)
        {
            base.ScanPartNo(partNo);
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
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
        }
    }
}