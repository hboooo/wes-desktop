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

namespace Wes.Component.Widgets.Action
{
    public class PalletizationAction : ScanActionBase<WesFlowID, PalletizationAction>, IScanAction
    {
        private string _command = WesScanCommand.NONE;

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        public static readonly string WesImageUri = @"http://wms.spreadlogistics.com/UploadFile/Shipping/LabelRequest/";

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
            RestApi.NewInstance(Method.GET)
            .AddUri("palletization/operation-images")
            .AddQueryParameter("truckOrder", scanVal)
            .ExecuteAsync((res, ex, restApi) =>
            {
                if (restApi != null)
                {
                    dynamic imageList = restApi.To<object>();
                    if (imageList != null)
                    {
                        List<object> datas = new List<object>();
                        foreach (var item in imageList)
                        {
                            datas.Add(new
                            {
                                ImageUri = item.photoUrl.ToString(),
                                FileName = item.fileName.ToString(),
                                Desc = Path.GetFileNameWithoutExtension(item.fileName.ToString()),
                                Remark = item.remarks.ToString()
                            });
                        }
                        this.Vm.SelfInfo.imageViewList = datas.Count > 0 ? datas : null;
                    }
                }
            });
        }

        [Ability(6417633559632158720, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanLoadingNo(string scanVal)
        {
            LoadingInfoByTruckOrder(scanVal);
            GetOperatorImageList(scanVal);
        }

        private void LoadingInfoByTruckOrder(string scanVal)
        {
            _packagePartNo = null;
            dynamic cartons = RestApi.NewInstance(Method.GET)
                        .AddUri("palletization/check-txt")
                        .AddQueryParameter("truckOrder", scanVal)
                        .Execute()
                        .To<object>();

            if (scanVal.ToLower().StartsWith("sxt"))
                base.Vm.SelfInfo.loadingNoText = "LoadingNo";
            else
                base.Vm.SelfInfo.loadingNoText = "TruckOrder";

            base.Vm.SelfInfo.undoPackages = cartons.undoPackages;
            base.Vm.SelfInfo.doingPackages = new List<dynamic>().ToArray();
            base.Vm.SelfInfo.donePackages = cartons.donePackages;
            base.Vm.SelfInfo.errorPackages = cartons.errorPackages;
            base.Vm.SelfInfo.truckOrder = scanVal;
            base.Vm.SelfInfo.consignee = cartons.consignee;
            base.Vm.SelfInfo.endCustomer = cartons.endCustomer;
            base.Vm.SelfInfo.withoutCheck = "false";

            RefreshCombineCount();
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        [AbilityAble(6420101420141256704, AbilityType.ScanAction, true, KPIActionType.LSCombinePallet | KPIActionType.LSCombinePalletPlus, "consignee", true)]
        public virtual bool ScanFlowActionScanPackageId(string scanVal)
        {
            if (ExecuteDelete(scanVal))
            {
                return true;
            }

            if (IsPackageDoing(scanVal))
            {
                WesDesktopSounds.Alarm();
                WesModernDialog.ShowWesMessage("Package正在組板中，請重新掃描");
                return false;
            }

            dynamic result = default(dynamic);
            if (base.Vm.SelfInfo.withoutCheck == "false")
            {
                try
                {
                    result = RestApi.NewInstance(Method.GET)
                                .AddUri("palletization/check-packageid")
                                .AddQueryParameter("truckOrder", base.Vm.SelfInfo.truckOrder)
                                .AddQueryParameter("packageId", scanVal)
                                .Execute()
                                .To<object>();
                }
                catch (Exception ex)
                {
                    WesDesktopSounds.Alarm();
                    throw ex;
                }
            }
            else
            {
                try
                {
                    result = RestApi.NewInstance(Method.PUT)
                            .AddUri("palletization/without-check-packageid")
                            .AddJsonBody("truckOrder", base.Vm.SelfInfo.truckOrder)
                            .AddJsonBody("packageId", scanVal)
                            .Execute()
                            .To<object>();
                }
                catch (Exception ex)
                {
                    WesDesktopSounds.Alarm();
                    throw ex;
                }
            }
            if ((bool)result.state)
            {
                if (result.partNos != null && result.partNos.Count > 0)
                {
                    foreach (var item in result.partNos)
                    {
                        _packagePartNo = item.PartNo.ToString();
                    }
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
                WesDesktopSounds.Failed();
                RefreshScanErrorList();
                IsPackageDone(scanVal);
                WesModernDialog.ShowWesMessage(result.message.ToString());
            }
            return false;
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
                if (item != null && string.Compare(item.packageID.ToString(), scanVal, true) == 0)
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
                if (item != null && string.Compare(item.packageID.ToString(), scanVal, true) == 0)
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
                if (item != null && string.Compare(item.packageID.ToString(), scanVal, true) == 0)
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
            string truckOrder = null;
            try
            {
                truckOrder = base.Vm.SelfInfo.truckOrder;
            }
            catch { };
            if (string.IsNullOrEmpty(truckOrder))
            {
                WesModernDialog.ShowWesMessage("請先輸入Loading No或者Truck Order");
            }
            else
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
                _command = WesScanCommand.DELETE_PALLET;
                string message = "請掃描要刪除的板號";
                base.Vm.Tooltip = message;
            }
        }

        [AbilityAble(6437537634364428288, AbilityType.ScanAction, true, KPIActionType.LSCombineCarton | KPIActionType.LSCombineCartonPlus, "consignee", true)]
        public virtual bool DeleteCarton(string scanVal)
        {
            string truckOrder = null;
            try
            {
                truckOrder = base.Vm.SelfInfo.truckOrder;
            }
            catch { };
            if (string.IsNullOrEmpty(truckOrder))
            {
                throw new WesException("請先輸入Loading No或者Truck Order");
            }
            Vm.SelfInfo.deleteKpiPackages = DynamicJson.DeserializeObject<dynamic>(DynamicJson.SerializeObject(Vm.SelfInfo.donePackages));
            var delResult = RestApi.NewInstance(Method.DELETE)
                .AddUri("palletization/combined-carton")
                .AddQueryParameter("truckOrder", truckOrder)
                .Execute()
                .ToBoolean();
            LoadingInfoByTruckOrder(base.Vm.SelfInfo.truckOrder);
            if (delResult)
            {
                WesModernDialog.ShowWesMessage(string.Format("刪除组箱:{0}成功", truckOrder));
            }
            else
            {
                throw new WesException(string.Format("刪除组箱:{0}失败，请联系管理员", truckOrder));
            }
            _command = WesScanCommand.NONE;
            return true;
        }

        [AbilityAble(6432782857596309504, AbilityType.ScanAction, true, KPIActionType.LSCombinePallet | KPIActionType.LSCombinePalletPlus, "consignee")]
        public virtual bool PalletEnd(string scanVal)
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

        protected void PrepareData()
        {
            LoadingInfoByTruckOrder(base.Vm.SelfInfo.truckOrder);

            if (base.Vm.SelfInfo.undoPackages.Count == 0)
            {
                WesModernDialog.ShowWesMessageAsyc(string.Format("{0}:{1} 組板完成", base.Vm.SelfInfo.loadingNoText, base.Vm.SelfInfo.truckOrder));
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOADING_NO);
            }
            else
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            }
        }

        [AbilityAble(6432783148219637760, AbilityType.ScanAction, true, KPIActionType.LSCombineCarton | KPIActionType.LSCombineCartonPlus, "consignee")]
        public virtual bool CartonEnd(string scanVal)
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
                var delResult = RestApi.NewInstance(Method.DELETE)
                .AddUri("palletization/combined-pallet")
                .AddJsonBody("truckOrder", base.Vm.SelfInfo.truckOrder)
                .AddJsonBody("endCustomer", base.Vm.SelfInfo.endCustomer)
                .AddJsonBody("palletNo", scanVal)
                .Execute()
                .ToBoolean();

                this.Vm.SelfInfo.palletNo = scanVal;
                this.Vm.SelfInfo.palletPackageCount = GetPalletPackageCount(scanVal);

                LoadingInfoByTruckOrder(base.Vm.SelfInfo.truckOrder);
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
                if (item != null && string.Compare(item.cPkgID.ToString(), palletNo, true) == 0)
                {
                    packageCount++;
                }
            }
            return packageCount;
        }

        private void RefreshScanErrorList()
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                            .AddUri("palletization/error")
                            .AddQueryParameter("truckOrder", base.Vm.SelfInfo.truckOrder)
                            .Execute()
                            .To<object>();
            base.Vm.SelfInfo.errorPackages = result;
        }

        public virtual void DataGridLoadingRow(DataGridRowEventArgs e)
        {
            dynamic obj = e.Row.Item as dynamic;
            if (obj != null && obj.partNo != null)
            {
                if (String.Compare(obj.partNo.ToString(), _packagePartNo, true) == 0)
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
