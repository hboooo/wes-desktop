using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Customer.Mcc.Action.Collecting
{
    [VirtualAction(("CollectingAction"), Type = "C04316")]
    public class C04316CollectionAction : CollectingAction
    {
        //掃描順序：箱號->PN->DC->QTY
        protected override string PnPropertyName => "Part:No";
        protected override string DcPropertyName => "DateCode";
        protected override string QtyPropertyName => "Q'tY";


        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, PnPropertyName); //掃完箱號掃PN
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName); //掃完PN掃DC
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName); //掃完DC掃QTY
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            this.Save(scanVal);
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "supplier")]
        public override bool Save(string scanVal)
        {
            var save = base.Save(scanVal);
            #if !DEBUG
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrPropertyName);
            #endif
            return save;
        }
    }
}