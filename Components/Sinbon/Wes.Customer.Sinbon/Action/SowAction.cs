using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using Wes.Component.Widgets.APIAddr;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.Sinbon.Model;
using Wes.Customer.Sinbon.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Controls;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Sinbon.Action
{
    /// <summary>
    /// 分播业务逻辑
    /// </summary>
    public class SowAction : ScanActionBase<WesFlowID, SowAction>, IScanAction
    {
        #region 成員變量初始化
        private IEnumerable<SowModel> _unScanList = new List<SowModel>();//所有待作業明細
        private IEnumerable<SowModel> _scanningList = new List<SowModel>();//作業中
        private IEnumerable<SowModel> _scannedList = new List<SowModel>();//已完成
        private IEnumerable<CommonSowModel> _sowList = new List<CommonSowModel>();//播種列表
        private string _currentShipper = string.Empty; //當前掃描Shipper
        private bool isAutoCartonEnd;//判断是否需要自动Cartonend
        public string Command { get; set; } = WesScanCommand.NONE;
        private readonly string[] needPrintConsignee = new string[] { "C03829","C03830","C03831","C03832","C03833","C03834","C03835","C03836","C03837",
            "C03838","C03839","C03840","C03841","C03842","C03843","C03844","C03845","C03846","C03847","C03848","C03920","C03939","C03940","C03941" };
        #endregion

        #region 掃描PXT
        [Ability(6445576129255317504, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {
            if (!scanValue.IsPxt())
            {
                throw new WesException("Pxt 不合法");
            }
            string result = RestApi.NewInstance(Method.GET)
                .AddUri("/dispatching/check-pxt")
                .AddQueryParameter("operationNo", scanValue)
                .Execute()
                .To<string>();
            Vm.SelfInfo.Target = result;
            Vm.SelfInfo.Pxt = scanValue;
            Vm.SelfInfo.NCartonNo = string.Empty;
            LoadSowInfo(scanValue);
            if (!_unScanList.Any()) return;
            BindingAutoComplateData();
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PID_OR_PN_OR_CLCODE_OR_QRCODE);
        }

        [Ability(6447702104818589696, AbilityType.ScanAction)]
        private void LoadSowInfo(string pxt)
        {
            DataSet result = RestApi.NewInstance(Method.GET)
                .AddUri("/dispatching/load-detail")
                .AddQueryParameter("operationNo", pxt)
                .Execute()
                .To<DataSet>();
            _unScanList = result.Tables[0].ToList<SowModel>();
            _sowList = result.Tables[1].ToList<CommonSowModel>();
            _scannedList = result.Tables[2].ToList<SowModel>();
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

        //绑定PN/CLCode自动完成
        private void BindingAutoComplateData()
        {
            string uri = "/dispatching/part-end?operationNo=" + Vm.SelfInfo.Pxt;
            RestApi.NewInstance(Method.GET)
                .AddUri(uri)
                .ExecuteAsync((res, exp, restApi) =>
                {
                    if (restApi != null)
                    {
                        dynamic listCode = restApi.To<object>();
                        ObservableCollection<BarCodeScanModel> souce = new ObservableCollection<BarCodeScanModel>();
                        for (int i = 0; i < listCode.Count; i++)
                        {
                            souce.Add(new BarCodeScanModel()
                            {
                                Name = listCode[i].clcode.ToString(),
                                Type = null,
                                Code = null
                            });
                        }
                        for (int i = 0; i < listCode.Count; i++)
                        {
                            souce.Add(new BarCodeScanModel()
                            {
                                Name = listCode[i].pn.ToString(),
                                Type = null,
                                Code = null
                            });
                        }
                        Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            base.Vm.SelfInfo.useIntelligent = true;
                            base.Vm.SelfInfo.intelligentItems = souce;
                        }));
                    }
                });
        }
        #endregion

        #region 掃描Pid/PN/CLCode/QRCode
        [Ability(6447368330360655872, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPidOrPnOrClcodeOrQrcode(string scanValue)
        {
            if (scanValue.IsPackageID())
            {
                SaveScanByPid(scanValue);
                return;
            }
            if (scanValue.Length > 30)
            {
                //说明扫描的是QRCode
                ResolvQRCode(scanValue);
                return;
            }

            scanValue = scanValue.Trim();
            if (scanValue.IndexOf(" ", StringComparison.Ordinal) >= 0)
            {
                scanValue = scanValue.Replace(" ", "-");
            }
            var matchList = _unScanList.Where(a => a.CLCode.Equals(scanValue, StringComparison.CurrentCultureIgnoreCase)
                                                   || a.PartNo.Equals(scanValue, StringComparison.CurrentCultureIgnoreCase));
            if (!matchList.Any())
            {
                WesModernDialog.ShowWesMessage("掃描PN/CLCode失敗，失敗原因：" + scanValue + " 無效！");
                return;
            }

            _scanningList = matchList;
            _currentShipper = matchList.First().Shipper;
            Vm.SelfInfo.PN = scanValue;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY);
        }

        //支持直接扫箱号End
        private void SaveScanByPid(string scanValue)
        {
            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUri("/dispatching/save-pid")
                .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                .AddJsonBody("PackageId", scanValue)
                .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                .Execute()
                .To<object>();
            LoadSowInfo(Vm.SelfInfo.Pxt.ToString());
            Vm.SelfInfo.selectCartonId = result.nCartonNo.ToString();
            Vm.SelfInfo.NCartonNo = result.nCartonNo.ToString();
            Vm.SelfInfo.Pn = result.partNo.ToString();
            Vm.SelfInfo.OrderNo = result.orderNo.ToString();
            Vm.SelfInfo.LoadingNo = result.loadingNo.ToString();
            Vm.SelfInfo.Target = result.consignee.ToString();
            PrinNewCartonNo(result.nCartonNo.ToString(), result.loadingNo.ToString());
            LoadCurrentScanInfo(Vm.SelfInfo.Pxt.ToString(), result.nCartonNo.ToString());
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
        }

        #endregion

        #region 掃描QRCode解析
        [Ability(6450665345563762688, AbilityType.ScanAction)]
        private void ResolvQRCode(string qrCode)
        {
            dynamic result = RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "{si}")
                .AddScriptId((long)ScriptSid.QR_CODE_DISPATCH)
                .AddJsonBody("qrCode", qrCode)
                .Execute()
                .To<object>();
            if (result != null)
            {
                _currentShipper = result.supplier;
                IEnumerable<SowModel> matchItem = null;
                switch (_currentShipper)
                {
                    case "C03690":
                        matchItem = _unScanList.Where(a => a.CLCode.Equals(result.clCode.ToString(), StringComparison.CurrentCultureIgnoreCase)
                                                           && a.Qty >= Convert.ToInt32(result.qty));
                        break;
                    case "C02973":
                    case "C02975":
                        matchItem = _unScanList.Where(a => a.PartNo.Equals(result.pn.ToString(), StringComparison.CurrentCultureIgnoreCase)
                                                           && a.Qty >= Convert.ToInt32(result.qty));
                        break;
                }
                if (matchItem != null && !matchItem.Any())
                {
                    WesModernDialog.ShowWesMessage("QRCode解析失敗,請重新掃描！");
                    return;
                }
                var model = matchItem.First();
                Vm.SelfInfo.Qty = Convert.ToInt32(result.qty);
                _scanningList = matchItem;
                SaveScan(model);
            }
            else
            {
                WesModernDialog.ShowWesMessage("QRCode解析失敗,請重新掃描！");
            }
        }
        #endregion

        #region 掃描數量
        [Ability(6447728448004689920, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanQty(string scanValue)
        {
            int _qty = 0;
            if (!int.TryParse(scanValue, out _qty) || _qty <= 0)
            {
                WesModernDialog.ShowWesMessage("Qty格式不正確！");
                return;
            }

            var scanQty = Convert.ToInt32(scanValue);
            IEnumerable<SowModel> matchItem = null;
            switch (_currentShipper)
            {
                case "C03690":
                    matchItem = _scanningList.Where(a => a.CLCode.Equals(Vm.SelfInfo.PN.ToString(), StringComparison.CurrentCultureIgnoreCase)
                                                         && a.Qty >= (double)scanQty * a.MPQ);
                    break;
                case "C02973":
                case "C02975":
                    matchItem = _scanningList.Where(a => a.PartNo.Equals(Vm.SelfInfo.PN.ToString(), StringComparison.CurrentCultureIgnoreCase)
                                                         && a.Qty >= scanQty);
                    break;
            }
            if (matchItem != null && !matchItem.Any())
            {
                WesModernDialog.ShowWesMessage(string.Format("掃描Qty失敗，PN:{0},Qty:{1}無效！", Vm.SelfInfo.PN.ToString(), scanQty));
                return;
            }

            SowModel model = matchItem.First();
            if (_currentShipper == "C03690")
            {
                scanQty = scanQty * model.MPQ;
            }
            Vm.SelfInfo.Qty = scanQty;
            SaveScan(model);
        }

        [Ability(6447732581789081600, AbilityType.ScanAction)]
        private void SaveScan(SowModel model)
        {
            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUri("/dispatching/save")
                .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                .AddJsonBody("LoadingNo", model.LoadingNo)
                .AddJsonBody("PartNo", model.PartNo)
                .AddJsonBody("CLCode", model.CLCode)
                .AddJsonBody("DC", model.DateCode)
                .AddJsonBody("OrderNo", model.OrderNo)
                .AddJsonBody("Qty", Convert.ToInt32(Vm.SelfInfo.Qty))
                .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                .Execute()
                .To<object>();
            LoadSowInfo(Vm.SelfInfo.Pxt.ToString());
            string nCartonNo = result.nCartonNo.ToString();
            Vm.SelfInfo.selectCartonId = nCartonNo;
            Vm.SelfInfo.NCartonNo = nCartonNo;
            Vm.SelfInfo.Pn = model.PartNo;
            Vm.SelfInfo.OrderNo = model.OrderNo;
            Vm.SelfInfo.LoadingNo = model.LoadingNo;
            Vm.SelfInfo.Target = result.consignee.ToString();
            PrinNewCartonNo(nCartonNo, model.LoadingNo);
            LoadCurrentScanInfo(Vm.SelfInfo.Pxt.ToString(), nCartonNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
        }
        #endregion

        #region 掃描新箱號Check
        [AbilityAble(6445531246654132224, AbilityType.ScanAction, true, KPIActionType.LSPacking, "Target", false)]
        public virtual bool ScanFlowActionScanCheckNewCartonNo(string scanValue)
        {
            if (ExecuteCartonEnd(scanValue))
            {
                return false;
            }
            string currentNCartonNo = Vm.SelfInfo.NCartonNo.ToString();
            if (!string.Equals(currentNCartonNo, scanValue, StringComparison.CurrentCultureIgnoreCase))
            {
                WesModernDialog.ShowWesMessage("校驗箱號失敗，失敗原因：" + scanValue + "無效,應為:" + Vm.SelfInfo.NCartonNo.ToString() + "");
                return false;
            }
            Vm.SelfInfo.Pid = scanValue;
            Vm.SelfInfo.Qty = 1;
            Vm.SelfInfo.selectCartonId = string.Empty;
            IsSacnFinished(currentNCartonNo);
            return true;
        }

        private void IsSacnFinished(string nCartonNo)
        {
            if (!_unScanList.Any())
            {
                CartonEnd(nCartonNo);//最后一箱自动Cartonend

                Vm.SelfInfo.scanningList = null;
                Vm.SelfInfo.currentPid = string.Empty;
                Vm.SelfInfo.currentTotalQty = string.Empty;
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
            }
            else
            {
                var isExist = _unScanList.Any(a => a.NCartonNo.Equals(nCartonNo, StringComparison.InvariantCultureIgnoreCase));
                if (isAutoCartonEnd && !isExist)
                {
                    //系统自动并箱
                    CartonEnd(nCartonNo);
                }
                if (!isAutoCartonEnd)
                {
                    //手动并箱
                    if (Vm.SelfInfo.Target.ToString() == "C03820" || Vm.SelfInfo.Target.ToString() == "C03821")
                    {
                        //C03820和C03821 需要根据PN+OrderNo并箱
                        isExist = _unScanList.Any(a => a.PartNo.Equals(Vm.SelfInfo.Pn.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && a.OrderNo.Equals(Vm.SelfInfo.OrderNo.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                                       && a.LoadingNo.Equals(Vm.SelfInfo.LoadingNo.ToString(), StringComparison.CurrentCultureIgnoreCase));
                    }
                    else
                    {
                        //其余客户需要根据PN并箱 即一箱一料
                        isExist = _unScanList.Any(a => a.PartNo.Equals(Vm.SelfInfo.Pn.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                                       && a.LoadingNo.Equals(Vm.SelfInfo.LoadingNo.ToString(), StringComparison.CurrentCultureIgnoreCase));
                    }

                    if (!isExist)
                    {
                        CartonEnd(nCartonNo);
                    }
                }
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PID_OR_PN_OR_CLCODE_OR_QRCODE);
            }
        }
        #endregion

        #region CartonEnd并箱
        [Ability(6447740231490736128, AbilityType.ScanAction)]
        public virtual void CartonEnd(string scanVal)
        {
            if (string.IsNullOrEmpty(Vm.SelfInfo.NCartonNo.ToString()))
            {
                WesModernDialog.ShowWesMessage("請掃描新箱號!");
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
                Command = WesScanCommand.CARTON_END;
                return;
            }
            DataTable result = RestApi.NewInstance(Method.POST)
                   .AddUri("/dispatching/cartonend")
                   .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                   .AddJsonBody("NewCartonNo", Vm.SelfInfo.NCartonNo.ToString())
                   .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                   .Execute()
                   .To<DataTable>();
            foreach (DataRow row in result.Rows)
            {
                string consignee = row["consignee"].ToString();
                string customer = row["customer"].ToString();
                string pn = row["partNo"].ToString();
                int qty = Convert.ToInt32(row["qty"]);
                if (needPrintConsignee.Contains(consignee))
                {
                    PrintToCustomers(customer, qty, pn);
                }
            }
            AddKpiPlus();
            LoadSowInfo(Vm.SelfInfo.Pxt.ToString());
        }

        //忘记Cartonend时候 可以补打：pxt-》Cartonend--》新箱号
        private bool ExecuteCartonEnd(string scanValue)
        {
            if (Command == WesScanCommand.CARTON_END)
            {
                Vm.SelfInfo.NCartonNo = scanValue;
                DataTable result = RestApi.NewInstance(Method.POST)
                    .AddUri("/dispatching/cartonend")
                    .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                    .AddJsonBody("NewCartonNo", Vm.SelfInfo.NCartonNo.ToString())
                    .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                    .Execute()
                    .To<DataTable>();
                foreach (DataRow row in result.Rows)
                {
                    string consignee = row["consignee"].ToString();
                    string customer = row["customer"].ToString();
                    string pn = row["partNo"].ToString();
                    int qty = Convert.ToInt32(row["qty"]);

                    if (needPrintConsignee.Contains(consignee))
                    {
                        PrintToCustomers(customer, qty, pn);
                    }
                }
                AddKpiPlus();
                LoadSowInfo(Vm.SelfInfo.Pxt.ToString());
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
                Command = WesScanCommand.NONE;
                return true;
            }
            return false;
        }

        [AbilityAble(6450657342965944320, AbilityType.ScanAction, true, KPIActionType.LSPackingPlus, "Target", false)]
        protected virtual bool AddKpiPlus()
        {
            Vm.SelfInfo.Pid = Vm.SelfInfo.NCartonNo.ToString();
            Vm.SelfInfo.Qty = 1;
            return true;
        }
        #endregion

        #region 加載當前掃描新箱號信息
        [Ability(6447735564249604096, AbilityType.ScanAction)]
        private void LoadCurrentScanInfo(string pxt, string nCartonNo)
        {
            DataTable result = RestApi.NewInstance(Method.GET)
                .AddUri("/dispatching/load-current")
                .AddQueryParameter("operationNo", pxt)
                .AddQueryParameter("newCartonNo", nCartonNo)
                .Execute()
                .To<DataTable>();
            IEnumerable<SowModel> currentScanningList = result.ToList<SowModel>();
            isAutoCartonEnd = currentScanningList.Any(a => a.LoadingNo.Equals(nCartonNo.Substring(0, 13), StringComparison.CurrentCultureIgnoreCase)
                                                   && a.IsAutoCombineCarton.Equals("Y", StringComparison.CurrentCultureIgnoreCase));
            Vm.SelfInfo.scanningList = currentScanningList;
            Vm.SelfInfo.currentPid = nCartonNo;
            Vm.SelfInfo.currentTotalQty = currentScanningList.Sum(a => a.Qty).ToString();
        }
        #endregion

        #region 打印新箱號
        [Ability(6447735472893464576, AbilityType.ScanAction)]
        private void PrinNewCartonNo(string nCartonNo, string loadingNo)
        {
            if (_scannedList.Count(a => a.NCartonNo.Equals(nCartonNo, StringComparison.CurrentCultureIgnoreCase)) == 1)
            {
                //如果是第一次則打印新箱號
                var pm = new PrintTemplateModel();
                pm.PrintDataValues = new Dictionary<string, object>
                {
                    { "NCartonNo", nCartonNo },
                    { "LoadingNo", loadingNo },
                    { "RowID", Vm.SelfInfo.selectIndex.ToString() }
                };
                pm.TemplateFileName = "Avnet_NewCartonLabel.btw";
                LabelPrintBase lpb = new LabelPrintBase(pm, false);
                var res = lpb.Print();
            }
        }
        #endregion

        #region 针对某些报关客户，需多出一张标签
        private void PrintToCustomers(string customer, int qty, string pn)
        {
            var pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>
                {
                    { "PartNO", pn },
                    { "Qty", qty },
                    { "Brand", customer }
                };
            pm.TemplateFileName = "SB_NcartonLabel.btw";
            LabelPrintBase lpb = new LabelPrintBase(pm, false);
            var res = lpb.Print();
        }
        #endregion

        #region 刪除一條分播數據
        [Ability(6445575938762608640, AbilityType.ScanAction)]
        public virtual void DeleteData(long rowId)
        {
            string cartonNo = RestApi.NewInstance(Method.DELETE)
                .AddUri("/dispatching")
                .AddQueryParameter("rowId", rowId)
                .Execute()
                .To<string>();
            LoadSowInfo(Vm.SelfInfo.Pxt);
            Vm.SelfInfo.Pid = cartonNo;
            DeleteDataForKPI();
        }

        [AbilityAble(6451023974145990656, AbilityType.ScanAction, true, KPIActionType.LSPackingPlus, "Target", true)]
        protected virtual bool DeleteDataForKPI()
        {
            return true;
        }
        #endregion

        #region 已完成查詢
        [Ability(6445575960963063808, AbilityType.ScanAction)]
        public virtual void SearchData()
        {
            Func<SowModel, bool> whereFunc1 = a => true;
            Func<SowModel, bool> whereFunc2 = a => true;
            var sowView = Vm as SowViewModel;
            if (!string.IsNullOrEmpty(sowView.SearchNPid))
                whereFunc1 = (a => a.NCartonNo.Equals(sowView.SearchNPid, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(sowView.SearchPN))
                whereFunc2 = (a => a.CLCode.Equals(sowView.SearchPN, StringComparison.InvariantCultureIgnoreCase)
                                   || a.PartNo.Equals(sowView.SearchPN, StringComparison.InvariantCultureIgnoreCase));
            var lstData = _scannedList.Where(whereFunc1).Where(whereFunc2);
            Vm.SelfInfo.scannedList = lstData;
        }
        #endregion
    }
}
