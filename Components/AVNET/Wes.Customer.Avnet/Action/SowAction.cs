using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.Avnet.Model;
using Wes.Customer.Avnet.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Avnet.Action
{
    /// <summary>
    /// 分播业务逻辑
    /// </summary>
    public class SowAction : ScanActionBase<WesFlowID, SowAction>, IScanAction
    {
        private const long CheckPxtScriptId = 6414677953342345216;
        private const long GetSowInfoScriptId = 6414779746944557056;
        private const long SaveDataScriptId = 6415052745165770752;
        private const long QrCodeScriptId = 6402348645168975872;
        private const long DecodeMPNScriptId = 6402348645173174282;
        private const long DeleteDataScriptId = 6417188046688489472;
        private const long GetLoadingNoScriptId = 6417253839241482240;

        private IEnumerable<SowModel> _unScanList = new List<SowModel>();//所有待作業明細
        private IEnumerable<SowModel> _scanningList = new List<SowModel>();//作業中
        private IEnumerable<SowModel> _scannedList = new List<SowModel>();//已完成
        private IEnumerable<CommonSowModel> _sowList = new List<CommonSowModel>();//播種列表
        private SowModel _currScanModel;//當前掃描數據
        private int scanningQty;


        [Ability(6415129929175797760, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {
            string result = RestApi.NewInstance(Method.GET)
                  .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                  .AddScriptId(CheckPxtScriptId)
                  .AddQueryParameter("pxt", scanValue)
                  .AddQueryParameter("user", WesDesktop.Instance.User.Code)
                  .Execute()
                  .To<string>();
            Vm.SelfInfo.Target = result;
            Vm.SelfInfo.Pxt = scanValue;
            LoadSowInfo(scanValue);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
            SetActionValid();
        }

        [Ability(6415131482313986048, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanMpn(string scanValue)
        {
            string mpn = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "{si}")
                .AddScriptId(DecodeMPNScriptId)
                .AddBranch("MPN")
                .AddQueryParameter("mpn", scanValue)
                .Execute()
                .To<string>();
            var scanItem = _unScanList.FirstOrDefault(a => a.FactoryPN.Equals(mpn, StringComparison.InvariantCultureIgnoreCase));
            if (scanItem == null)
            {
                WesModernDialog.ShowWesMessage("未匹配到數據，請聯繫IT！Mpn：" + mpn);
                return;
            }
            _currScanModel = new SowModel
            {
                FactoryPN = mpn,
                Shipper = scanItem.Shipper
            };
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QRCODE);
        }

        [Ability(6415139049094254592, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanQrcode(string scanValue)
        {
            dynamic qrCode = RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "{si}")
                .AddScriptId(QrCodeScriptId)
                .AddJsonBody("qrCode", scanValue)
                .AddJsonBody("mpn", _currScanModel.FactoryPN)
                .Execute()
                .To<object>();
            scanningQty = Convert.ToInt32(qrCode.qty.ToString());
            string dc = qrCode.dc.ToString();
            string lot = qrCode.lot.ToString();
            var scanItem = _unScanList.Where(a =>
                a.FactoryPN.Equals(_currScanModel.FactoryPN, StringComparison.OrdinalIgnoreCase)
                && a.Qty == scanningQty
                && a.DateCode.Equals(dc, StringComparison.OrdinalIgnoreCase)
                && a.LotNos.Equals(lot, StringComparison.InvariantCultureIgnoreCase));
            if (!scanItem.Any())
            {
                scanItem = _unScanList.Where(a =>
                    a.FactoryPN.Equals(_currScanModel.FactoryPN, StringComparison.OrdinalIgnoreCase)
                    && a.Qty > scanningQty
                    && a.DateCode.Equals(dc, StringComparison.OrdinalIgnoreCase)
                    && a.LotNos.Equals(lot, StringComparison.InvariantCultureIgnoreCase));
                if (!scanItem.Any())
                {
                    WesModernDialog.ShowWesMessage(string.Format("未匹配數據，請確認!LotNo:{0},DateCode:{1},Qty:{2}",
                        lot, dc, scanningQty));
                    return;
                }
            }
            _scanningList = scanItem;
            _currScanModel = scanItem.First();
            PrintNCartonNo();
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_CARTON_NO);
        }

        private void PrintNCartonNo()
        {
            Vm.SelfInfo.scanningList = _scanningList.Where(a => a.NCartonNo.Equals(_currScanModel.NCartonNo, StringComparison.CurrentCultureIgnoreCase));
            Vm.SelfInfo.currentPid = _currScanModel.NCartonNo;
            Vm.SelfInfo.currentTotalQty = _scanningList.Where(a => a.NCartonNo.Equals(_currScanModel.NCartonNo, StringComparison.CurrentCultureIgnoreCase))
                .Sum(a => a.Qty).ToString();
            Vm.SelfInfo.selectCartonId = _currScanModel.NCartonNo;
            if (_scannedList.Count(a => a.NCartonNo.Equals(_currScanModel.NCartonNo, StringComparison.CurrentCultureIgnoreCase)) == 0)
            {
                //如果是第一次則打印新箱號
                string loadingNo = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                    .AddScriptId(GetLoadingNoScriptId)
                    .AddQueryParameter("pxt", _currScanModel.OperationNo)
                    .AddQueryParameter("npid", _currScanModel.NCartonNo)
                    .Execute()
                    .To<string>();
                var pm = new PrintTemplateModel();
                pm.PrintDataValues = new Dictionary<string, object>
                {
                    { "NCartonNo", _currScanModel.NCartonNo },
                    { "LoadingNo", loadingNo },
                    { "RowID", Vm.SelfInfo.selectIndex.ToString() }
                };
                pm.TemplateFileName = "Avnet_NewCartonLabel.btw";
                LabelPrintBase lpb = new LabelPrintBase(pm, false);
                var res = lpb.Print();
            }
        }

        [Ability(6415139884503146496, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanCartonNo(string scanValue)
        {
            if (!_currScanModel.NCartonNo.Equals(scanValue, StringComparison.OrdinalIgnoreCase))
            {
                WesModernDialog.ShowWesMessage("校驗箱號失敗，失敗原因：" + scanValue + "無效,應為:" + _currScanModel.NCartonNo + "！");
                return;
            }

            dynamic result = RestApi.NewInstance(Method.POST)
                    .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                    .AddScriptId(SaveDataScriptId)
                    .AddJsonBody("pxt", _currScanModel.OperationNo)
                    .AddJsonBody("newCarton", scanValue)
                    .AddJsonBody("pid", _currScanModel.CartonNo)
                    .AddJsonBody("cpn", _currScanModel.CustomerPN)
                    .AddJsonBody("dc", _currScanModel.DateCode)
                    .AddJsonBody("mpn", _currScanModel.FactoryPN)
                    .AddJsonBody("qty", scanningQty)
                    .AddJsonBody("lot", _currScanModel.LotNos)
                    .AddJsonBody("user", WesDesktop.Instance.User.Code)
                    .Execute()
                    .To<object>();

            Vm.SelfInfo.selectCartonId = string.Empty;
            LoadSowInfo(Vm.SelfInfo.Pxt);
            Vm.SelfInfo.Pid = scanValue;
            Vm.SelfInfo.Qty = 1;
            Vm.SelfInfo.NeedAddPlus = false;
            if (!_unScanList.Any(a => a.NCartonNo.Equals(scanValue, StringComparison.InvariantCultureIgnoreCase)))
            {
                //已满箱需添加Plu积分
                Vm.SelfInfo.NeedAddPlus = true;
            }
            IsSacnFinished();
            SetActionValid();
        }
        private void IsSacnFinished()
        {
            Vm.SelfInfo.currentPid = string.Empty;
            Vm.SelfInfo.currentTotalQty = string.Empty;
            Vm.SelfInfo.scanningList = null;
            if (!_unScanList.Any())
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
                _currScanModel = null;
            }
            else
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
            }
        }

        [Ability(6415131763361718272, AbilityType.ScanAction)]
        private void LoadSowInfo(string scanValue)
        {
            DataSet result = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(GetSowInfoScriptId)
                .AddQueryParameter("pxt", scanValue)
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

        [Ability(6417188076006678528, AbilityType.ScanAction)]
        public virtual void DeleteData(long rowId)
        {
            RestApi.NewInstance(Method.DELETE)
                .AddUriParam(RestUrlType.WmsServer, DeleteDataScriptId, true)
                .AddJsonBody("rowId", rowId)
                .Execute();

            LoadSowInfo(Vm.SelfInfo.Pxt);
        }

        [Ability(6417638052075675648, AbilityType.ScanAction)]
        public virtual void SearchData()
        {
            Func<SowModel, bool> whereFunc1 = a => true;
            Func<SowModel, bool> whereFunc2 = a => true;
            var sowView = Vm as SowViewModel;
            if (!string.IsNullOrEmpty(sowView.SearchNPid))
                whereFunc1 = (a => a.NCartonNo.Equals(sowView.SearchNPid, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(sowView.SearchPN))
                whereFunc2 = (a => a.FactoryPN.Equals(sowView.SearchPN, StringComparison.InvariantCultureIgnoreCase)
                                   || a.PartNo.Equals(sowView.SearchPN, StringComparison.InvariantCultureIgnoreCase));
            var lstData = _scannedList.Where(whereFunc1).Where(whereFunc2);
            Vm.SelfInfo.scannedList = lstData;
        }
    }
}
