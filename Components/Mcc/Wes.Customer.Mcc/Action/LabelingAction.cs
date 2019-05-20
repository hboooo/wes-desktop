using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Wes.Core.Api;
using Wes.Core.Attribute;
using Wes.Core.ViewModel;
using Wes.Customer.Mcc.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Controls;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action
{
    public class LabelingAction : ScanActionBase<WesFlowID, LabelingAction>, IScanActionContext
    {
        private ObservableCollection<BarCodeScanModel> _source;

        protected virtual string PnPropertyName => "Part NO.";

        protected virtual string GwPropertyName => "Gw.";

        protected virtual string DcPropertyName => "DC";

        protected virtual string QtyPropertyName => "QTY";

        protected virtual string MinQtyPropertyName => "Min Qty";

        protected virtual string PnOrQrPropertyName => "Part No. Or QrCode";

        protected virtual string LotPropertyName => "Lot";

        public object getContext()
        {
            return Vm.SelfInfo;
        }

        public virtual void RouteMe(Dictionary<string, object> routeParamsMap)
        {
            routeParamsMap["routeIsHandled"] = true;
            ScanPackageId(routeParamsMap["pid"].ToString());
        }

        public virtual void ScanLocalSpn(string scanVal)
        {
            string defaultSupplier = null;
            if (Vm.SelfInfo.localSupplierList != null)
            {
                var suppliers = Vm.SelfInfo.localSupplierList as List<dynamic>;
                var supplier = suppliers.FirstOrDefault(spl => Convert.ToString(spl.partNo) == scanVal);
                if (supplier != null)
                {
                    defaultSupplier = supplier.shipper.ToString();
                }
            }
            else if (Vm.SelfInfo.supplier != null)
            {
                defaultSupplier = Vm.SelfInfo.supplier;
            }

            if (defaultSupplier == null)
            {
                throw new WesException("无法根据箱号和料号确定供应商");
            }

            Vm.SelfInfo.supplier = defaultSupplier;
            Vm.SelfInfo.isFirstSkipLocalSpn = false;
            Vm.ReinitializeAction(defaultSupplier, "pid", Vm.SelfInfo.pid, "spn", scanVal, "supplier", defaultSupplier);
        }

        public virtual void ScanLoadingNo(string scanVal)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-operation")
                .AddQueryParameter("sxt", scanVal)
                .Execute()
                .To<object>();

            Vm.SelfInfo.sxt = scanVal;
            Vm.SelfInfo.consignee = (string) result.consignee;
            BindImage(Vm, "REEL", "BOX");

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, "Package Id");
            BindingAutoCompleteData();
        }

        public virtual void ScanPackageId(string scanVal)
        {
            #region 自动CartonEnd上一箱

            if (!string.IsNullOrEmpty(base.Vm.SelfInfo.pid) && scanVal != base.Vm.SelfInfo.pid)
            {
                try
                {
                    CartonEnd(scanVal);
                    WesDesktopSounds.Success();
                }
                catch (WesRestException ex)
                {
                    if (ex.MessageCode == 6468026496215683072 || ex.MessageCode == 6437996107376103424)
                    {
                        //ignore
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            #endregion

            LoadingCarton(scanVal);

            #region 智能提示

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                Vm.SelfInfo.useIntelligent = true;
                Vm.SelfInfo.intelligentItems = _source;
            }));

            #endregion

            if ((bool) Vm.SelfInfo.isFirstSkipLocalSpn)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOCAL_SPN, "该箱有多个供应商, 请先扫描料号确定供应商");
            }
            else
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
            }
        }

        public virtual void ScanBranch(string scanVal)
        {
            if (scanVal.Length > 30)
            {
                ScanQrCode(scanVal);
            }
            else
            {
                ScanSpn(scanVal);
            }
        }

        public virtual void ScanSpn(string scanVal)
        {
            Vm.SelfInfo.spn = scanVal;
            dynamic partResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/spn")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.spn = partResult.spn;
            Vm.SelfInfo.pn = partResult.partNo;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                Vm.SelfInfo.useIntelligent = false;
            }));
        }

        public virtual void ScanLotNo(string scanVal)
        {
            Vm.SelfInfo.lot = scanVal;

            dynamic lotResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/lot")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.lot = lotResult.lot;
            if (lotResult.dc != null)
            {
                Vm.SelfInfo.dc = lotResult.dc;
                Vm.SelfInfo.dt = lotResult.dt;
                Vm.SelfInfo.originDc = lotResult.originDc;
            }
        }

        public virtual void ScanDcNo(string scanVal)
        {
            Vm.SelfInfo.dc = scanVal;

            dynamic dcResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/dc")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.dc = dcResult.dc;
            Vm.SelfInfo.dt = dcResult.dt;
            Vm.SelfInfo.originDc = dcResult.originDc;
            Vm.SelfInfo.formatDc = dcResult.formatDc;
        }

        public virtual void ScanQty(string scanVal)
        {
            Vm.SelfInfo.strQty = scanVal;

            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/qty")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.qty = qtyResult.qty;
        }

        public virtual void ScanQrCode(string scanVal)
        {
            Vm.SelfInfo.qrCode = scanVal;

            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/qc")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.pn = qtyResult.pn;
            Vm.SelfInfo.dc = qtyResult.dc;
            Vm.SelfInfo.dt = qtyResult.dt;
            Vm.SelfInfo.lot = qtyResult.lot;
            Vm.SelfInfo.qty = qtyResult.qty;
            Vm.SelfInfo.originDc = qtyResult.originDc;
        }

        public virtual void DeleteBoxLabel(dynamic model)
        {
            if (!Vm.SelfInfo.isMasterDelete)
                Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!Vm.SelfInfo.isMasterDelete) return;

            var result = WesModernDialog.ShowWesMessage("[盒]盒标多行会被删除, 您确定要删除吗?", "WES_Message".GetLanguage(),
                MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;
            RestApi.NewInstance(Method.DELETE)
                .AddUri(RestUrlType.WmsServer, "labeling/box")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .AddJsonBody("rid", (string) model.rowId)
                .Execute();

            this.LoadingCarton(this.Vm.SelfInfo.pid);
        }

        public virtual void DeleteReelLabel(dynamic model)
        {
            if (!Vm.SelfInfo.isMasterDelete)
                Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!Vm.SelfInfo.isMasterDelete) return;

            var result = WesModernDialog.ShowWesMessage("[盘]您确定要删除盘标吗?", "WES_Message".GetLanguage(),
                MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;
            RestApi.NewInstance(Method.DELETE)
                .AddUri(RestUrlType.WmsServer, "labeling/reel")
                .AddJsonBody(Vm.GetSelfInfo())
                .AddJsonBody("rid", (string) model.rowId)
                .Execute();

            this.LoadingCarton(this.Vm.SelfInfo.pid);
        }

        [AbilityAble(true, KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "consignee")]
        public virtual bool Save(string scanVal)
        {
            if ((bool) Vm.SelfInfo.isLabeling)
            {
                SaveCheckingAndLabeling(scanVal);
            }
            else
            {
                SaveChecking(scanVal);
            }

//#if !DEBUG
            if (LabelingViewModel.IsMoreSupplier)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOCAL_SPN, "该箱有多个供应商, 请先扫描料号确定供应商");
            }
            else
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrPropertyName);
            }

