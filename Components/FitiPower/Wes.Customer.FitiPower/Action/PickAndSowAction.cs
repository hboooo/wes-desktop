using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.FitiPower.API;
using Wes.Customer.FitiPower.Model;
using Wes.Customer.FitiPower.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.FitiPower.Action
{
    public class PickAndSowAction : ScanActionBase<WesFlowID, PickAndSowAction>, IScanAction
    {
        private IEnumerable<PickAndSowModel> _unScanList = new List<PickAndSowModel>();//所有待作業明細
        private IEnumerable<PickAndSowModel> _scanningList = new List<PickAndSowModel>();//作業中
        private IEnumerable<PickAndSowModel> _scannedList = new List<PickAndSowModel>();//已完成
        private IEnumerable<CommonSowModel> _sowList = new List<CommonSowModel>();//播種列表
        private PickAndSowModel _currScanModel;//當前掃描數據

        [Ability(6442641212812828672, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {
            string result = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId((long)ScriptSid.CHECK_PXT)
                .AddQueryParameter("pxt", scanValue)
                .AddQueryParameter("customer", "C03707")
                .AddQueryParameter("user", WesDesktop.Instance.User.Code)
                .Execute()
                .To<string>();
            Vm.SelfInfo.consignee = result;
            Vm.SelfInfo.Pxt = scanValue;
            LoadSowInfo(scanValue);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_NEW_CARTON_NO);
        }

        [Ability(6415131763361718272, AbilityType.ScanAction)]
        private void LoadSowInfo(string scanValue)
        {
            DataSet result = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId((long)ScriptSid.GET_INFO_BY_PXT)
                .AddQueryParameter("pxt", scanValue)
                .Execute()
                .To<DataSet>();
            _unScanList = result.Tables[0].ToList<PickAndSowModel>();
            _sowList = result.Tables[1].ToList<CommonSowModel>();
            _scannedList = result.Tables[2].ToList<PickAndSowModel>();
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

        [Ability(6442659439404126208, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanNewCartonNo(string scanValue)
        {
            var isExistNewCartonNo = _unScanList.Any(a => a.NCartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
            if (!isExistNewCartonNo)
            {
                WesModernDialog.ShowWesMessage("掃描新箱號失敗，失敗原因:" + scanValue + " 無效！");
                return;
            }
            Vm.SelfInfo.selectCartonId = scanValue;
            BindCurrentCartonScanList(Vm.SelfInfo.Pxt, scanValue);
            _currScanModel = new PickAndSowModel
            {
                NCartonNo = scanValue
            };
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO);
        }

        private void BindCurrentCartonScanList(string operationNo, string nCartonNo)
        {
            var dsCurr = _unScanList.Where(a => a.OperationNo.Equals(operationNo, StringComparison.InvariantCultureIgnoreCase)
                                            && a.NCartonNo.Equals(nCartonNo, StringComparison.InvariantCultureIgnoreCase));

            _scanningList = dsCurr;
            Vm.SelfInfo.currentPid = nCartonNo;
            Vm.SelfInfo.currentTotalQty = _scanningList.Sum(a => a.Qty).ToString();
            Vm.SelfInfo.scanningList = _scanningList;
        }

        [Ability(6442662725486125056, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanCartonNo(string scanValue)
        {
            var isExistCartonNo = _scanningList.Any(a => a.CartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
            if (!isExistCartonNo)
            {
                WesModernDialog.ShowWesMessage("掃描原箱號失敗，失敗原因:" + scanValue + " 無效！");
                return;
            }
            _currScanModel.CartonNo = scanValue;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PART_NO);
        }


        [Ability(6442671894498906112, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPartNo(string scanValue)
        {
            var isExistPartNo = _scanningList.Any(a => a.NCartonNo.Equals(_currScanModel.NCartonNo, StringComparison.OrdinalIgnoreCase)
                                                         && a.CartonNo.Equals(_currScanModel.CartonNo, StringComparison.OrdinalIgnoreCase)
                                                         && a.PartNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
            if (!isExistPartNo)
            {
                WesModernDialog.ShowWesMessage("掃描料號失敗，失敗原因：" + scanValue + " 無效！");
                return;
            }
            _currScanModel = _scanningList.First(a => a.NCartonNo.Equals(_currScanModel.NCartonNo, StringComparison.OrdinalIgnoreCase)
                                                    && a.CartonNo.Equals(_currScanModel.CartonNo, StringComparison.OrdinalIgnoreCase)
                                                    && a.PartNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
        }

        [AbilityAble(6442674532921647104, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCarton, "consignee", false)]
        public virtual bool ScanFlowActionScanCheckNewCartonNo(string scanValue)
        {
            if (_currScanModel != null && !_currScanModel.NCartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase))
            {
                WesModernDialog.ShowWesMessage("校驗箱號失敗，失敗原因：" + scanValue + "無效,應為:" + _currScanModel.NCartonNo + "！");
                return false;
            }

            dynamic result = RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId((long)ScriptSid.SAVE_DATA)
                .AddJsonBody("pxt", Vm.SelfInfo.Pxt)
                .AddJsonBody("sxt", _currScanModel.LoadingNo)
                .AddJsonBody("pid", _currScanModel.CartonNo)
                .AddJsonBody("nCartonNo", scanValue)
                .AddJsonBody("pn", _currScanModel.PartNo)
                .AddJsonBody("cpn", _currScanModel.CustomerPN)
                .AddJsonBody("cpo", _currScanModel.OrderNo)
                .AddJsonBody("qty", _currScanModel.Qty)
                .AddJsonBody("lot", _currScanModel.LotNo)
                .AddJsonBody("dc", _currScanModel.DateCode)
                .AddJsonBody("user", WesDesktop.Instance.User.Code)
                .Execute()
                .To<object>();
            LoadSowInfo(Vm.SelfInfo.Pxt);
            BindCurrentCartonScanList(Vm.SelfInfo.Pxt, scanValue);
            Vm.SelfInfo.Pid = _currScanModel.CartonNo;
            Vm.SelfInfo.Qty = 1;
            Vm.SelfInfo.NeedAddPlus = false;
            IsSacnFinished(scanValue, _currScanModel.CartonNo);
            return true;
        }

        private void IsSacnFinished(string newCartonNo, string cartonNo)
        {
            var isExist = _scanningList.Any(a => a.NCartonNo.Equals(newCartonNo, StringComparison.OrdinalIgnoreCase));
            if (isExist)
            {
                var isExistCartonNo = _scanningList.Any(a =>
                    a.NCartonNo.Equals(newCartonNo, StringComparison.OrdinalIgnoreCase)
                    && a.CartonNo.Equals(cartonNo, StringComparison.OrdinalIgnoreCase));
                if (isExistCartonNo)
                {
                    Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PART_NO);
                }
                else
                {
                    UpdateStorage(cartonNo);
                    //已扫完整箱需添加Plu积分
                    Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO);
                }
            }
            else
            {
                UpdateStorage(cartonNo);
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_NEW_CARTON_NO);
            }
            if (!_unScanList.Any())
            {
                Vm.SelfInfo.scanningList = null;
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
            }
        }

        [AbilityAble(6443298628365721600, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCartonPlus, "consignee", false)]
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

        public virtual void DeleteData(long rowId)
        {
            var cartonNo = RestApi.NewInstance(Method.DELETE)
                 .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.DELETE_DATA, true)
                 .AddJsonBody("rowId", rowId)
                 .Execute()
                .To<string>();

            LoadSowInfo(Vm.SelfInfo.Pxt);
            Vm.SelfInfo.Pid = cartonNo;
            DeleteDataForKPI();
        }

        [AbilityAble(6442643936971329536, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCarton | KPIActionType.LSPickingSplitCartonPlus, "consignee", true)]
        protected virtual bool DeleteDataForKPI()
        {
            return true;
        }

        [Ability(6442643978541080576, AbilityType.ScanAction)]
        public virtual void SearchData()
        {
            Func<PickAndSowModel, bool> whereFunc1 = a => true;
            Func<PickAndSowModel, bool> whereFunc2 = a => true;
            var sowView = Vm as PickAndSowViewModel;
            if (!string.IsNullOrEmpty(sowView.SearchNPid))
                whereFunc1 = (a => a.NCartonNo.Equals(sowView.SearchNPid, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(sowView.SearchPN))
                whereFunc2 = (a => a.PartNo.Equals(sowView.SearchPN, StringComparison.InvariantCultureIgnoreCase));
            var lstData = _scannedList.Where(whereFunc1).Where(whereFunc2);
            Vm.SelfInfo.scannedList = lstData;
        }
    }
}
