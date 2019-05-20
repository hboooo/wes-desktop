using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Allsor.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C04244")]
    public class C04244LabelingAction : C03988LabelingAction
    {
        protected override string PnPropertyName => "Mfg P/N";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, PnPropertyName);
        }
    }
}