//#endif
            return true;
        }

        [AbilityAble(true, KPIActionType.LSChecking, "consignee")]
        public virtual bool SaveChecking(string scanVal)
        {
            PrivateSave(scanVal);
            return true;
        }

        [AbilityAble(true, KPIActionType.LSLabeling, "consignee")]
        public virtual bool SaveCheckingAndLabeling(string scanVal)
        {
            PrivateSave(scanVal);
            return true;
        }

        protected virtual void PrivateSave(string scanVal)
        {
            Vm.SelfInfo.isReprint = false;
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/reel-end")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            var printCount= PrivatePrint(Vm.SelfInfo, labels);
            Vm.SelfInfo.labelCount = printCount;
            this.LoadingCarton(this.Vm.SelfInfo.pid);
        }

        public virtual void Reprint(long rid)
        {
            if (!Vm.SelfInfo.isMasterReprint)
                Vm.SelfInfo.isMasterReprint = MasterAuthorService.Authorization(VerificationType.Print);
            if (!Vm.SelfInfo.isMasterReprint) return;
            Vm.SelfInfo.isReprint = true;
            Vm.SelfInfo.rid = rid;
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/reel-end")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            PrivatePrint(Vm.SelfInfo, labels);
        }

        [AbilityAble(true, KPIActionType.LSLabelingPlus, "consignee")]
        public virtual bool CartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/pre-carton-end")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            Vm.CleanScanValue();
            return true;
        }

        [AbilityAble(true, KPIActionType.LSLabeling, "consignee")]
        public virtual bool BoxEnd(string scanVal)
        {
            PrivateBoxBnd(scanVal);
            return true;
        }

        public virtual void NoBox(string scanVal)
        {
            PrivateBoxBnd(scanVal);
        }

        private void PrivateBoxBnd(string scanValue)
        {
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/box-end")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .AddJsonBody("boxType", scanValue.ToUpper())
                .Execute()
                .To<List<object>>();
            PrivatePrint(Vm.SelfInfo, labels);
            this.LoadingCarton(Vm.SelfInfo.pid);
            if ((int) Vm.SelfInfo.overQty == 0)
            {
                CartonEnd(scanValue);
            }

            Vm.CleanScanValue();
        }

        protected virtual void LoadingCarton(string pid)
        {
            Vm.SelfInfo.pid = pid;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-carton")
                .AddQueryParameter(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();

            Vm.SelfInfo.overQty = (int) result.overQty;
            Vm.SelfInfo.isLabeling = (bool) result.isLabeling;
            Vm.SelfInfo.cartonList = result.cartons;
            Vm.SelfInfo.reelsList = result.reels;
            Vm.SelfInfo.isMasterReprint = false;
            Vm.SelfInfo.isMasterDelete = false;

            if (!(bool) result.isLabeling)
            {
                Vm.SelfInfo.OnlyCheckMessage = "(Checking)";
            }
            else
            {
                Vm.SelfInfo.OnlyCheckMessage = "(Labeling)";
            }
        }

        public static void BindImage(dynamic vm, params string[] labelImageType)
        {
            vm.SelfInfo.labelImageType = labelImageType.ToList<string>();

            var result = RestApi.NewInstance(Method.POST)
                .AddUri(RestUrlType.WmsServer, "labeling/load-image")
                .AddJsonBody(vm.GetSelfInfo())
                .Execute()
                .To<dynamic>();
            var viewData = new List<dynamic>();
            foreach (var item in result)
            {
                viewData.Add(new
                {
                    ImageUri = item.photoUrl.ToString(),
                    FileName = item.fileName.ToString(),
                    Desc = Path.GetFileNameWithoutExtension(item.fileName.ToString()),
                    Remark = item.remarks == null ? "" : item.remarks.ToString()
                });
            }

            vm.SelfInfo.imageViewList = viewData;
        }

        public static int PrivatePrint(dynamic vm, List<dynamic> labels)
        {
            int printCount=0;
            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
            foreach (var label in labels)
            {
                for (int i = 0; i < (int) label.printCount; i++)
                {
                    PrintTemplateModel ptm = new PrintTemplateModel();
                    ptm.TemplateFileName = label.labelName + ".btw";
                    ptm.PrintData = label.context;
                   
                    templates.Add(ptm);
                    printCount++;
                }
            }

            var labelPrint = new LabelPrintBase(templates);
            ErrorCode error = labelPrint.Print();
            Debug.WriteLine($"打印返回CODE: {error}");
            return printCount;
        }

        private void BindingAutoCompleteData()
        {
            RestApi.NewInstance(Method.GET)
                .AddUri("/reel-labeling/part-end")
                .AddQueryParameter("sxt", (string) base.Vm.SelfInfo.sxt)
                .ExecuteAsync((res, exp, restApi) =>
                {
                    if (restApi != null)
                    {
                        dynamic listCode = restApi.To<object>();
                        _source = new ObservableCollection<BarCodeScanModel>();
                        if (listCode == null)
                        {
                            return;
                        }

                        for (int i = 0; i < listCode.Count; i++)
                        {
                            _source.Add(new BarCodeScanModel()
                            {
                                Name = listCode[i].ToString(),
                                Type = null,
                                Code = null
                            });
                        }
                    }
                });
        }
    }
}