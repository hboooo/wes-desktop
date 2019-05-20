using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Collecting
{

    [VirtualAction(("CollectingAction"), Type = "C04313")]
    public class C04313CollectingAction : CollectingAction
    {
        protected string DvcPropertyName => "DVC";

        protected override string PnPropertyName => "Part No";

        protected override string QtyPropertyName => "Q'ty";
        protected override string DcPropertyName => "W/D";
        protected override string LotPropertyName => "LOT No";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, DvcPropertyName);
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);

        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanLotNo(string scanVal)
        {
            base.ScanLotNo(scanVal);
            //判断是LOT 中是否包含QTY 值，若包含，则下一步就不用再次跳入qty，而是进入dc 区分条件为lot值长度为20
            if (scanVal.Length >= 20)
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO,DcPropertyName);
            }
            else
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
            }


           
        }

        public override void ScanDcNo(string scanVal)
        {
            base.ScanDcNo(scanVal);
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