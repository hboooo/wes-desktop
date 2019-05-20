using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Collecting
{
    [VirtualAction(("CollectingAction"), Type = "C04320")]
    public class C04320CollectionAction : CollectingAction
    {
        //採集順序：P/N -> Q'TY(PCS) ->LOT NO
        protected override string PnPropertyName => "P/N";
        protected override string DcPropertyName => "Date Code";
        protected override string QtyPropertyName => "Q'TY(PCS)";
        protected override string LotPropertyName => "LOT NO";


        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_SPN, PnPropertyName);
        }

        public override void ScanSpn(string scanVal)
        {
            base.ScanSpn(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanLotNo(string scanVal)
        {
            
            base.ScanLotNo(scanVal);
            if (base.Vm.SelfInfo.dc !=null&& base.Vm.SelfInfo.dt != null && base.Vm.SelfInfo.originDc != null)
            {
                this.Save(scanVal);
            }else
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
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