﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Allsor.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C03986")]
    public class C03986LabelingAction : LabelingAction
    {
        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, "Type");
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, "Lot");
        }

        public override void ScanLotNo(string scanVal)
        {
            base.ScanLotNo(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, "Qty");
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

        public override bool Save(string scanVal)
        {
            var result = base.Save(scanVal);
            #if !DEBUG
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, "Type");
            #endif
            return result;
        }
    }
}
