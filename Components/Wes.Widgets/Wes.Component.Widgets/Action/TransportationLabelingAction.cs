using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Desktop.Windows;
using Wes.Wrapper;
using Wes.Print;
using System.Windows.Controls;
using System.Windows;
using Wes.Core.Attribute;

namespace Wes.Component.Widgets.Action
{
    public class TransportationLabelingAction : ScanActionBase<WesFlowID, TransportationLabelingAction>, IScanAction
    {
        /// <summary>
        /// 是否未整板出
        /// </summary>
        private bool _isPalletShipping = false;

        public virtual void ScanFlowActionScanTruckNo(string val)
        {
            if (!val.IsTxt())
            {
                WesModernDialog.ShowWesMessage("Truck Order 错误，请重新扫描");
                return;
            }
            _isPalletShipping = false;
            dynamic truckInfo = RestApi.NewInstance(Method.GET)
                        .AddCommonUri(RestUrlType.WmsServer, "air-shipping/get-txt-info")
                        .AddQueryParameter("txt", val)
                        .Execute()
                        .To<object>();

            Vm.SelfInfo.txt = truckInfo.txt;
            Vm.SelfInfo.endCustomer = truckInfo.endCustomer;
            Vm.SelfInfo.requireDeliveryTime = truckInfo.requireDeliveryTime;
            Vm.SelfInfo.hawb = truckInfo.hawb;
            Vm.SelfInfo.mawb = truckInfo.mawb;
            Vm.SelfInfo.plts = truckInfo.plts;
            Vm.SelfInfo.ctn = truckInfo.ctn;
            Vm.SelfInfo.looseCtn = truckInfo.looseCtn;
            Vm.SelfInfo.isDone = (bool)truckInfo.isDone;
            Vm.SelfInfo.isSmallCompleted = (bool)truckInfo.isSmallCompleted;
            Vm.SelfInfo.isAirShippingLabeled = (bool)truckInfo.isAirShippingLabeled;
            Vm.SelfInfo.isBindTruck = (bool)truckInfo.isBindTruck;
            Vm.SelfInfo.statusFlag = truckInfo.statusFlag;
            Vm.SelfInfo.isTimeout = (bool)truckInfo.isTimeout;
            Vm.SelfInfo.shippedDate = truckInfo.shippedDate;
            Vm.SelfInfo.orderNo = truckInfo.orderNo;
            Vm.SelfInfo.orderGuid = truckInfo.orderGuid;
            Vm.SelfInfo.consignee = truckInfo.consignee;
            if ((int)Vm.SelfInfo.plts > 0)
            {
                _isPalletShipping = true;
            }
            LoadPalletList(val);

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PALLET_OR_CARTON);
        }

        private void LoadPalletList(string txt)
        {
            dynamic palletList = RestApi.NewInstance(Method.GET)
                        .AddCommonUri(RestUrlType.WmsServer, "air-shipping/get-pallet-list")
                        .AddQueryParameter("txt", txt)
                        .Execute()
                        .To<object>();
            Vm.SelfInfo.palletItems = palletList;
            if (Vm.SelfInfo.palletItems == null || Vm.SelfInfo.palletItems.Count == 0)
            {
                WesModernDialog.ShowWesMessage("Truck Order 沒有任何板或箱，請確認單號是否正確");
            }
        }

        private void CheckTxtLabeledState()
        {
            if (Vm.SelfInfo.palletItems != null)
            {
                int labeledCount = 0;
                foreach (var item in Vm.SelfInfo.palletItems)
                {
                    if (item.Labeled == 1)
                    {
                        labeledCount++;
                    }
                }
                if (labeledCount == Vm.SelfInfo.palletItems.Count)
                {
                    bool res = RestApi.NewInstance(Method.POST)
                            .AddCommonUri(RestUrlType.WmsServer, "air-shipping/update-labeled-status")
                            .AddJsonBody(Vm.GetSelfInfo())
                            .Execute()
                            .ToBoolean();

                    WesModernDialog.ShowWesMessageAsyc("Truck Order中所有的板或箱已贴标完成");

                    Vm.ResetUiStatus();
                }
            }
        }

