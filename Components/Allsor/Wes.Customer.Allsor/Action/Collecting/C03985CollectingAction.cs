﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Allsor.Action.Collecting
{
    [VirtualAction(("CollectingAction"), Type = "C03985")]
    public class C03985CollectingAction : CollectingAction
    {
        protected override string PnPropertyName => "PN";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, "Pn");
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, "Date");
        }

        public override void ScanLotNo(string scanVal)
        {
            base.ScanLotNo(scanVal);
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, "Qty");
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            this.Save(scanVal);
        }
    }
}
