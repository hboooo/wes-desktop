using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Wes.Component.Widgets.APIAddr;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.Mcc.Model;
using Wes.Customer.Mcc.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action
{
    public class PickDispatchingAction : ScanActionBase<WesFlowID, PickDispatchingAction>, IScanAction
    {
        #region 成员变量
        private IEnumerable<PickDispatchingModel> _unScanList = new List<PickDispatchingModel>();//所有待作業明細
        private IEnumerable<PickDispatchingModel> _scanningList = new List<PickDispatchingModel>();//作業中
        private IEnumerable<PickDispatchingModel> _scannedList = new List<PickDispatchingModel>();//已完成
        private IEnumerable<CommonSowModel> _sowList = new List<CommonSowModel>();//播種列表
        private PickDispatchingModel _currScanModel;//當前掃描數據
        public string Command { get; set; } = WesScanCommand.NONE;
        #endregion

        #region 扫描PXT
        [Ability(6493009038295040, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {
            if (scanValue.ToUpper().Equals("CARTONEND"))
            {
                if (!string.IsNullOrEmpty(Vm.SelfInfo.Pxt.ToString()))
                {
                    WesModernDialog.ShowWesMessage("請掃描新箱號!");
                    this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
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
            Vm.SelfInfo.Target = result;
            Vm.SelfInfo.Pxt = scanValue;
            Vm.SelfInfo.NCartonNo = string.Empty;
            LoadSowInfo(scanValue);

            if (!_unScanList.Any()) return;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO);
        }

        [Ability(6493009130553344, AbilityType.ScanAction)]
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
        [Ability(6493009222844416, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanCartonNo(string scanValue)
        {
            var isExistCartonNo = _unScanList.Any(a => a.CartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
            if (!isExistCartonNo)
            {
                WesModernDialog.ShowWesMessage("掃描原箱號失敗，失敗原因:" + scanValue + " 無效！");
                return;
            }
            _currScanModel = new PickDispatchingModel
            {
                CartonNo = scanValue
            };
            Vm.SelfInfo.currentPid = scanValue;
            BindCurrentCartonScanList(scanValue);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PART_NO);
        }
        private void BindCurrentCartonScanList(string cartonNo)
        {
            var dsCurr = _unScanList.Where(a => a.CartonNo.Equals(cartonNo, StringComparison.InvariantCultureIgnoreCase));
            _scanningList = dsCurr;
            Vm.SelfInfo.currentTotalQty = _scanningList.Sum(a => a.Qty).ToString();
            Vm.SelfInfo.scanningList = _scanningList;
        }
        #endregion

        #region 扫描料号
        [Ability(6493009256402944, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPartNo(string scanValue)
        {
            string partNo = scanValue;
            if (scanValue.StartsWith("1P", StringComparison.InvariantCultureIgnoreCase))
            {
                partNo = scanValue.Right(partNo.Length - 2);
            }

            var isExistPartNo = _unScanList.Any(a => a.CartonNo.Equals(_currScanModel.CartonNo, StringComparison.OrdinalIgnoreCase)
                                                       && a.PartNo.Equals(partNo, StringComparison.OrdinalIgnoreCase));
            if (!isExistPartNo)
            {
                WesModernDialog.ShowWesMessage("掃描料號失敗，失敗原因：" + scanValue + " 無效！");
                return;
            }

            _currScanModel = _unScanList.First(a => a.CartonNo.Equals(_currScanModel.CartonNo, StringComparison.OrdinalIgnoreCase)
                                                  && a.PartNo.Equals(partNo, StringComparison.OrdinalIgnoreCase));
            Vm.SelfInfo.scanningListSelected = _currScanModel;
            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUri("/dispatching/saveByPn")
                .AddJsonBody("pxt", Vm.SelfInfo.Pxt)
                .AddJsonBody("sxt", _currScanModel.LoadingNo)
                .AddJsonBody("pid", _currScanModel.CartonNo)
                .AddJsonBody("pn", _currScanModel.PartNo)
                .AddJsonBody("cpn", _currScanModel.CustomerPN)
                .AddJsonBody("consignee", Vm.SelfInfo.Target)
                .AddJsonBody("user", WesDesktop.Instance.User.Code)
                .Execute()
                .To<object>();
            string nCartonNo = result.nCartonNo.ToString();
            LoadSowInfo(Vm.SelfInfo.Pxt);
            Vm.SelfInfo.selectCartonId = nCartonNo;
            Vm.SelfInfo.LoadingNo = _currScanModel.LoadingNo;
            Vm.SelfInfo.Pn = _currScanModel.PartNo;
            Vm.SelfInfo.CPN = _currScanModel.CustomerPN;
            PrinNewCartonNo(nCartonNo, _currScanModel.LoadingNo);
            _currScanModel.NCartonNo = nCartonNo;
            BindCurrentCartonScanList(_currScanModel.CartonNo);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
        }

        #endregion

        #region Check 新箱号
        [AbilityAble(6493009281548288, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanCheckNewCartonNo(string scanValue)
        {
            if (ExecuteCartonEnd(scanValue))
            {
                return;
            }
            if (_currScanModel != null && !_currScanModel.NCartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase))
            {
                WesModernDialog.ShowWesMessage("校驗箱號失敗，失敗原因：" + scanValue + "無效,應為:" + _currScanModel.NCartonNo + "！");
                return;
            }

            Vm.SelfInfo.NCartonNo = scanValue;
            Vm.SelfInfo.Pid = _currScanModel.CartonNo;
            Vm.SelfInfo.Qty = 1;
            IsSacnFinished(_currScanModel.CartonNo);
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

        #region CartonEnd并箱
        [Ability(6507075341393920, AbilityType.ScanAction)]
        public virtual void CartonEnd(string scanVal)
        {
            if (string.IsNullOrEmpty(Vm.SelfInfo.NCartonNo.ToString()))
            {
                WesModernDialog.ShowWesMessage("請掃描新箱號!");
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
                Command = WesScanCommand.CARTON_END;
                return;
            }

            RestApi.NewInstance(Method.POST)
                .AddUri("/dispatching/cartonend")
                .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                .AddJsonBody("NewCartonNo", Vm.SelfInfo.NCartonNo.ToString())
                .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                .Execute();
            AddKpiPlus();
            Vm.SelfInfo.selectCartonId = string.Empty;
            LoadSowInfo(Vm.SelfInfo.Pxt.ToString());
        }

        //忘记Cartonend时候 可以补打：pxt-》Cartonend--》新箱号
        private bool ExecuteCartonEnd(string scanValue)
        {
            if (Command == WesScanCommand.CARTON_END)
            {
                Vm.SelfInfo.NCartonNo = scanValue;
                RestApi.NewInstance(Method.POST)
                    .AddUri("/dispatching/cartonend")
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

        [AbilityAble(6507072338284544, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCartonPlus, "Target", false)]
        protected virtual bool AddKpiPlus()
        {
            Vm.SelfInfo.Pid = Vm.SelfInfo.NCartonNo.ToString();
            Vm.SelfInfo.Qty = 1;
            return true;
        }
        #endregion

        #region 判断是否完成
        private void IsSacnFinished(string cartonNo)
        {
            var isExist = _scanningList.Any(a => a.CartonNo.Equals(cartonNo, StringComparison.OrdinalIgnoreCase));
            if (isExist)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PART_NO);
            }
            else
            {
                UpdateStorage(cartonNo);
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO);
            }
            if (!_unScanList.Any())
            {
                CartonEnd(Vm.SelfInfo.NCartonNo);//最后一箱自动Cartonend
                Vm.SelfInfo.scanningList = null;
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
            }
            else
            {
                if (Vm.SelfInfo.Target.ToString() != "C04325")
                {
                    //按PN+CPN分箱
                    isExist = _unScanList.Any(a => a.PartNo.Equals(Vm.SelfInfo.Pn.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                                   && a.CustomerPN.Equals(Vm.SelfInfo.CPN.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                                   && a.LoadingNo.Equals(Vm.SelfInfo.LoadingNo.ToString(), StringComparison.CurrentCultureIgnoreCase));
                    if (!isExist)
                    {
                        CartonEnd(Vm.SelfInfo.NCartonNo);
                    }
                }
            }
        }
        #endregion

        #region 更新储位
        [AbilityAble(6493009398992896, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCarton, "Target", false)]
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
        #endregion

        #region 删除
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

        [AbilityAble(6493009466105856, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCarton | KPIActionType.LSPickingSplitCartonPlus, "Target", true)]
        protected virtual bool DeleteDataForKPI()
        {
            return true;
        }
        #endregion

        #region 已完成数据查询
        [Ability(6493009508052992, AbilityType.ScanAction)]
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
        #endregion
    }
}
