using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Wrapper;
using System.Linq;

namespace Wes.Customer.Avnet.Action
{
    public class ShippingPallteAction : ScanActionBase<WesFlowID, ShippingPallteAction>, IScanAction
    {
        public const long ShippingScriptID = 6417222208598315008;
        public const long CheckShippingPackageStateScriptID = 6417257600601038848;
        public const long UpdatePalletsScriptID = 6418021490788143104;
        public const long DeletePalletScriptID = 6418021455396610048;
        public const long DeleteCartonScriptID = 6436502016754716672;
        public const long GetOperatorImageListScriptID = 6418376110920179712;
        public const long GetPalletLabelScriptID = 6419824609285120000;
        public const long UpdateCartonScriptID = 6419832252812759040;
        public const long GetCartonLabelScriptID = 6419835889064615936;
        public const long GetErrorScanScriptID = 6417266168163209216;
        public const long GetPalletHeaderLabelScriptID = 6436179585699680256;

        private string _command = WesScanCommand.NONE;

        #region TPAD需求ID:1000672 
        // 以下 15 個 Wistron's Consignee Code 只允許 Carton End (NOT Pallet End)
        private HashSet<string> _wistron_Consignee_CartonEnd = new HashSet<string>()
        {
            "C03887",
            "C03895",
            "C03896",
            "C03897",
            "C03898",
            "C03899",
            "C03900",
            "C03901",
            "C03902",
            "C03903",
            "C03904",
            "C03905",
            "C03993",
            "C03994",
            "C03906",
        };

        // 檢測標籤規則
        private HashSet<string> _wistron_Consignee_LabelCheck = new HashSet<string>()
        {
            "C03887",
            "C03895",
            "C03896",
            "C03897",
            "C03898",
            "C03899",
            "C03900",
            "C03901",
            "C03902",
            "C03903",
            "C03904",
            "C03905",
            "C03993",
            "C03994",
        };

        //Consignee Code C03576 只允許 Palllet End (not Carton End) 
        private HashSet<string> _wistron_Consignee_PalletEnd = new HashSet<string>()
        {
            "C03576"
        };

        #endregion

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        public static readonly string WesImageUri = @"http://wms.spreadlogistics.com/UploadFile/Shipping/LabelRequest/";

        /// <summary>
        /// 保存當前打板的partNo
        /// </summary>
        private HashSet<string> _partNo = new HashSet<string>();

        public HashSet<string> PartNo
        {
            get { return _partNo; }
            set { _partNo = value; }
        }

        private string _packagePartNo = string.Empty;
        public string PackagePartNo
        {
            get { return _packagePartNo; }
            set { _packagePartNo = value; }
        }
        /// <summary>
        /// 獲取操作的圖片列表
        /// </summary>
        private void GetOperatorImageList(string scanVal)
        {
            dynamic imageList = RestApi.NewInstance(Method.POST)
                           .AddUriParam(RestUrlType.WmsServer, GetOperatorImageListScriptID, false)
                           .AddJsonBody("truckNo", scanVal)
                           .Execute()
                           .To<object>();
            if (imageList != null)
            {
                List<object> datas = new List<object>();
                foreach (var item in imageList)
                {
                    datas.Add(new
                    {
                        ImageUri = WesImageUri + item.PhotoUrl.ToString(),
                        FileName = item.FileName.ToString(),
                        Desc = Path.GetFileNameWithoutExtension(item.FileName.ToString()),
                        Remark = item.Remarks.ToString()
                    });
                }
                this.Vm.SelfInfo.imageViewList = datas.Count > 0 ? datas : null;
            }
        }

