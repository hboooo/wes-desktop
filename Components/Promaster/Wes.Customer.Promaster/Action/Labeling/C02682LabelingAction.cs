using Wes.Core.Attribute;
using Wes.Core.VirtualAction;
using Wes.Customer.Promaster.ViewModel;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action.Labeling
{
    [VirtualAction(("LabelingAction"), Type = "C02682")]
    public class C02682LabelingAction : LabelingAction
    {
        public static string MpnOrQrCodePropertyName = "MPN or QrCode";
        public static string CooPropertyName = "COO";

        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            //针对COO采集顺序调整做出的代码调整
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO, CooPropertyName);
        }

        public override void ScanPartNo(string partNo)
        {
            base.ScanPartNo(partNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO, LotPropertyName);
        }

        public override void ScanLotNo(string lot)
        {
            base.ScanLotNo(lot);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY, QtyPropertyName);
        }

        public override void ScanDcNo(string dc)
        {
            base.ScanDcNo(dc);
            Save(dc);
        }

        public virtual void ScanCoo(string coo)
        {
            base.ScanCoo(coo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, MpnOrQrCodePropertyName);
        }


        public override void ScanQty(string qty)
        {
            base.ScanQty(qty);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO, DcPropertyName);
        }

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
            Save(qrCode);
        }

        [AbilityAble(true, KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "consignee")]
        public override bool Save(string scanVal)
        {
            if ((bool) Vm.SelfInfo.isLabeling)
            {
                //保存并检查
                SaveCheckingAndLabeling(scanVal);
            }
            else
            {
                SaveChecking(scanVal);
            }

            if (LabelingViewModel.IsMoreSupplier)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOCAL_SPN, "该箱有多个供应商, 请先扫描料号确定供应商");
            }
            else
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO, CooPropertyName);
            }

            return true;
        }
    }
}