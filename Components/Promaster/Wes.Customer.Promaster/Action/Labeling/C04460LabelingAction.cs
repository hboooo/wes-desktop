using System;
using System.Windows;
using Wes.Core.VirtualAction;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Labeling
{
    [VirtualAction("LabelingAction", Type = "C04460")]
    public class C04460LabelingAction : LabelingAction
    {
        private static string Code = "C04460";
        private static string FWPropertyName = "FW";
        private static string Origin = "Origin";
        protected bool isScanQrCode = false;
        private static string TraceCodePropertyName => "TRACE CODE";

        public override void ScanQrCode(string qrCode)
        {
            Vm.SelfInfo.qrCode = qrCode;

            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/qc")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.pn = qtyResult.pn;
            Vm.SelfInfo.dc = qtyResult.dc;
            Vm.SelfInfo.dt = qtyResult.dt;
            Vm.SelfInfo.lot = qtyResult.lot;
            Vm.SelfInfo.qty = qtyResult.qty;
            Vm.SelfInfo.originDc = qtyResult.originDc;
            Vm.SelfInfo.seriesNO = qtyResult.seriesNO;

            isScanQrCode = true;
            if (Vm.SelfInfo.seriesNO != null &&
                Vm.SelfInfo.seriesNO.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_FW_NEW, FWPropertyName);
            }
            else
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
            }

            Save(qrCode);
        }

        public override void ScanPartNo(string partNo)
        {
            Vm.SelfInfo.spn = partNo;
            dynamic partResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/spn")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.spn = partResult.spn;
            Vm.SelfInfo.pn = partResult.partNo;
            Vm.SelfInfo.seriesNO = partResult.seriesNO;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                Vm.SelfInfo.useIntelligent = false;
            }));
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanLotNo(string lot)
        {
            base.ScanLotNo(lot);
            if (Vm.SelfInfo.seriesNO != null &&
                string.Equals("Y", (string)Vm.SelfInfo.seriesNO, StringComparison.OrdinalIgnoreCase))
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_FW, FWPropertyName);
            }
            else
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
            }
        }

        public override void ScanDcNo(string dc)
        {
            base.ScanDcNo(dc);
            Save(dc);
        }

        public virtual void ScanCoo(string coo)
        {
            base.ScanCoo(coo);
            Save(coo);
        }

        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, TraceCodePropertyName);
        }

        public virtual void ScanFw(string fw)
        {
            Vm.SelfInfo.fw = fw;
            Vm.SelfInfo.isShipping = true;
            Vm.SelfInfo.supplier = Code;
            dynamic dcResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/fw")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.fw = dcResult.fw;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
        }
    }
}