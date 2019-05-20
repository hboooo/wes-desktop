using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Allsor.Action.Collecting
{
    [VirtualAction(("CollectingAction"), Type = "C03986")]
    public class C03986CollectingAction : CollectingAction
    {
        protected override string PnPropertyName => "Type";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, "Type");
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, "Qty");
        }

        public override void ScanLotNo(string scanVal)
        {
            base.ScanLotNo(scanVal);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, "Dc");
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            this.Save(scanVal);
        }
    }
}
