using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.VirtualAction;
using Wes.Flow;

namespace Wes.Customer.Allsor.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C03989")]
    public class C03989LabelingAction : LabelingAction
    {
        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, "Pn or Qrcode");
        }

        public override void ScanBranch(string scanVal)
        {
            if (scanVal.Length >= 20 && scanVal.Contains("-"))
            {
                this.ScanQrCode(scanVal);
            }
            else
            {
                this.ScanSpn(scanVal);
            }
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
            this.Save(scanVal);
        }

        public override void ScanQrCode(string scanVal)
        {
            base.ScanQrCode(scanVal);
            this.Save(scanVal);
        }

        public override bool Save(string scanVal)
        {
            var result = base.Save(scanVal);
            #if !DEBUG
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, "Pn or Qrcode");
            #endif
            return result;
        }
    }
}
