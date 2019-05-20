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
using Wes.Customer.Promaster.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Controls;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action
{
    public class LabelingAction : ScanActionBase<WesFlowID, LabelingAction>, IScanActionContext
    {
        private ObservableCollection<BarCodeScanModel> _source;

        private static string PnOrQrCodePropertyName => "Pn Or QrCode";

        protected virtual string PnPropertyName => "PN";
        protected virtual string LotPropertyName => "Lot";
        protected virtual string DcPropertyName => "DC";
        protected virtual string QtyPropertyName => "Qty";
        protected virtual string CooPropertyName => "Coo";

        public object getContext()
        {
            return Vm.SelfInfo;
        }

        /// <summary>
        /// 0. 路由控制
        /// </summary>
        /// <param name="routParams"></param>
        public void RoutMe(Dictionary<string, object> routParams)
        {
            routParams["routeIsHandled"] = true;
            ScanPackageId(routParams["pid"].ToString());
        }

        /// <summary>
        /// 1. 扫描 package ID
        /// </summary>
        /// <param name="packageId"></param>
        public virtual void ScanPackageId(string packageId)
        {
            //判断是否有上一箱未进行保存
            if (!string.IsNullOrEmpty(Vm.SelfInfo.pid) && !string.Equals(Vm.SelfInfo.pid, packageId))
            {
                try
                {
                    //保存上一箱数据
                    CartonEnd(packageId);

                    WesDesktopSounds.Success();
                }
                catch (WesRestException e)
                {
                    if (e.MessageCode == 6468026496215683072 || e.MessageCode == 6437996107376103424)
                    {
                        //ignore
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            //根据packageId加载箱数据
            LoadingCarton(packageId);
            //智能提示
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                Vm.SelfInfo.useIntelligent = true;
                Vm.SelfInfo.intelligentItems = _source;
            }));
            // 判断是否为多料号供应商
            if ((bool) Vm.SelfInfo.isFirstSkipLocalSpn)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOCAL_SPN, "该箱有多个供应商, 请先扫描料号确定供应商");
            }
            else
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
            }
        }

        /// <summary>
        /// 将箱信息进行入库处理
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public void CartonEnd(string packageId)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/pre-carton-end")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            Vm.CleanScanValue();
        }

        /// <summary>
        /// 根据箱号加载箱数据
        /// </summary>
        /// <param name="packageId"></param>
        public void LoadingCarton(string packageId)
        {
            Vm.SelfInfo.pid = packageId;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-carton")
                .AddQueryParameter(Vm.GetSelfInfo())
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

        public void Reprint(long rid)
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

        /// <summary>
        /// 2. 判断入参为 partNo 还是 QrCode 并进行引导下一步流程
        /// </summary>
        /// <param name="pnOrQrCode"></param>
        public virtual void ScanBranch(string pnOrQrCode)
        {
            if (pnOrQrCode.Length > 30)
            {
                //QrCode
                ScanQrCode(pnOrQrCode);
            }
            else
            {
                //partNo
                ScanPartNo(pnOrQrCode);
            }
        }

        /// <summary>
        /// 2.1 处理 QrCode 的业务逻辑
        /// </summary>
        /// <param name="qrCode"></param>
        public virtual void ScanQrCode(string qrCode)
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
        }

        /// <summary>
        /// 2.2 扫描 partNo
        /// </summary>
        /// <param name="partNo"></param>
        public virtual void ScanPartNo(string partNo)
        {
            Vm.SelfInfo.spn = partNo;
            dynamic partResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/spn")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.spn = partResult.spn;
            Vm.SelfInfo.pn = partResult.partNo;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                Vm.SelfInfo.useIntelligent = false;
            }));
        }

        /// <summary>
        /// 3. 扫描 lot
        /// </summary>
        /// <param name="lot"></param>
        public virtual void ScanLotNo(string lot)
        {
            Vm.SelfInfo.lot = lot;

            dynamic lotResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/lot")
                .AddJsonBody(Vm.GetSelfInfo())
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

        /// <summary>
        /// 4. 扫描 dc
        /// </summary>
        /// <param name="dc"></param>
        public virtual void ScanDcNo(string dc)
        {
            Vm.SelfInfo.dc = dc;

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

        /// <summary>
        /// 5. 扫描 Qty
        /// </summary>
        /// <param name="qty"></param>
        public virtual void ScanQty(string qty)
        {
            Vm.SelfInfo.strQty = qty;

            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/qty")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.qty = qtyResult.qty;
        }

        /// <summary>
        /// 删除盒标签
        /// </summary>
        /// <param name="model"></param>
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
                .AddJsonBody(Vm.GetSelfInfo())
                .AddJsonBody("rid", (string) model.rowId)
                .Execute();

            this.LoadingCarton(Vm.SelfInfo.pid);
        }

        /// <summary>
        /// 删除卷标签
        /// </summary>
        /// <param name="model"></param>
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

            this.LoadingCarton(Vm.SelfInfo.pid);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="scanVal"></param>
        /// <returns></returns>
        [AbilityAble(true, KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "consignee")]
        public virtual bool Save(string scanVal)
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
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnOrQrCodePropertyName);
            }

            return true;
        }

        /// <summary>
        /// 保存检查信息
        /// </summary>
        /// <param name="scanVal"></param>
        public virtual void SaveChecking(string scanVal)
        {
            PrivateSave(scanVal);
        }

        /// <summary>
        /// 保存检查和标签
        /// </summary>
        /// <param name="scanVal"></param>
        /// <returns></returns>
        [AbilityAble(true, KPIActionType.LSLabeling, "consignee")]
        public virtual void SaveCheckingAndLabeling(string scanVal)
        {
            PrivateSave(scanVal);
        }

        /// <summary>
        /// 私有的保存方法
        /// </summary>
        /// <param name="scanVal"></param>
        public virtual void PrivateSave(string scanVal)
        {
            Vm.SelfInfo.isReprint = false;
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/reel-end")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            var printCount = PrivatePrint(Vm.SelfInfo, labels);
            Vm.SelfInfo.labelCount = printCount;
            this.LoadingCarton(Vm.SelfInfo.pid);
        }

        /// <summary>
        /// 私有的打印方法
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="labels"></param>
        /// <returns></returns>
        public static int PrivatePrint(dynamic vm, List<dynamic> labels)
        {
            var printCount = 0;
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

        /// <summary>
        /// 重新打印
        /// </summary>
        /// <param name="rid"></param>
        public virtual void RePrint(long rid)
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

        /// <summary>
        /// 保存盒数据
        /// </summary>
        /// <param name="scanVal"></param>
        /// <returns></returns>
        [AbilityAble(true, KPIActionType.LSLabeling, "consignee")]
        public virtual bool BoxEnd(string scanVal)
        {
            PrivateSaveBox(scanVal);
            return true;
        }

        /// <summary>
        /// 私有的盒保存方法
        /// </summary>
        /// <param name="scanValue"></param>
        public void PrivateSaveBox(string scanValue)
        {
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/box-end")
                .AddJsonBody(Vm.GetSelfInfo())
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

        /// <summary>
        /// 绑定图片
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="labelImageType"></param>
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

        /// <summary>
        /// 绑定自动完成数据
        /// </summary>
        public void BindingAutoCompleteData()
        {
            RestApi.NewInstance(Method.GET)
                .AddUri("/reel-labeling/part-end")
                .AddQueryParameter("sxt", (string)Vm.SelfInfo.sxt)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scanVal"></param>
        public virtual void NoBox(string scanVal)
        {
            PrivateSaveBox(scanVal);
        }

        /// <summary>
        /// 扫描本地 pn
        /// </summary>
        /// <param name="partNo"></param>
        /// <exception cref="WesException"></exception>
        public virtual void ScanLocalSpn(string partNo)
        {
            string defaultSupplier = null;
            if (Vm.SelfInfo.localSupplierList != null)
            {
                var suppliers = Vm.SelfInfo.localSupplierList as List<dynamic>;
                var supplier = suppliers.FirstOrDefault(spl => Convert.ToString(spl.partNo) == partNo);
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
            Vm.ReinitializeAction(defaultSupplier, "pid", Vm.SelfInfo.pid, "spn", partNo, "supplier", defaultSupplier);
        }

        /// <summary>
        /// 扫描 LoadingNo(订单编号)
        /// </summary>
        /// <param name="scanVal"></param>
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

        protected dynamic GetPartNo(string partNo)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri("collecting/part-number")
                .AddQueryParameter(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            return result;
        }

        protected void CommonLoadPackageInfo(string scanVal)
        {
            Vm.SelfInfo.pid = scanVal;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri("collecting")
                .AddQueryParameter(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.isCompleted = result.isCompleted;
            Vm.SelfInfo.rxt = result.rxt;

            Vm.SelfInfo.cartons = result.cartons;
            Vm.SelfInfo.total = result.total;
            Vm.SelfInfo.isMasterReprint = false;
            Vm.SelfInfo.isMasterDelete = false;
        }

        public virtual void ScanCoo(string coo)
        {
            Vm.SelfInfo.coo = coo;
            dynamic otherResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/coo")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.coo = otherResult.coo;
        }
    }
}