        [Ability(6417633559632158720, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanTruckNo(string scanVal)
        {
            LoadingInfoByTruckNo(scanVal);
            GetOperatorImageList(scanVal);
        }

        private void LoadingInfoByTruckNo(string scanVal)
        {
            _partNo.Clear();
            _packagePartNo = null;

            dynamic cartons = RestApi.NewInstance(Method.GET)
                        .AddUriParam(RestUrlType.WmsServer, ShippingScriptID, false)
                        .AddQueryParameter("truckNo", scanVal)
                        .Execute()
                        .To<object>();

            base.Vm.SelfInfo.undoPackages = cartons.undoPackages;
            base.Vm.SelfInfo.doingPackages = new List<dynamic>().ToArray();
            base.Vm.SelfInfo.donePackages = cartons.donePackages;
            base.Vm.SelfInfo.errorPackages = cartons.errorPackages;
            base.Vm.SelfInfo.truckNo = scanVal;
            base.Vm.SelfInfo.consignee = cartons.consignee;
            base.Vm.SelfInfo.endCustomer = cartons.endCustomer;
            base.Vm.SelfInfo.Target = cartons.consignee;
            if (base.Vm.SelfInfo.consignee == "C03575")   //苏州默认选中
            {
                base.Vm.SelfInfo.IsOnePalletPN = true;
            }
            else
            {
                base.Vm.SelfInfo.IsOnePalletPN = false;
            }

            base.Vm.SelfInfo.gw = "";
            base.Vm.SelfInfo.length = "";
            base.Vm.SelfInfo.width = "";
            base.Vm.SelfInfo.height = "";
            if (IsShippingToAvnet())
            {
                base.Vm.SelfInfo.gwVisibility = Visibility.Collapsed;
            }
            else
            {
                base.Vm.SelfInfo.gwVisibility = Visibility.Visible;
            }

            RefreshCombineCount();
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        public bool IsShippingToAvnet()
        {
            //AVNET 發給自己的貨 不需要磅重，默認錄入0
            return base.Vm.SelfInfo.consignee != null && base.Vm.SelfInfo.consignee.ToString().ToUpper() == "C00339";
        }

        [Ability(6420101420141256704, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPackageId(string scanVal)
        {
            if (ExecuteDelete(scanVal))
            {
                SetActionValid();
                return;
            }

            if (IsPackageDoing(scanVal))
            {
                WesDesktopSounds.Alarm();
                WesModernDialog.ShowWesMessage("Package正在組板中，請重新掃描");
                return;
            }

            try
            {
                dynamic result = RestApi.NewInstance(Method.GET)
                            .AddUriParam(RestUrlType.WmsServer, CheckShippingPackageStateScriptID, false)
                            .AddQueryParameter("truckNo", base.Vm.SelfInfo.truckNo)
                            .AddQueryParameter("packageId", scanVal)
                            .AddQueryParameter("consignee", base.Vm.SelfInfo.consignee)
                            .Execute()
                            .To<object>();
                if ((bool)result.state)
                {
                    if (result.partNos != null && result.partNos.Count > 0)
                    {
                        foreach (var item in result.partNos)
                        {
                            string pn = item.PartNo.ToString();
                            if (_partNo.Count == 1 && !_partNo.Contains(pn) && base.Vm.SelfInfo.IsOnePalletPN == true)
                            {
                                WesDesktopSounds.Alarm();
                                WesModernDialog.ShowWesMessage("不同料號不能組板");
                                return;
                            }
                            _partNo.Add(pn);
                            _packagePartNo = pn;
                        }
                    }

                    if (result.packageList != null && result.packageList.Count > 1)
                    {
                        HashSet<string> pkgIdList = new HashSet<string>();
                        HashSet<string> dcList = new HashSet<string>();
                        foreach (var item in result.packageList)
                        {
                            pkgIdList.Add("(3S)" + item.NInvoiceNo.ToString());
                            if (_wistron_Consignee_LabelCheck.Contains(base.Vm.SelfInfo.consignee.ToString().ToUpper()))
                            {
                                dcList.Add("(P)" + item.CustomerPN.ToString());
                            }
                            else
                            {
                                dcList.Add("##" + item.CustomerPN.ToString() + item.DataCode.ToString());
                            }
                        }
                        Dictionary<string, string> checkLabelValues = new Dictionary<string, string>();
                        for (int i = 0; i < pkgIdList.Count; i++)
                        {
                            checkLabelValues["PKG ID " + (i + 1)] = pkgIdList.ElementAt(i);
                        }
                        for (int i = 0; i < dcList.Count; i++)
                        {
                            checkLabelValues["DC " + (i + 1)] = dcList.ElementAt(i);
                        }

                        ActiveCheckLabelWindow checkLabelWindow = new ActiveCheckLabelWindow();
                        checkLabelWindow.AddItem(checkLabelValues);
                        checkLabelWindow.ShowDialog();
                    }

                    dynamic scanObj = this.RemovePackageUnDone(scanVal);
                    if (scanObj != null)
                    {
                        var doingPkgs = base.Vm.SelfInfo.doingPackages;
                        List<object> newDoingPkgs = new List<object>();
                        if (doingPkgs != null)
                        {
                            foreach (var item in doingPkgs)
                            {
                                newDoingPkgs.Add(item);
                            }
                            newDoingPkgs.Add(scanObj);
                        }
                        base.Vm.SelfInfo.doingPackages = newDoingPkgs.ToArray();
                        base.Vm.SelfInfo.doingPackageSelected = scanObj;
                        RefreshCombineCount();
                    }
                }
                else
                {
                    WesDesktopSounds.Alarm();
                    RefreshScanErrorList();
                    IsPackageDone(scanVal);
                    WesModernDialog.ShowWesMessage(result.message.ToString());
                }
            }
            catch (Exception ex)
            {
                WesDesktopSounds.Alarm();
                throw ex;
            }
        }

        private void RefreshCombineCount()
        {
            try
            {
                var totalCount = base.Vm.SelfInfo.undoPackages.Count + base.Vm.SelfInfo.doingPackages.Length + base.Vm.SelfInfo.donePackages.Count;
                var doneCount = base.Vm.SelfInfo.donePackages.Count + base.Vm.SelfInfo.doingPackages.Length;
                base.Vm.SelfInfo.undoCtn = doneCount + "/" + totalCount;
                base.Vm.SelfInfo.doingCtn = base.Vm.SelfInfo.doingPackages.Length;
                base.Vm.SelfInfo.doneCtn = base.Vm.SelfInfo.donePackages.Count;
            }
            catch (Exception ex)
            {
                LoggingService.Error("刷新Combine數據失敗", ex);
            }
        }

        /// <summary>
        /// 是否正在组板
        /// </summary>
        /// <param name="scanVal"></param>
        /// <returns></returns>
        private bool IsPackageDoing(string scanVal)
        {
            foreach (var item in base.Vm.SelfInfo.doingPackages)
            {
                if (item != null && string.Compare(item.PackageID.ToString(), scanVal, true) == 0)
                {
                    base.Vm.SelfInfo.doingPackageSelected = item;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否已经组板
        /// </summary>
        /// <param name="scanVal"></param>
        /// <returns></returns>
        private bool IsPackageDone(string scanVal)
        {
            foreach (var item in base.Vm.SelfInfo.donePackages)
            {
                if (item != null && string.Compare(item.PackageID.ToString(), scanVal, true) == 0)
                {
                    base.Vm.SelfInfo.donePackageSelected = item;
                    return true;
                }
            }
            return false;
        }

        private dynamic RemovePackageUnDone(string scanVal)
        {
            foreach (var item in base.Vm.SelfInfo.undoPackages)
            {
                if (item != null && string.Compare(item.PackageID.ToString(), scanVal, true) == 0)
                {
                    base.Vm.SelfInfo.undoPackages.Remove(item);
                    base.Vm.SelfInfo.undoPackages = DynamicJson.DeserializeObject<List<object>>(DynamicJson.SerializeObject(base.Vm.SelfInfo.undoPackages));
                    return item;
                }
            }
            return null;
        }

        #region 扫描命令
        public void DeletePallet(string scanVal)
        {
            string truckNo = null;
            try
            {
                truckNo = base.Vm.SelfInfo.truckNo;
            }
            catch { };
            if (string.IsNullOrEmpty(truckNo))
            {
                WesModernDialog.ShowWesMessage("請先輸入truckNo");
            }
            else
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
                _command = WesScanCommand.DELETE_PALLET;
                string message = "請掃描要刪除的板號";
                base.Vm.Tooltip = message;
            }
        }

        [Ability(6437537634364428288, AbilityType.ScanAction)]
        public virtual void DeleteCarton(string scanVal)
        {
            string truckNo = null;
            try
            {
                truckNo = base.Vm.SelfInfo.truckNo;
            }
            catch { };
            if (string.IsNullOrEmpty(truckNo))
            {
                throw new WesException("請先輸入truckNo");
            }
            Vm.SelfInfo.deleteKpiPackages = DynamicJson.DeserializeObject<dynamic>(DynamicJson.SerializeObject(Vm.SelfInfo.donePackages));
            var delResult = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, DeleteCartonScriptID, true)
                .AddQueryParameter("truckNo", truckNo)
                .Execute()
                .ToBoolean();
            LoadingInfoByTruckNo(base.Vm.SelfInfo.truckNo);
            if (delResult)
            {
                WesModernDialog.ShowWesMessage(string.Format("刪除组箱:{0}成功", truckNo));
            }
            else
            {
                throw new WesException(string.Format("刪除组箱:{0}失败，请联系管理员", truckNo));
            }
            SetActionValid();
        }

        private bool IsPalletSizeInput()
        {
            if (string.IsNullOrEmpty(base.Vm.SelfInfo.length.ToString()))
            {
                WesModernDialog.ShowWesMessage("請先輸入Length");
                return false;
            }
            else if (string.IsNullOrEmpty(base.Vm.SelfInfo.width.ToString()))
            {
                WesModernDialog.ShowWesMessage("請先輸入Length");
                return false;
            }
            else if (string.IsNullOrEmpty(base.Vm.SelfInfo.height.ToString()))
            {
                WesModernDialog.ShowWesMessage("請先輸入Length");
                return false;
            }
            if (!IsShippingToAvnet())
            {
                if (string.IsNullOrEmpty(base.Vm.SelfInfo.gw.ToString()))
                {
                    WesModernDialog.ShowWesMessage("請先輸入GW");
                    return false;
                }
            }
            else
            {
                base.Vm.SelfInfo.gw = "";
            }
            return true;
        }

        private bool IsCartonSizeInput()
        {
            if (string.IsNullOrEmpty(base.Vm.SelfInfo.gw.ToString()))
            {
                WesModernDialog.ShowWesMessage("請先輸入GW");
                return false;
            }
            return true;
        }

        [Ability(6432782857596309504, AbilityType.ScanAction)]
        public virtual void PalletEnd(string scanVal)
        {
            if (!DynamicUtil.IsExist(base.Vm.SelfInfo, "doingPackages"))
            {
                WesModernDialog.ShowWesMessage("未掃描任何packageId");
                return;
            }

            if (!_wistron_Consignee_CartonEnd.Contains(base.Vm.SelfInfo.consignee.ToString().ToUpper()))
            {
                if (!IsPalletSizeInput())
                {
                    return;
                }
            }

            if (base.Vm.SelfInfo.doingPackages == null || base.Vm.SelfInfo.doingPackages.Length == 0)
            {
                WesModernDialog.ShowWesMessage("未掃描任何packageId");
                return;
            }
            //if (_wistron_Consignee_CartonEnd.Contains(base.Vm.SelfInfo.consignee.ToString().ToUpper()))
            //{
            //    WesModernDialog.ShowWesMessage(string.Format("Consignee Code {0} 只允許 Carton End", base.Vm.SelfInfo.consignee.ToString()));
            //    return;
            //}

            var result = RestApi.NewInstance(Method.POST)
                            .AddUriParam(RestUrlType.WmsServer, UpdatePalletsScriptID, true)
                            .AddJsonBody("truckNo", base.Vm.SelfInfo.truckNo)
                            .AddJsonBody("length", base.Vm.SelfInfo.length)
                            .AddJsonBody("width", base.Vm.SelfInfo.width)
                            .AddJsonBody("height", base.Vm.SelfInfo.height)
                            .AddJsonBody("gw", base.Vm.SelfInfo.gw)
                            .AddJsonBody("endCustomer", base.Vm.SelfInfo.endCustomer)
                            .AddJsonBody("doingPackages", base.Vm.SelfInfo.doingPackages)
                            .Execute()
                            .To<Object>();
            if ((bool)result.state)
            {
                Vm.SelfInfo.kpiPackages = DynamicJson.DeserializeObject<dynamic>(DynamicJson.SerializeObject(Vm.SelfInfo.doingPackages));

                string palletNo = result.palletNo;
                this.Vm.SelfInfo.palletNo = palletNo;

                var labelInfo = RestApi.NewInstance(Method.POST)
                            .AddUriParam(RestUrlType.WmsServer, GetPalletLabelScriptID, false)
                            .AddJsonBody("truckNo", base.Vm.SelfInfo.truckNo)
                            .AddJsonBody("palletNo", base.Vm.SelfInfo.palletNo)
                            .Execute()
                            .To<object>();

                //打印板標籤
                List<PrintTemplateModel> templates = new List<PrintTemplateModel>();

                PrintTemplateModel pm = new PrintTemplateModel();
                pm.PrintData = labelInfo;
                pm.TemplateFileName = "PalletEndByTruck_Pallet.tff";
                pm.Mode = PrintMode.TFORMer;
                templates.Add(pm);

                //打印板头纸
                var palletLabelHeader = RestApi.NewInstance(Method.POST)
                           .AddUriParam(RestUrlType.WmsServer, GetPalletHeaderLabelScriptID, false)
                           .AddJsonBody("truckNo", base.Vm.SelfInfo.truckNo)
                           .AddJsonBody("palletNo", this.Vm.SelfInfo.palletNo)
                           .Execute()
                           .To<object>();
                var model = new PrintTemplateModel()
                {
                    PrintData = palletLabelHeader,
                    TemplateFileName = "AvnetPLT.tff",
                    Mode = PrintMode.TFORMer
                };
                templates.Add(model);

                LabelPrintBase lpb = new LabelPrintBase(templates, false);
                var res = lpb.Print();

                SetActionValid();
                PrepareData();
            }
        }

        private void PrepareData()
        {
            LoadingInfoByTruckNo(base.Vm.SelfInfo.truckNo);

            if (base.Vm.SelfInfo.undoPackages.Count == 0)
            {
                WesModernDialog.ShowWesMessageAsyc(string.Format("TruckNo:{0} 組板完成", base.Vm.SelfInfo.truckNo));
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_TRUCK_NO);
            }
            else
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            }
        }

        [Ability(6432783148219637760, AbilityType.ScanAction)]
        public virtual void CartonEnd(string scanVal)
        {
            if (!DynamicUtil.IsExist(base.Vm.SelfInfo, "doingPackages") || base.Vm.SelfInfo.doingPackages == null || base.Vm.SelfInfo.doingPackages.Length == 0)
            {
                WesModernDialog.ShowWesMessage("未掃描任何packageId");
                return;
            }
            //if (!IsCartonSizeInput())
            //{
            //    return;
            //}
            if (_wistron_Consignee_PalletEnd.Contains(base.Vm.SelfInfo.consignee.ToString().ToUpper()))
            {
                WesModernDialog.ShowWesMessage("Consignee Code C03576 只允許 Palllet End");
                return;
            }

            Vm.SelfInfo.kpiPackages = DynamicJson.DeserializeObject<dynamic>(DynamicJson.SerializeObject(Vm.SelfInfo.doingPackages));

            bool result = RestApi.NewInstance(Method.POST)
                            .AddUriParam(RestUrlType.WmsServer, UpdateCartonScriptID, true)
                            .AddJsonBody("truckNo", base.Vm.SelfInfo.truckNo)
                            .AddJsonBody("endCustomer", base.Vm.SelfInfo.endCustomer)
                            .AddJsonBody("gw", 0)
                            .AddJsonBody("doingPackages", base.Vm.SelfInfo.doingPackages)
                            .Execute()
                            .ToBoolean();
            if (result)
            {
                var labelInfo = RestApi.NewInstance(Method.POST)
                           .AddUriParam(RestUrlType.WmsServer, GetCartonLabelScriptID, false)
                           .AddJsonBody("truckNo", base.Vm.SelfInfo.truckNo)
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

                    //板头纸
                    var palletLabelHeader = RestApi.NewInstance(Method.POST)
                        .AddUriParam(RestUrlType.WmsServer, GetPalletHeaderLabelScriptID, false)
                        .AddJsonBody("truckNo", base.Vm.SelfInfo.truckNo)
                        .AddJsonBody("palletNo", item.PackageId.ToString())
                        .Execute()
                        .To<object>();

                    PrintTemplateModel tffModel = new PrintTemplateModel();
                    tffModel.PrintData = palletLabelHeader;
                    tffModel.TemplateFileName = "AvnetPLT.tff";
                    tffModel.Mode = PrintMode.TFORMer;
                    templates.Add(tffModel);
                }

                LabelPrintBase labelPrint = new LabelPrintBase(templates, false);
                labelPrint.Print();

                SetActionValid();
                PrepareData();
            }
        }

        #endregion

        public virtual bool ExecuteDelete(string scanVal)
        {
            if (_command == WesScanCommand.DELETE_PALLET)
            {
                DeleteP(scanVal);
                _command = WesScanCommand.NONE;
                return true;
            }
            return false;
        }

        private void DeleteP(string scanVal)
        {
            string[] scanStrs = scanVal.Split('-');
            if (scanStrs.Length > 1 && scanStrs[1].Substring(0, 1).ToUpper() == "P" && scanVal.Length == 18)
            {
                var delResult = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, DeletePalletScriptID, true)
                .AddQueryParameter("truckNo", base.Vm.SelfInfo.truckNo)
                .AddQueryParameter("endCustomer", base.Vm.SelfInfo.endCustomer)
                .AddQueryParameter("palletNo", scanVal)
                .Execute()
                .ToBoolean();

                this.Vm.SelfInfo.palletNo = scanVal;
                this.Vm.SelfInfo.palletPackageCount = GetPalletPackageCount(scanVal);

                LoadingInfoByTruckNo(base.Vm.SelfInfo.truckNo);
                if (delResult)
                {
                    WesModernDialog.ShowWesMessageAsyc(string.Format("刪除组板:{0}成功", scanVal));
                }
                else
                {
                    throw new WesException(string.Format("刪除板號:{0}失敗，請檢查掃描是否正確", scanVal));
                }
            }
            else
            {
                throw new WesException("板號格式錯誤,請掃描要刪除的板標籤");
            }
        }

        private int GetPalletPackageCount(string palletNo)
        {
            int packageCount = 0;
            foreach (var item in base.Vm.SelfInfo.donePackages)
            {
                if (item != null && string.Compare(item.CPkgID.ToString(), palletNo, true) == 0)
                {
                    packageCount++;
                }
            }
            return packageCount;
        }

        private void RefreshScanErrorList()
        {
            dynamic result = RestApi.NewInstance(Method.POST)
                            .AddUriParam(RestUrlType.WmsServer, GetErrorScanScriptID, false)
                            .AddJsonBody("truckNo", base.Vm.SelfInfo.truckNo)
                            .Execute()
                            .To<object>();
            base.Vm.SelfInfo.errorPackages = result;
        }

        [Ability(6437900147731996672, AbilityType.ScanAction)]
        public virtual void RePrint(object obj)
        {
            Button btn = null;
            if (obj is RoutedEventArgs)
            {
                btn = (obj as RoutedEventArgs).Source as Button;
            }

            if (btn != null && MasterAuthorService.Authorization())
            {
                btn.IsEnabled = false;
                dynamic val = btn.Tag as dynamic;

                string palletNo = string.Empty;
                if (val.CPkgID == null || string.IsNullOrWhiteSpace(val.CPkgID.ToString()))
                {
                    palletNo = val.PackageID.ToString();
                }
                else
                {
                    palletNo = val.CPkgID.ToString();
                }

                //打印板头纸
                RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, GetPalletHeaderLabelScriptID, false)
                .AddJsonBody("truckNo", base.Vm.SelfInfo.truckNo)
                .AddJsonBody("palletNo", palletNo)
                .ExecuteAsync(new Action<bool, Exception, RestApi>((res, ex, restApi) =>
                {
                    if (restApi != null)
                    {
                        var palletLabelHeader = restApi.To<object>();
                        var model = new PrintTemplateModel()
                        {
                            PrintData = palletLabelHeader,
                            TemplateFileName = "AvnetPLT.tff",
                            Mode = PrintMode.TFORMer
                        };

                        Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            LabelPrintBase tffPrint = new LabelPrintBase(model, false);
                            tffPrint.PrintParam.DefaultCheck = false;
                            tffPrint.Print();

                            btn.IsEnabled = true;
                        }));
                    }
                    else
                    {
                        throw ex;
                    }
                }));

            }
        }

        public virtual void DataGridLoadingRow(DataGridRowEventArgs e)
        {
            dynamic obj = e.Row.Item as dynamic;
            if (obj != null && obj.PartNo != null)
            {
                if (String.Compare(obj.PartNo.ToString(), _packagePartNo, true) == 0)
                {
                    e.Row.Style = Application.Current.FindResource("DataGridCellHighlightStyle") as Style;
                }
                else
                {
                    e.Row.Style = Application.Current.FindResource("DataGridCellDefaultStyle") as Style;
                }
            }
        }
    }
}
