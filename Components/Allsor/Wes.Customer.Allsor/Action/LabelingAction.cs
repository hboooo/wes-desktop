using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Controls;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Allsor.Action
{
    public class LabelingAction : ScanActionBase<WesFlowID, LabelingAction>, IScanAction
    {
        private ObservableCollection<BarCodeScanModel> souce = null;

        public virtual void ScanLoadingNo(string scanVal)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-operation")
                .AddQueryParameter("sxt", scanVal)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.sxt = scanVal;
            base.Vm.SelfInfo.consignee = (string) result.consignee;
            BindImage(base.Vm, "REEL", "BOX");

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID, "Package Id");
            BindingAutoComplateData();
        }

        private void BindingAutoComplateData()
        {
            RestApi.NewInstance(Method.GET)
                .AddUri("/reel-labeling/part-end")
                .AddQueryParameter("sxt", (string) base.Vm.SelfInfo.sxt)
                .ExecuteAsync((res, exp, restApi) =>
                {
                    if (restApi != null)
                    {
                        dynamic listCode = restApi.To<object>();
                        souce = new ObservableCollection<BarCodeScanModel>();
                        for (int i = 0; i < listCode.Count; i++)
                        {
                            souce.Add(new BarCodeScanModel()
                            {
                                Name = listCode[i].ToString(),
                                Type = null,
                                Code = null
                            });
                        }
                    }
                });
        }

        public virtual void ScanPackageId(string scanVal)
        {
            //自动CartonEnd上一箱
            if (!string.IsNullOrEmpty(base.Vm.SelfInfo.pid) && scanVal != base.Vm.SelfInfo.pid)
            {
                try
                {
                    this.CartonEnd(scanVal);
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

            this.LoadingCarton(scanVal);

            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                base.Vm.SelfInfo.useIntelligent = true;
                base.Vm.SelfInfo.intelligentItems = souce;
            }));
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
            base.Vm.SelfInfo.spn = scanVal;

            dynamic partResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/spn")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.spn = partResult.spn;
            base.Vm.SelfInfo.pn = partResult.partNo;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                base.Vm.SelfInfo.useIntelligent = false;
            }));
        }

        public virtual void ScanLotNo(string scanVal)
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
            }
        }

        public virtual void ScanDcNo(string scanVal)
        {
            base.Vm.SelfInfo.dc = scanVal;

            dynamic dcResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/dc")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.dc = dcResult.dc;
            base.Vm.SelfInfo.dt = dcResult.dt;
        }

        public virtual void ScanQty(string scanVal)
        {
            base.Vm.SelfInfo.strQty = scanVal;

            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/qty")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.qty = qtyResult.qty;
        }

        public virtual void ScanQrCode(string scanVal)
        {
            base.Vm.SelfInfo.qrCode = scanVal;

            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/qc")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.pn = qtyResult.pn;
            base.Vm.SelfInfo.dc = qtyResult.dc;
            base.Vm.SelfInfo.dt = qtyResult.dt;
            base.Vm.SelfInfo.lot = qtyResult.lot;
            base.Vm.SelfInfo.qty = qtyResult.qty;
        }

        public virtual void DeleteBoxLabel(dynamic model)
        {
            if (!base.Vm.SelfInfo.isMasterDelete)
                base.Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!base.Vm.SelfInfo.isMasterDelete) return;

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
            if (!base.Vm.SelfInfo.isMasterDelete)
                base.Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!base.Vm.SelfInfo.isMasterDelete) return;

            var result = WesModernDialog.ShowWesMessage("[盘]您确定要删除盘标吗?", "WES_Message".GetLanguage(),
                MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;
            RestApi.NewInstance(Method.DELETE)
                .AddUri(RestUrlType.WmsServer, "labeling/reel")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .AddJsonBody("rid", (string) model.rowId)
                .Execute();

            this.LoadingCarton(this.Vm.SelfInfo.pid);
        }

        public virtual bool Save(string scanVal)
        {
            if ((bool) base.Vm.SelfInfo.isLabeling)
            {
                SaveCheckingAndLabeling(scanVal);
            }
            else
            {
                SaveChecking(scanVal);
            }

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
            base.Vm.SelfInfo.isReprint = false;
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/reel-end")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            var labelCount=PrivatePrint(base.Vm.SelfInfo, labels);

            //將返回的KPI統計值賦值給Vm.SelfInfo
            Vm.SelfInfo.labelCount = labelCount;

            this.LoadingCarton(this.Vm.SelfInfo.pid);
        }

        public virtual void Reprint(long rid)
        {
            if (!base.Vm.SelfInfo.isMasterReprint)
                base.Vm.SelfInfo.isMasterReprint = MasterAuthorService.Authorization(VerificationType.Print);
            if (!base.Vm.SelfInfo.isMasterReprint) return;
            base.Vm.SelfInfo.isReprint = true;
            base.Vm.SelfInfo.rid = rid;
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/reel-end")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            PrivatePrint(base.Vm.SelfInfo, labels);
        }

        [AbilityAble(true, KPIActionType.LSLabelingPlus, "consignee")]
        public virtual bool CartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/pre-carton-end")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            this.Vm.CleanScanValue();
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
            PrivatePrint(base.Vm.SelfInfo, labels);
            this.LoadingCarton(this.Vm.SelfInfo.pid);
            if ((int) base.Vm.SelfInfo.overQty == 0)
            {
                this.CartonEnd(scanValue);
            }

            this.Vm.CleanScanValue();
        }

        public static int PrivatePrint(dynamic vm, List<dynamic> labels)
        {
            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();

            //添加對倉庫作業人員的kpi的統計
      
            var labelCunt = 0;
            foreach (var label in labels)
            {
                for (int i = 0; i < (int) label.printCount; i++)
                {
                    PrintTemplateModel ptm = new PrintTemplateModel();
                    ptm.TemplateFileName = label.labelName + ".btw";
                    ptm.PrintData = label.context;
                    templates.Add(ptm);
                    //對kpi操作數量進行累積
                    labelCunt++;
                }
            }

            var labelPrint = new LabelPrintBase(templates);
            ErrorCode error = labelPrint.Print();
            Debug.WriteLine($"打印返回CODE: {error}");

            return labelCunt;
        }

        private void LoadingCarton(string pid)
        {
            base.Vm.SelfInfo.pid = pid;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-carton")
                .AddQueryParameter(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.overQty = (int) result.overQty;
            base.Vm.SelfInfo.isLabeling = (bool) result.isLabeling;
            base.Vm.SelfInfo.cartonList = result.cartons;
            base.Vm.SelfInfo.reelsList = result.reels;
            base.Vm.SelfInfo.isMasterReprint = false;
            base.Vm.SelfInfo.isMasterDelete = false;

            if (!(bool) result.isLabeling)
            {
                base.Vm.SelfInfo.OnlyCheckMessage = "(Checking)";
            }
            else
            {
                base.Vm.SelfInfo.OnlyCheckMessage = "(Labeling)";
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

        protected virtual string PnPropertyName => "Part NO.";

        protected virtual string GwPropertyName => "Gw.";

        protected virtual string DcPropertyName => "DC";

        protected virtual string QtyPropertyName => "QTY";

        protected virtual string MinQtyPropertyName => "Min Qty";
    }
}