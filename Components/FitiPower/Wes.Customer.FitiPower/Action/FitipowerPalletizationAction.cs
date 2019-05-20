using System;
using System.Collections.Generic;
using Wes.Component.Widgets.Action;
using Wes.Core.Attribute;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities;
using Wes.Wrapper;

namespace Wes.Customer.FitiPower.Action
{
    public class FitipowerPalletizationAction : PalletizationAction
    {
        private readonly int _specialPalletRulesThreshold = 12;
        /// <summary>
        /// 12箱以下只能使用cartonend，12箱及以上只能使用palletend
        /// </summary>
        private HashSet<string> _specialPalletRulesConsignees = new HashSet<string>()
        {
            "C03706",
            "C03861",
            "C03977",
            "C04129",
            "C04250",
            "C04260",
            "C04271",
            "C03707",
        };
        
        [AbilityAble(6432782857596309504, AbilityType.ScanAction, true, KPIActionType.LSCombinePallet | KPIActionType.LSCombinePalletPlus, "consignee")]
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

            if (_specialPalletRulesConsignees.Contains(base.Vm.SelfInfo.consignee.ToString()) && base.Vm.SelfInfo.doingPackages.Length < _specialPalletRulesThreshold)
            {
                WesModernDialog.ShowWesMessage($"少於{_specialPalletRulesThreshold}箱請使用CartonEnd。包含客戶:{string.Join(",", _specialPalletRulesConsignees)}");
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

                //打印板標籤
                List<PrintTemplateModel> templates = new List<PrintTemplateModel>();

                PrintTemplateModel pm = new PrintTemplateModel();
                pm.PrintData = labelInfo;
                pm.TemplateFileName = "PalletEndByTruck_Pallet.tff";
                pm.Mode = PrintMode.TFORMer;
                templates.Add(pm);

                LabelPrintBase lpb = new LabelPrintBase(templates, false);
                var res = lpb.Print();

                PrepareData();
                return true;
            }
            return false;
            
        }
        
        [AbilityAble(6432783148219637760, AbilityType.ScanAction, true, KPIActionType.LSCombineCarton | KPIActionType.LSCombineCartonPlus, "consignee")]
        public override bool CartonEnd(string scanVal)
        {
            if (!DynamicUtil.IsExist(base.Vm.SelfInfo, "doingPackages") || base.Vm.SelfInfo.doingPackages == null || base.Vm.SelfInfo.doingPackages.Length == 0)
            {
                WesModernDialog.ShowWesMessage("未掃描任何packageId");
                return false;
            }

            if (_specialPalletRulesConsignees.Contains(base.Vm.SelfInfo.consignee.ToString()) && base.Vm.SelfInfo.doingPackages.Length >= _specialPalletRulesThreshold)
            {
                WesModernDialog.ShowWesMessage($"{_specialPalletRulesThreshold}箱以上請使用PalletEnd。包含客戶:{string.Join(",", _specialPalletRulesConsignees)}");
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
                    barTenderModel.TemplateFileName = "Avnet_PalletEnd_Pallet.btw";
                    templates.Add(barTenderModel);
                }

                LabelPrintBase labelPrint = new LabelPrintBase(templates, false);
                labelPrint.Print();

                PrepareData();
                return true;
            }
            return false;
        }

    }
}
