using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C04312")]
    public class C04312LabelingAction : LabelingAction
    {
        protected override string PnOrQrPropertyName => "P/N OR QrCode";
        protected override string LotPropertyName => "Lot No";
        protected override string QtyPropertyName => "Q'ty";
        protected override string DcPropertyName => "Date Code";

        public override void ScanQrCode(string scanVal)
        {
            base.ScanQrCode(scanVal);
            this.Save(scanVal);
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanLotNo(string scanVal)
        {
            base.ScanLotNo(scanVal);
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
    }
}