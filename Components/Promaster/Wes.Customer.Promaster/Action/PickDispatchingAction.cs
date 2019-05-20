using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Wes.Component.Widgets.APIAddr;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.Promaster.Model;
using Wes.Customer.Promaster.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action
{
    /// <summary>
    /// 采集指派接口类
    /// </summary>
    public class PickDispatchingAction : ScanActionBase<WesFlowID, PickDispatchingAction>, IScanAction
    {
        private readonly List<PickDispatchingModel> printList = new List<PickDispatchingModel>();
        private PickDispatchingModel _currScanModel; //當前掃描數據
        private bool _scanCmd=false;//判断是否扫描了指令
        private IEnumerable<PickDispatchingModel> _scannedList = new List<PickDispatchingModel>(); //已完成
        private IEnumerable<PickDispatchingModel> _scanningList = new List<PickDispatchingModel>(); //作業中
        private IEnumerable<CommonSowModel> _sowList = new List<CommonSowModel>(); //播種列表
        private IEnumerable<PickDispatchingModel> _unScanList = new List<PickDispatchingModel>(); //所有待作業明細
        public string Command { get; set; } = WesScanCommand.NONE;

        #region 扫描指令或者PartNo

        public virtual void ScanFlowActionScanCommandOrPn(string scanValue)
        {
            if (scanValue.ToUpper().Equals("BOXSTART"))
            {
                _scanCmd = true;
                ScanFlowActionScanCommand(scanValue);
            }
            else
            {
                _scanCmd = false;
                ScanFlowActionScanPnOrQrcode(scanValue);
            }
        }

        #endregion

        #region 掃描DC

        public virtual void ScanFlowActionScanDcNo(string scanValue)
        {
            Vm.SelfInfo.dc = scanValue;
            dynamic dcResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/dc")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            var dc = dcResult.dc.ToString();
            var isExistDC = _scanningList.Any(a =>
                a.PartNo.Equals(_currScanModel.PartNo, StringComparison.OrdinalIgnoreCase)
                && a.DateCode.Equals(dc, StringComparison.OrdinalIgnoreCase));
            if (!isExistDC)
            {
                WesModernDialog.ShowWesMessage("掃描DC失敗，失敗原因：" + scanValue + " 無效！");
                return;
            }

            _currScanModel.DateCode = dc;
            if (Vm.SelfInfo.supplier.ToString() == "C04463")
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY);
            }
            else
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
            }
        }

        #endregion

        #region 掃描LotNO

        public virtual void ScanFlowActionScanLotNo(string scanValue)
        {
            if (scanValue.StartsWith("1T", StringComparison.OrdinalIgnoreCase))
            {
                scanValue = scanValue.Right(scanValue.Length - 2);
            }

            var isExistLotNo = _scanningList.Any(a =>
                a.PartNo.Equals(_currScanModel.PartNo, StringComparison.OrdinalIgnoreCase)
                && a.DateCode.Equals(_currScanModel.DateCode, StringComparison.OrdinalIgnoreCase)
                && a.LotNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase)
            );
            if (!isExistLotNo)
            {
                WesModernDialog.ShowWesMessage("掃描LotNO失敗，失敗原因：" + scanValue + " 無效！");
                return;
            }

            _currScanModel.LotNo = scanValue;

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY);
        }

        #endregion

        #region 掃描Qty

        public virtual void ScanFlowActionScanQty(string scanValue)
        {
            if (scanValue.StartsWith("Q", StringComparison.OrdinalIgnoreCase))
            {
                scanValue = scanValue.Right(scanValue.Length - 1);
            }

            int _qty = 0;
            if (!int.TryParse(scanValue, out _qty) || _qty <= 0)
            {
                WesModernDialog.ShowWesMessage("Qty格式不正確！");
                return;
            }

            var scanQty = Convert.ToInt32(scanValue);
            //if (scanQty > Convert.ToInt32(Vm.SelfInfo.currentTotalQty))
            //{
            //    WesModernDialog.ShowWesMessage("輸入Qty已經大於該箱Qty，請確認！");
            //    return;
            //}
            _currScanModel.Qty = scanQty;
            if (Vm.SelfInfo.supplier.ToString() == "C04460" && Vm.SelfInfo.ScanFW.ToString() == "Y")
            {
                //還需要根據料號判斷料號基礎表裡的SeriesNO是否為Y，Y代表要掃FW，否則不掃
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_FW, "FW");
            }
            else
            {
                if (_scanCmd)
                {
                    Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COMMAND);
                }
                else
                {
                    BoxEnd("BOXEND");
                }
            }
        }

        #endregion

        #region 掃描FW((只針對Supplier=C04460）

        public virtual void ScanFlowActionScanFw(string scanValue)
        {
            Vm.SelfInfo.fw = scanValue;
            Vm.SelfInfo.isShipping = true;
            dynamic dcResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/fw")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            var fw = dcResult.fw.ToString();
            var matchItem = _scanningList.Where(a =>
                a.PartNo.Equals(_currScanModel.PartNo, StringComparison.OrdinalIgnoreCase)
                && a.DateCode.Equals(_currScanModel.DateCode, StringComparison.OrdinalIgnoreCase)
                && a.LotNo.Equals(_currScanModel.LotNo, StringComparison.OrdinalIgnoreCase)
                && a.BatchNo.Equals(fw, StringComparison.OrdinalIgnoreCase)
            );
            if (!matchItem.Any())
            {
                WesModernDialog.ShowWesMessage(string.Format("掃描FW失敗，FW:{0}無效！", scanValue));
                return;
            }

            _currScanModel.BatchNo = fw;
            if (_scanCmd)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COMMAND);
            }
            else
            {
                BoxEnd("BOXEND");
            }
        }

        #endregion

        /// <summary>
        /// Check 新箱号
        /// </summary>
        /// <param name="scanValue"></param>
        [Ability(6525914036842496, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanCheckNewCartonNo(string scanValue)
        {
            if (ExecuteCartonEnd(scanValue))
            {
                return;
            }

            if (_currScanModel != null &&
                !_currScanModel.NCartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase))
            {
                WesModernDialog.ShowWesMessage("校驗箱號失敗，失敗原因：" + scanValue + "無效,應為:" + _currScanModel.NCartonNo + "！");
                return;
            }

            Vm.SelfInfo.NCartonNo = scanValue;
            Vm.SelfInfo.Pid = _currScanModel.CartonNo;
            Vm.SelfInfo.Qty = 1;
            IsScanFinished(_currScanModel.CartonNo);
        }

        private bool ExecuteCartonEnd(string scanValue)
        {
            if (Command == WesScanCommand.CARTON_END)
            {
                Vm.SelfInfo.NCartonNo = scanValue;
                RestApi.NewInstance(Method.POST)
                    .AddUri("/dispatching/carton-end")
                    .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                    .AddJsonBody("NewCartonNo", Vm.SelfInfo.NCartonNo.ToString())
                    .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                    .Execute();

                AddKpiPlus();
                LoadSowInfo(Vm.SelfInfo.Pxt.ToString());
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
                Command = WesScanCommand.NONE;
                return true;
            }

            return false;
        }

        [AbilityAble(6525914141704192, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCartonPlus, "Target",
            false)]
        protected virtual bool AddKpiPlus()
        {
            Vm.SelfInfo.Pid = Vm.SelfInfo.NCartonNo.ToString();
            Vm.SelfInfo.Qty = 1;
            return true;
        }

        /// <summary>
        /// 判断是否完成
        /// </summary>
        /// <param name="cartonNo"></param>
        private void IsScanFinished(string cartonNo)
        {
            var isExist = _scanningList.Any(a => a.CartonNo.Equals(cartonNo, StringComparison.OrdinalIgnoreCase));
            if (isExist)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COMMAND_OR_PN);
            }
            else
            {
                UpdateStorage(cartonNo);
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO);
            }

            if (!_unScanList.Any())
            {
                CartonEnd(Vm.SelfInfo.NCartonNo); //最后一箱自动Cartonend
                Vm.SelfInfo.scanningList = null;
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
            }
            else
            {
                //系统并箱
                PickDispatchingModel pdModel = (PickDispatchingModel)Vm.SelfInfo.pdModel;
                if (!string.IsNullOrEmpty(pdModel.CombineNo))
                {
                    isExist = _unScanList.Any(pd => pd.LoadingNo.Equals(Vm.SelfInfo.LoadingNo.ToString(), StringComparison.CurrentCultureIgnoreCase)
                        && pd.CombineNo.Equals(pdModel.CombineNo.ToString(),
                            StringComparison.InvariantCultureIgnoreCase));
                }
                //按CPO+CPN分箱,一箱有且只能有一个DC
                else if (Vm.SelfInfo.Target.ToString() == "C04476")
                {
                    isExist = _unScanList.Any(a =>
                        a.OrderNo.Equals(Vm.SelfInfo.CPO.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && a.CustomerPN.Equals(Vm.SelfInfo.CPN.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && a.PartNo.Equals(Vm.SelfInfo.PN.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && a.DateCode.Equals(Vm.SelfInfo.DC.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && a.LoadingNo.Equals(Vm.SelfInfo.LoadingNo.ToString(), StringComparison.CurrentCultureIgnoreCase));
                }
                //按CPN分箱
                else
                {
                    isExist = _unScanList.Any(a => a.CustomerPN.Equals(Vm.SelfInfo.CPN.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && a.PartNo.Equals(Vm.SelfInfo.PN.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && a.LoadingNo.Equals(Vm.SelfInfo.LoadingNo.ToString(), StringComparison.CurrentCultureIgnoreCase));
                }

                if (!isExist)
                {
                    CartonEnd(Vm.SelfInfo.NCartonNo);
                }
            }
        }

        /// <summary>
        /// 更新储位
        /// </summary>
        /// <param name="cartonNo"></param>
        [AbilityAble(6525914322046976, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCarton, "Target",
            false)]
        protected virtual bool UpdateStorage(string cartonNo)
        {
            //判断是否需要更新储位
            string yushuQty = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId((long)ScriptSid.GET_YUSHU_QTY)
                .AddQueryParameter("pid", cartonNo)
                .Execute()
                .To<string>();
            if (!string.IsNullOrEmpty(yushuQty) && Convert.ToInt32(yushuQty) > 0)
            {
                //余数Qty大于0，代表该箱还有余数，需更新储位
                var updateModel = new CommonUpdateBinNoModel
                {
                    ActionType = "pick",
                    OperationNo = Vm.SelfInfo.Pxt,
                    PackageId = cartonNo,
                    YushuQty = yushuQty,
                    UpdateUser = WesDesktop.Instance.User.Code
                };
                var updateBinNoWin = new WesUpdateBinNo(updateModel);
                updateBinNoWin.ShowDialog();
            }

            return true;
        }

        /// <summary>
        /// CartonEnd并箱
        /// </summary>
        /// <param name="scanVal"></param>
        [Ability(6525914414329856, AbilityType.ScanAction)]
        public virtual void CartonEnd(string scanVal)
        {
            if (string.IsNullOrEmpty(Vm.SelfInfo.NCartonNo.ToString()))
            {
                WesModernDialog.ShowWesMessage("請掃描新箱號!");
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
                Command = WesScanCommand.CARTON_END;
                return;
            }

            RestApi.NewInstance(Method.POST)
                .AddUri("/dispatching/carton-end")
                .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                .AddJsonBody("NewCartonNo", Vm.SelfInfo.NCartonNo.ToString())
                .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                .Execute();
            AddKpiPlus();
            Vm.SelfInfo.selectCartonId = string.Empty;
            LoadSowInfo(Vm.SelfInfo.Pxt.ToString());
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="rowId"></param>
        [Ability(6525914850529280, AbilityType.ScanAction)]
        public virtual void DeleteData(long rowId)
        {
            string cartonNo = RestApi.NewInstance(Method.DELETE)
                .AddUri("/dispatching")
                .AddJsonBody("rowId", rowId)
                .AddJsonBody("user", WesDesktop.Instance.User.Code)
                .Execute()
                .To<string>();
            LoadSowInfo(Vm.SelfInfo.Pxt);
            Vm.SelfInfo.Pid = cartonNo;
            DeleteDataForKPI();
        }

        protected virtual bool DeleteDataForKPI()
        {
            return true;
        }

        /// <summary>
        /// 已完成数据查询
        /// </summary>
        [Ability(6525914502418432, AbilityType.ScanAction)]
        public virtual void SearchData()
        {
            Func<PickDispatchingModel, bool> whereFunc1 = a => true;
            Func<PickDispatchingModel, bool> whereFunc2 = a => true;
            var sowView = Vm as PickDispatchingViewModel;
            if (!string.IsNullOrEmpty(sowView.SearchNPid))
                whereFunc1 = (a => a.NCartonNo.Equals(sowView.SearchNPid, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(sowView.SearchPN))
                whereFunc2 = (a => a.PartNo.Equals(sowView.SearchPN, StringComparison.InvariantCultureIgnoreCase));
            var lstData = _scannedList.Where(whereFunc1).Where(whereFunc2);
            Vm.SelfInfo.scannedList = lstData;
        }

        [Ability(6525914665975808, AbilityType.ScanAction)]
        private void PrintNewCartonNo(string nCartonNo, string loadingNo)
        {
            if (_scannedList.Count(a => a.NCartonNo.Equals(nCartonNo, StringComparison.CurrentCultureIgnoreCase)) == 1)
            {
                //如果是第一次則打印新箱號
                var pm = new PrintTemplateModel
                {
                    PrintDataValues = new Dictionary<string, object>
                    {
                        {"NCartonNo", nCartonNo},
                        {"LoadingNo", loadingNo},
                        {"RowID", Vm.SelfInfo.selectIndex.ToString()}
                    },
                    TemplateFileName = "Avnet_NewCartonLabel.btw"
                };
                var lpb = new LabelPrintBase(pm, false);
                var res = lpb.Print();
            }
        }

        [Ability(6526304165842944, AbilityType.ScanAction)]
        private void PrintLabel()
        {
            //打印原廠標籤和餘數箱標籤
            //最多出三張：盒，袋，管
            //至少出一張
            var minQty = printList.Min(a => a.Qty);
            var pm = new PrintTemplateModel();
            foreach (var model in printList)
            {
                int printQty = model.Qty - minQty;
                if (printQty == 0)
                {
                    printQty = minQty;
                }

                if (string.IsNullOrEmpty(model.LotNo))
                {
                    //說明沒有掃LotNo
                    pm.PrintDataValues = new Dictionary<string, object>
                    {
                        {"PartNo", model.PartNo.ToUpper()},
                        {"DateCode", model.DateCode},
                        {"OriginCountry",   Vm.SelfInfo.COO},
                        {"Qty", printQty},
                        {"Supplier", Vm.SelfInfo.supplierName}
                    };
                    pm.TemplateFileName = "PM_Remainder_NotIncludeLot.btw";
                }
                else
                {
                    if (string.IsNullOrEmpty(model.BatchNo))
                    {
                        //說明沒有掃FW
                        pm.PrintDataValues = new Dictionary<string, object>
                        {
                            {"PartNo", model.PartNo.ToUpper()},
                            {"LotNo", model.LotNo},
                            {"DateCode", model.DateCode},
                            {"OriginCountry",  Vm.SelfInfo.COO},
                            {"Qty", printQty},
                            {"Supplier", Vm.SelfInfo.supplierName}
                        };
                        pm.TemplateFileName = "PM_Remainder_NotIncludeFW.btw";
                    }
                    else
                    {
                        pm.PrintDataValues = new Dictionary<string, object>
                        {
                            {"PartNo", model.PartNo.ToUpper()},
                            {"LotNo", model.LotNo},
                            {"DateCode", model.DateCode},
                            {"FW", model.BatchNo},
                            {"OriginCountry",  Vm.SelfInfo.COO},
                            {"Qty", printQty},
                            {"Supplier", Vm.SelfInfo.supplierName}
                        };
                        pm.TemplateFileName = "PM_Remainder_IncludeAll.btw";
                    }
                }

                var lpb = new LabelPrintBase(pm, false);
                var res = lpb.Print();
            }
        }

        #region 扫描PXT

        /// <summary>
        /// 扫描 FlowActionScanPickingNo
        /// </summary>
        /// <param name="scanValue"></param>
        /// <exception cref="WesException"></exception>
        [Ability(6525913109897216, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {
            printList.Clear();
            _currScanModel = null;

            if (scanValue.ToUpper().Equals("CARTONEND"))
            {
                if (!string.IsNullOrEmpty(Vm.SelfInfo.Pxt.ToString()))
                {
                    WesModernDialog.ShowWesMessage("請掃描新箱號!");
                    Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
                    Command = WesScanCommand.CARTON_END;
                    return;
                }
            }

            if (!scanValue.IsPxt())
            {
                throw new WesException("Pxt 不合法");
            }

            string result = RestApi.NewInstance(Method.GET)
                .AddUri("/dispatching/check-pxt")
                .AddQueryParameter("pxt", scanValue)
                .Execute()
                .To<string>();
            string sxt = RestApi.NewInstance(Method.GET)
                .SetWmsGlobalUri("/receiving/sxt-by-pxt")
                .AddQueryParameter("pxt", scanValue)
                .Execute()
                .To<string>();
            Vm.SelfInfo.Target = result;
            Vm.SelfInfo.Pxt = scanValue;
            Vm.SelfInfo.sxt = sxt;
            LoadSowInfo(scanValue);
            if (!_unScanList.Any()) return;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO);
        }

        /// <summary>
        /// 加载信息
        /// </summary>
        /// <param name="scanValue"></param>
        [Ability(6525908240322560, AbilityType.ScanAction)]
        private void LoadSowInfo(string scanValue)
        {
            DataSet result = RestApi.NewInstance(Method.GET)
                .AddUri("/dispatching/load-detail")
                .AddQueryParameter("pxt", scanValue)
                .Execute()
                .To<DataSet>();
            _unScanList = result.Tables[0].ToList<PickDispatchingModel>();
            _sowList = result.Tables[1].ToList<CommonSowModel>();
            _scannedList = result.Tables[2].ToList<PickDispatchingModel>();
            Vm.SelfInfo.unScanList = _unScanList;
            Vm.SelfInfo.scannedList = _scannedList;
            if (_sowList.Any())
            {
                Vm.SelfInfo.ucSowList = _sowList;
            }

            int qtyScanned = _scannedList.Sum(a => a.Qty);
            int qtyUnScanned = _unScanList.Sum(a => a.Qty);
            Vm.SelfInfo.totalQty = qtyScanned + "/" + (qtyScanned + qtyUnScanned);
        }

        #endregion

        #region 扫描原箱号

        /// <summary>
        /// 扫描 FlowActionScanCartonNo
        /// </summary>
        /// <param name="scanValue"></param>
        public virtual void ScanFlowActionScanCartonNo(string scanValue)
        {
            var isExistCartonNo = _unScanList.Any(a => a.CartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
            if (!isExistCartonNo)
            {
                WesModernDialog.ShowWesMessage("掃描原箱號失敗，失敗原因:" + scanValue + " 無效！");
                return;
            }
            _currScanModel = null;
            var currentScanCarton = _unScanList.First(a => a.CartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
            Vm.SelfInfo.currentPid = scanValue;
            Vm.SelfInfo.supplier = currentScanCarton.Shipper;
            Vm.SelfInfo.supplierName = currentScanCarton.ShipperName;
            Vm.SelfInfo.COO = currentScanCarton.OriginCountry;
            Vm.SelfInfo.pid = scanValue;
            BindCurrentCartonScanList(scanValue);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COMMAND_OR_PN);
        }

        private void BindCurrentCartonScanList(string cartonNo)
        {
            var dsCur = _unScanList.Where(a => a.CartonNo.Equals(cartonNo, StringComparison.InvariantCultureIgnoreCase));
            _scanningList = dsCur;
            Vm.SelfInfo.currentTotalQty = _scanningList.Sum(a => a.Qty).ToString();
            Vm.SelfInfo.scanningList = _scanningList;
        }

        #endregion

        #region 扫描指令

        /// <summary>
        /// </summary>
        /// <param name="scanValue"></param>
        public virtual void ScanFlowActionScanCommand(string scanValue)
        {
            List<string> cmdList = new List<string>
            {
                "BOXSTART",
                "BOXEND"
            };
            if (!cmdList.Contains(scanValue.ToUpper()))
            {
                WesModernDialog.ShowWesMessage("指令輸入錯誤,請確認！");
                return;
            }

            if (scanValue.Equals("BOXSTART", StringComparison.InvariantCultureIgnoreCase))
            {
                if (_currScanModel != null)
                {
                    printList.Add(_currScanModel);
                }
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PN_OR_QRCODE);
            }
        }

        public virtual void BoxEnd(string scanValue)
        {
            var matchItem = _scanningList.Where(a => a.PartNo.Equals(_currScanModel.PartNo, StringComparison.OrdinalIgnoreCase)
                                                     && a.DateCode.Equals(_currScanModel.DateCode, StringComparison.OrdinalIgnoreCase)
                                                     && a.Qty >= _currScanModel.Qty);
            if (!matchItem.Any())
            {
                WesModernDialog.ShowWesMessage(string.Format("掃描Qty失敗，Qty:{0}無效！", _currScanModel.Qty));
                return;
            }

            if (!string.IsNullOrEmpty(_currScanModel.LotNo))
            {
                matchItem = _scanningList.Where(a =>
                    a.PartNo.Equals(_currScanModel.PartNo, StringComparison.OrdinalIgnoreCase)
                    && a.DateCode.Equals(_currScanModel.DateCode, StringComparison.OrdinalIgnoreCase)
                    && a.LotNo.Equals(_currScanModel.LotNo, StringComparison.OrdinalIgnoreCase)
                    && a.Qty >= _currScanModel.Qty);
            }

            if (!string.IsNullOrEmpty(_currScanModel.BatchNo))
            {
                matchItem = _scanningList.Where(a =>
                    a.PartNo.Equals(_currScanModel.PartNo, StringComparison.OrdinalIgnoreCase)
                    && a.DateCode.Equals(_currScanModel.DateCode, StringComparison.OrdinalIgnoreCase)
                    && a.LotNo.Equals(_currScanModel.LotNo, StringComparison.OrdinalIgnoreCase)
                    && a.BatchNo.Equals(_currScanModel.BatchNo, StringComparison.OrdinalIgnoreCase)
                    && a.Qty >= _currScanModel.Qty);
            }

            var qty = _currScanModel.Qty;
            _currScanModel = matchItem.First();
            _currScanModel.Qty = qty;
            printList.Add(_currScanModel);
            SaveScan(_currScanModel);
        }


        [Ability(6447732581789081600, AbilityType.ScanAction)]
        private void SaveScan(PickDispatchingModel model)
        {
            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUri("/dispatching/save-data")
                .AddJsonBody("pxt", Vm.SelfInfo.Pxt)
                .AddJsonBody("sxt", model.LoadingNo)
                .AddJsonBody("pid", model.CartonNo)
                .AddJsonBody("pn", model.PartNo)
                .AddJsonBody("dc", model.DateCode)
                .AddJsonBody("cpn", model.CustomerPN)
                .AddJsonBody("cpo", model.OrderNo)
                .AddJsonBody("lot", model.LotNo)
                .AddJsonBody("rowId", model.ItemNo)
                .AddJsonBody("batchNo", model.BatchNo)
                .AddJsonBody("combineNo", model.CombineNo)
                .AddJsonBody("qty", Convert.ToInt32(model.Qty))
                .AddJsonBody("user", WesDesktop.Instance.User.Code)
                .AddJsonBody("consignee", Vm.SelfInfo.Target.ToString())
                .Execute()
                .To<object>();
            string nCartonNo = result.nCartonNo.ToString();
            LoadSowInfo(Vm.SelfInfo.Pxt.ToString());
            Vm.SelfInfo.selectCartonId = nCartonNo;
            Vm.SelfInfo.LoadingNo = model.LoadingNo;
            Vm.SelfInfo.CPN = model.CustomerPN;
            Vm.SelfInfo.CPO = model.OrderNo;
            Vm.SelfInfo.PN = model.PartNo;
            Vm.SelfInfo.DC = model.DateCode;
            Vm.SelfInfo.pdModel = model;
            PrintNewCartonNo(nCartonNo, model.LoadingNo);
            if (_scanCmd)
            {
                PrintLabel();
            }
            printList.Clear();
            _currScanModel.NCartonNo = nCartonNo;
            BindCurrentCartonScanList(_currScanModel.CartonNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
        }

        #endregion

        #region 掃描PN

        [Ability(6525997344104448, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPnOrQrcode(string scanValue)
        {
            if (scanValue.Contains(" "))
            {
                scanValue = scanValue.Replace(" ", "");
            }
            if (scanValue.StartsWith("1P", StringComparison.OrdinalIgnoreCase))
            {
                scanValue = scanValue.Right(scanValue.Length - 2);
            }
            else if (scanValue.StartsWith("P", StringComparison.OrdinalIgnoreCase))
            {
                scanValue = scanValue.Right(scanValue.Length - 1);
            }

            if (scanValue.Length>30)
            {
                ScanQrCode(scanValue);
            }
            else
            {
                var isExistPartNo = _scanningList.Any(a => a.PartNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
                if (!isExistPartNo)
                {
                    WesModernDialog.ShowWesMessage("掃描料號失敗，失敗原因：" + scanValue + " 無效！");
                    return;
                }

                _currScanModel = new PickDispatchingModel
                {
                    CartonNo = Vm.SelfInfo.currentPid.ToString(),
                    PartNo = scanValue
                };
                var currentCarton = _scanningList.First(a => a.PartNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
                Vm.SelfInfo.ScanFW = currentCarton.NeedScanFW;
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO);
            }
        }

        public virtual void ScanQrCode(string qrCode)
        {
            Vm.SelfInfo.qrCode = qrCode;
            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("/resolver/qc")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            var matchItem = _scanningList.Where(a => a.PartNo.Equals(qtyResult.pn.ToString(), StringComparison.OrdinalIgnoreCase)
                                                     && a.LotNo.Equals(qtyResult.lot.ToString(), StringComparison.OrdinalIgnoreCase)
                                                     && a.DateCode.Equals(qtyResult.dc.ToString(), StringComparison.OrdinalIgnoreCase)
                                                     && a.Qty >= Convert.ToInt32(qtyResult.qty));
            if (!matchItem.Any())
            {
                WesModernDialog.ShowWesMessage(string.Format("掃描QRCode失敗，未匹配到數據！PN：{0};LotNO:{1};DC:{2};Qty:{3}",qtyResult.pn.ToString(), qtyResult.lot.ToString(), 
                    qtyResult.dc.ToString(), Convert.ToInt32(qtyResult.qty)));
                return;
            }
            _currScanModel = matchItem.First();
            _currScanModel.Qty = Convert.ToInt32(qtyResult.qty);
            printList.Add(_currScanModel);
            SaveScan(_currScanModel);
        }

        #endregion
    }
}