        public virtual void ScanFlowActionScanPalletOrCarton(string val)
        {
            dynamic scanItem = null;
            foreach (var item in Vm.SelfInfo.palletItems)
            {
                if (string.Compare(item.CPkgID.ToString(), val, true) == 0)
                {
                    scanItem = item;
                    break;
                }
            }
            if (scanItem == null)
            {
                WesModernDialog.ShowWesMessage($"扫描板号或箱号{val}未找到");
                return;
            }

            PrintPallet(scanItem);

            CheckTxtLabeledState();
        }

        private void PrintPallet(dynamic item)
        {
            int labelCount = 0;
            if (item.TotalCtns > 0)
            {
                if (_isPalletShipping)
                {
                    labelCount += 1;
                }
                else
                {
                    labelCount += (int)item.TotalCtns;
                }
            }
            else if (item.TotalLCtns > 0)
            {
                labelCount += (int)item.TotalLCtns;
            }

            Vm.SelfInfo.labelCount = labelCount;
            Vm.SelfInfo.ctns = item.TotalCtns;
            Vm.SelfInfo.looseCtns = item.TotalLCtns;
            Vm.SelfInfo.qty = item.TotalQty;
            Vm.SelfInfo.location = item.BinNo;
            Vm.SelfInfo.palletId = item.CPkgID;
            Vm.SelfInfo.userCode = WesDesktop.Instance.User.Code;
            Vm.SelfInfo.version = WesDesktop.Instance.Version;
            Print(item);
            ResetLabeledInfo();
        }

        [AbilityAble(6432783148219637760, AbilityType.ScanAction, true, KPIActionType.PDAAirLabel, "consignee")]
        public virtual bool Print(dynamic palletItem)
        {
            dynamic labelList = RestApi.NewInstance(Method.POST)
                        .AddCommonUri(RestUrlType.WmsServer, "air-shipping/get-label-info")
                        .AddJsonBody(Vm.GetSelfInfo())
                        .Execute()
                        .To<object>();
            if (labelList != null && labelList.Count > 0)
            {
                List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
                int labelCount = labelList.Count;
                for (int i = 0; i < labelCount; i++)
                {
                    var item = labelList[i];
                    PrintTemplateModel barTenderModel = new PrintTemplateModel();
                    barTenderModel.PrintData = item;
                    barTenderModel.TemplateFileName = "AirLabel-Template.btw";
                    templates.Add(barTenderModel);
                }

                LabelPrintBase labelPrint = new LabelPrintBase(templates, false);
                var res = labelPrint.Print();

                if (res == ErrorCode.Success)
                {
                    if (palletItem.Labeled == 0)
                    {
                        palletItem.Labeled = 1;
                        return true;
                    }
                }
            }

            return false;
        }

        private void ResetLabeledInfo()
        {
            Vm.SelfInfo.labelCount = 0;
            Vm.SelfInfo.ctns = 0;
            Vm.SelfInfo.looseCtns = 0;
            Vm.SelfInfo.qty = 0;
            Vm.SelfInfo.location = "";
            Vm.SelfInfo.palletId = "";
            Vm.SelfInfo.userCode = "";
            Vm.SelfInfo.version = "";
        }

        public virtual void RePrint(object obj)
        {
            Button btn = null;
            if (obj is RoutedEventArgs)
            {
                btn = (obj as RoutedEventArgs).Source as Button;
            }

            if (btn != null && MasterAuthorService.Authorization())
            {
                dynamic val = btn.Tag as dynamic;
                if (val.Labeled == 1)
                    PrintPallet(val);
                else
                    WesModernDialog.ShowWesMessage("無法重新打印，請先掃描板號或箱號打印");
            }
        }
    }
}
