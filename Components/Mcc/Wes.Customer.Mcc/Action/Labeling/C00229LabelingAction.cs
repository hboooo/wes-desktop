using System.Windows;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Utilities;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C00229")]
    public class C00229LabelingAction : LabelingAction
    {
        protected override string PnOrQrPropertyName => "Product Number";
        protected override string QtyPropertyName => "Q'ty";

        public override void ScanQrCode(string scanVal)
        {
            base.ScanQrCode(scanVal);
            this.Save(scanVal);
        }

        public override void ScanSpn(string scanVal)
        {
            base.Vm.SelfInfo.spn = scanVal;

            dynamic partResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/spn")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.spn = partResult.spn;
            base.Vm.SelfInfo.pn = partResult.partNo;
            base.Vm.SelfInfo.qty = partResult.qty;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                base.Vm.SelfInfo.useIntelligent = false;
            }));
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanQty(string scanVal)
        {
            base.ScanQty(scanVal);
            this.Save(scanVal);
        }

        public override void ScanLotNo(string scanVal)
        {
            base.Vm.SelfInfo.lot = scanVal;

            dynamic lotResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/lot")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.lot = lotResult.lot;
            if (lotResult.dc != null)
            {
                base.Vm.SelfInfo.dc = lotResult.dc;
                base.Vm.SelfInfo.dt = lotResult.dt;
                base.Vm.SelfInfo.originDc = lotResult.originDc;
                base.Vm.SelfInfo.formatDc = lotResult.formatDc;
            }

            if (!DynamicUtil.IsExist(base.Vm.SelfInfo, "qty") && DynamicUtil.IsExist(lotResult, "qty"))
            {
                base.Vm.SelfInfo.qty = lotResult.qty;
            }

            if (DynamicUtil.IsExist(base.Vm.SelfInfo, "qty") && base.Vm.SelfInfo.qty > 0)
            {
                this.Save(scanVal);
            }
            else
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
            }
        }
    }
}