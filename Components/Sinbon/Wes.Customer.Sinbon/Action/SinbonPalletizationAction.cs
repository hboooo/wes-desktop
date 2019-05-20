using System;
using System.Collections.Generic;
using Wes.Component.Widgets.Action;
using Wes.Desktop.Windows;
using Wes.Print;
using Wes.Utilities;
using Wes.Wrapper;

namespace Wes.Customer.Sinbon.Action
{
    public class SinbonPalletizationAction : PalletizationAction
    {
        public override bool CartonEnd(string scanVal)
        {
            if (!DynamicUtil.IsExist(base.Vm.SelfInfo, "doingPackages") || base.Vm.SelfInfo.doingPackages == null || base.Vm.SelfInfo.doingPackages.Length == 0)
            {
                WesModernDialog.ShowWesMessage("未掃描任何packageId");
                return false;
            }

            Vm.SelfInfo.kpiPackages = DynamicJson.DeserializeObject<dynamic>(DynamicJson.SerializeObject(Vm.SelfInfo.doingPackages));

            bool result = RestApi.NewInstance(Method.PUT)
                            .AddUri("palletization/cartonend")
                            .AddJsonBody("truckOrder", base.Vm.SelfInfo.truckOrder)
                            .AddJsonBody("endCustomer", base.Vm.SelfInfo.endCustomer)
                            .AddJsonBody("doingPackages", base.Vm.SelfInfo.doingPackages)
                            .Execute()
                            .ToBoolean();
            if (result)
            {
                var labelInfo = RestApi.NewInstance(Method.POST)
                           .AddUri("palletization/carton-label")
                           .AddJsonBody("truckOrder", base.Vm.SelfInfo.truckOrder)
                           .AddJsonBody("doingPackages", base.Vm.SelfInfo.doingPackages)
                           .Execute()
                           .To<object>();


                List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
                int labelCount = labelInfo.Count;
                for (int i = 0; i < labelCount; i++)
                {
                    var item = labelInfo[i];
                    PrintTemplateModel barTenderModel = new PrintTemplateModel();
                    barTenderModel.PrintData = item;
                    barTenderModel.PrintData.CtnOfPltCount = (i + 1).ToString() + "/" + labelCount;
                    barTenderModel.TemplateFileName = "SINBON_PalletEnd_Pallet.btw";
                    templates.Add(barTenderModel);
                }

                LabelPrintBase labelPrint = new LabelPrintBase(templates, false);
                labelPrint.Print();

                PrepareData();
                return true;
            }

            return false;
        }
        
        public override bool PalletEnd(string scanVal)
        {
            if (!DynamicUtil.IsExist(base.Vm.SelfInfo, "doingPackages"))
            {
                WesModernDialog.ShowWesMessage("未掃描任何packageId");
                return false;
            }

            if (base.Vm.SelfInfo.doingPackages == null || base.Vm.SelfInfo.doingPackages.Length == 0)
            {
                WesModernDialog.ShowWesMessage("未掃描任何packageId");
                return false;
            }

            var result = RestApi.NewInstance(Method.PUT)
                            .AddUri("palletization/palletend")
                            .AddJsonBody("truckOrder", base.Vm.SelfInfo.truckOrder)
                            .AddJsonBody("endCustomer", base.Vm.SelfInfo.endCustomer)
                            .AddJsonBody("doingPackages", base.Vm.SelfInfo.doingPackages)
                            .Execute()
                            .To<Object>();
            if ((bool)result.state)
            {
                Vm.SelfInfo.kpiPackages = DynamicJson.DeserializeObject<dynamic>(DynamicJson.SerializeObject(Vm.SelfInfo.doingPackages));

                this.Vm.SelfInfo.palletNo = result.palletNo;

                var labelInfo = RestApi.NewInstance(Method.GET)
                            .AddUri("palletization/pallet-label")
                            .AddQueryParameter("truckOrder", base.Vm.SelfInfo.truckOrder)
                            .AddQueryParameter("palletNo", base.Vm.SelfInfo.palletNo)
                            .Execute()
                            .To<object>();
                if (labelInfo != null && labelInfo.Count > 0)
                {
                    //打印板標籤
                    List<PrintTemplateModel> templates = new List<PrintTemplateModel>();

                    PrintTemplateModel pm = new PrintTemplateModel();
                    pm.PrintData = labelInfo[0];
                    pm.TemplateFileName = "SINBON_PalletEnd_Pallet.btw";
                    templates.Add(pm);

                    LabelPrintBase lpb = new LabelPrintBase(templates, false);
                    var res = lpb.Print();

                    PrepareData();
                    return true;
                }
            }
            return false;
        }
    }
}
