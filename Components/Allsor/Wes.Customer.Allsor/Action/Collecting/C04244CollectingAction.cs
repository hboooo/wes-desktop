using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Allsor.Action.Collecting
{
    [VirtualAction(("CollectingAction"), Type = "C04244")]
    public class C04244CollectingAction : C03988CollectingAction
    {
        protected override string PnPropertyName => "Mfg P/N";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, "Mfg P/N");
        }
    }
}
