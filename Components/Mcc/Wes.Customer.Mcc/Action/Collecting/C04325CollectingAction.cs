using System;
using Wes.Core.VirtualAction;
using Wes.Customer.Mcc.ViewModel;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action.Collecting
{
    [VirtualAction("CollectingAction", Type = "C04325")]
    public class C04325CollectingAction : CollectingAction
    {
        public override void ScanPackageId(string scanVal)
        {
            base.ScanPackageId(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO, "CNO");
        }

        public void ScanCartonNo(string scanVal)
        {
            if ((Vm as CollectingViewModel).IsScanCollectingCnoForFlow(scanVal))
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO, "CNO");
            }
            else
            {
                Vm.SelfInfo.cno = scanVal;
                dynamic data = RestApi.NewInstance(Method.POST)
                    .AddUri("collecting/scan-cno")
                    .AddParams(Vm.GetSelfInfo())
                    .Execute()
                    .To<object>();
                var supplier = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/receiving/supplier-by-part-no")
                    .AddParams("partNo", data.partNo)
                    .Execute()
                    .To<string>();
                Vm.SelfInfo.supplier = supplier;
                Vm.SelfInfo.pcsGw = data.gw;
                Vm.SelfInfo.pcsSize = data.size;
                this.ScanSpn(Convert.ToString(data.partNo));
                this.ScanDcNo(Convert.ToString(data.dateCode));
                this.ScanQty(Convert.ToString(data.qty));
                Save(scanVal);
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            }
        }

        /*public override void ScanSpn(string scanVal)
        {
            Vm.SelfInfo.spn = scanVal;
            base.ScanSpn(scanVal);
            dynamic data = RestApi.NewInstance(Method.POST)
                .AddUri("collecting/scan-pn")
                .AddJsonBody("pn", scanVal)
                .Execute()
                .To<object>();
            var supplier = data.shipper.ToString();
            Vm.ReinitializeAction(supplier,
                "pid", Vm.SelfInfo.pid,
                "spn", Vm.SelfInfo.spn,
                "supplier", supplier
            );
        }*/
    }
}