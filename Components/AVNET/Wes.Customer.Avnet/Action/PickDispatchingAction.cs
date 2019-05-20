using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Wes.Component.Widgets.APIAddr;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.Avnet.Model;
using Wes.Customer.Avnet.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Avnet.Action
{
    public class PickDispatchingAction : ScanActionBase<WesFlowID, PickDispatchingAction>, IScanAction
    {
        private IEnumerable<PickDispatchingModel> _unScanList = new List<PickDispatchingModel>();//所有待作業明細
        private IEnumerable<PickDispatchingModel> _scanningList = new List<PickDispatchingModel>();//作業中
        private IEnumerable<PickDispatchingModel> _scannedList = new List<PickDispatchingModel>();//已完成
        private IEnumerable<CommonSowModel> _sowList = new List<CommonSowModel>();//播種列表
        private PickDispatchingModel _currScanModel;//當前掃描數據

        #region 扫描PXT
        [Ability(6471610973743484928, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {
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
            LoadSowInfo(scanValue);
            if (!_unScanList.Any()) return;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_NEW_CARTON_NO);
            SetActionValid();
        }

        [Ability(6471611746606915584, AbilityType.ScanAction)]
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

        #region 扫描新箱号
        [Ability(6471613155192606720, AbilityType.ScanAction)]
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
            _currScanModel = new PickDispatchingModel
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
        #endregion

        #region 扫描原箱号
        [Ability(6471614953219432448, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanCartonNo(string scanValue)
        {
            var isExistCartonNo = _scanningList.Any(a => a.CartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase));
            if (!isExistCartonNo)
            {
                WesModernDialog.ShowWesMessage("掃描原箱號失敗，失敗原因:" + scanValue + " 無效！");
                return;
            }
            _currScanModel.CartonNo = scanValue;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }
        #endregion

        #region 扫描MPN
        [Ability(6471616650838806528, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanMpn(string scanValue)
        {
            string mpn = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, 6402348645173174282)
                .AddBranch("MPN")
                .AddQueryParameter("mpn", scanValue)
                .Execute()
                .To<string>();
            var scanItem = _scanningList.FirstOrDefault(a => a.FactoryPN.Equals(mpn, StringComparison.OrdinalIgnoreCase)
                            && a.CartonNo.Equals(_currScanModel.CartonNo, StringComparison.OrdinalIgnoreCase));
            if (scanItem == null)
            {
                WesModernDialog.ShowWesMessage("掃描MPN失敗，失敗原因:" + scanValue + " 無效！");
                return;
            }
            _currScanModel.FactoryPN = mpn;
            _currScanModel.Shipper = scanItem.Shipper;
            Vm.SelfInfo.mpn = mpn;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QRCODE);
        }
        #endregion

        #region 扫描QRCode
        [Ability(6471619817706102784, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanQrcode(string scanValue)
        {
            dynamic qrCode = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, 6402348645168975872)
                .AddJsonBody("qrCode", scanValue)
                .AddJsonBody("mpn", Vm.SelfInfo.mpn)
                .Execute()
                .To<object>();
            int scanningQty = Convert.ToInt32(qrCode.qty.ToString());
            string dc = qrCode.dc.ToString();
            string lot = qrCode.lot.ToString();
            var scanItem = _scanningList.Where(a => a.FactoryPN.Equals(_currScanModel.FactoryPN, StringComparison.OrdinalIgnoreCase)
                                            && a.Qty >= scanningQty
                                            && a.DateCode.Equals(dc, StringComparison.OrdinalIgnoreCase)
                                            && a.CartonNo.Equals(_currScanModel.CartonNo, StringComparison.OrdinalIgnoreCase)
                                            && a.LotNo.Equals(lot, StringComparison.OrdinalIgnoreCase));
            if (!scanItem.Any())
            {
                WesModernDialog.ShowWesMessage(string.Format("未匹配數據，請確認!LotNo:{0},DateCode:{1},Qty:{2}", lot, dc, scanningQty));
                return;
            }
            _currScanModel = scanItem.First();
            _currScanModel.Qty = scanningQty;
            Vm.SelfInfo.scanningListSelected = _currScanModel;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CHECK_NEW_CARTON_NO);
        }
        #endregion

        #region Check 新箱号
        [Ability(6471620061349023744, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanCheckNewCartonNo(string scanValue)
        {
            if (_currScanModel != null && !_currScanModel.NCartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase))
            {
                WesModernDialog.ShowWesMessage("校驗箱號失敗，失敗原因：" + scanValue + "無效,應為:" + _currScanModel.NCartonNo + "！");
                return;
            }

            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUri("/dispatching/save")
                .AddJsonBody("pxt", Vm.SelfInfo.Pxt)
                .AddJsonBody("sxt", _currScanModel.LoadingNo)
                .AddJsonBody("pid", _currScanModel.CartonNo)
                .AddJsonBody("nCartonNo", scanValue)
                .AddJsonBody("supplier", _currScanModel.Shipper)
                .AddJsonBody("pn", _currScanModel.PartNo)
                .AddJsonBody("cpn", _currScanModel.CustomerPN)
                .AddJsonBody("cpo", _currScanModel.OrderNo)
                .AddJsonBody("spn", _currScanModel.FactoryPN)
                .AddJsonBody("qty", _currScanModel.Qty)
                .AddJsonBody("dc", _currScanModel.DateCode)
                .AddJsonBody("lot", _currScanModel.LotNo)
                .AddJsonBody("batchNo", _currScanModel.BatchNo)
                .AddJsonBody("minQty", _currScanModel.MPQ)
                .AddJsonBody("user", WesDesktop.Instance.User.Code)
                .Execute()
                .To<object>();
            LoadSowInfo(Vm.SelfInfo.Pxt);
            BindCurrentCartonScanList(Vm.SelfInfo.Pxt, scanValue);
            Vm.SelfInfo.Pid = _currScanModel.CartonNo;
            Vm.SelfInfo.Qty = 1;
            Vm.SelfInfo.NeedAddPlus = false;
            if (!_unScanList.Any(a => a.NCartonNo.Equals(scanValue, StringComparison.InvariantCultureIgnoreCase)))
            {
                //已满箱需添加Plu积分
                Vm.SelfInfo.NeedAddPlus = true;
            }
            IsSacnFinished(scanValue, _currScanModel.CartonNo);
            SetActionValid();
        }
        #endregion

        #region 判断是否完成
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
                    Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
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
        #endregion

        #region 更新储位
        [Ability(6471621246093107200, AbilityType.ScanAction)]
        protected virtual void UpdateStorage(string cartonNo)
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
        }

        #endregion

        #region 已完成数据查询
        [Ability(6471582069179813888, AbilityType.ScanAction)]
        public virtual void SearchData()
        {
            Func<PickDispatchingModel, bool> whereFunc1 = a => true;
            Func<PickDispatchingModel, bool> whereFunc2 = a => true;
            var sowView = Vm as PickDispatchingViewModel;
            if (!string.IsNullOrEmpty(sowView.SearchNPid))
                whereFunc1 = (a => a.NCartonNo.Equals(sowView.SearchNPid, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(sowView.SearchPN))
                whereFunc2 = (a => a.FactoryPN.Equals(sowView.SearchPN, StringComparison.InvariantCultureIgnoreCase)
                                   || a.PartNo.Equals(sowView.SearchPN, StringComparison.InvariantCultureIgnoreCase));
            var lstData = _scannedList.Where(whereFunc1).Where(whereFunc2);
            Vm.SelfInfo.scannedList = lstData;
        }
        #endregion
    }
}
