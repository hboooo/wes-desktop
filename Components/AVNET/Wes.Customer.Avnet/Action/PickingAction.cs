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
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Utilities.Sort;
using Wes.Wrapper;

namespace Wes.Customer.Avnet.Action
{
    /// <summary>
    /// 拣货动作集合
    /// </summary>
    public class PickingAction : ScanActionBase<WesFlowID, PickingAction>, IScanAction
    {
        private const long CheckPxtScriptId = 6412882377395146752;
        private const long GetPackageInfoScriptId = 6412948505882529792;
        private const long GetPartNoInfoScriptId = 6413597000217927680;
        private const long InsertDataScriptId = 6413638807177924608;
        private const long QrCodeScriptId = 6402348645168975872;
        private const long DecodeScriptId = 6402348645173174282;
        private const long GetYushuInfoScriptId = 6417687321541349376;
        private const long UpdateBinNoScriptId = 6417651280818216960;

        private IEnumerable<PickModel> _todoList = new List<PickModel>(); //所有待作業
        private IEnumerable<PickModel> _nextList = new List<PickModel>(); //將要進行作業
        private IEnumerable<PickModel> _scanningList = new List<PickModel>(); //作業中
        private string _currentPackageID = string.Empty;
        private string _currentBinNo = string.Empty;
        private string _currentShipper = string.Empty;
        private string _currentConsignee = string.Empty;


        [Ability(6412901264241205248, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {

            string result = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(CheckPxtScriptId)
                .AddQueryParameter("pxt", scanValue)
                .Execute()
                .To<string>();

            _currentConsignee = result;
            Vm.SelfInfo.Target = _currentConsignee;
            LoadPackageInfo(scanValue, string.Empty, string.Empty);
            Vm.SelfInfo.Pxt = scanValue;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            SetActionValid();
        }

        [Ability(6413596675343912960, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPackageId(string scanValue)
        {
            Vm.SelfInfo.scanningPn = string.Empty;
            var isExistTodoList = _todoList.Any(a => a.PackageID.Equals(scanValue, StringComparison.CurrentCultureIgnoreCase));
            var isExistNextList = _nextList.Any(a => a.PackageID.Equals(scanValue, StringComparison.CurrentCultureIgnoreCase));
            if (!isExistNextList && !isExistTodoList)
            {
                WesModernDialog.ShowWesMessage("掃描箱號失敗,原因：" + scanValue + " 無效！");
                return;
            }
            _currentPackageID = scanValue;
            var currentScanningBinNo = Vm.SelfInfo.nextBinNo.ToString();
            if (isExistTodoList && !isExistNextList)
            {
                currentScanningBinNo = _todoList.First(a => a.PackageID.Equals(_currentPackageID, StringComparison.CurrentCultureIgnoreCase)).BinNo;
                LoadPackageInfo(Vm.SelfInfo.Pxt.ToString(), currentScanningBinNo, string.Empty);
            }
            _currentBinNo = currentScanningBinNo;

            DataTable dtResult = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(GetPartNoInfoScriptId)
                .AddQueryParameter("pxt", Vm.SelfInfo.Pxt.ToString())
                .AddQueryParameter("pid", scanValue)
                .Execute()
                .To<DataTable>();
            _scanningList = dtResult.ToList<PickModel>();
            if (_scanningList == null || !_scanningList.Any())
            {
                WesModernDialog.ShowWesMessage("掃描箱號失敗,未找到對應箱數據!");
                return;
            }
            _currentShipper = _scanningList.First().Shipper;
            Vm.SelfInfo.scanning = _scanningList;
            Vm.SelfInfo.currentPid = _currentPackageID.ToUpper();
            Vm.SelfInfo.currentBinNo = currentScanningBinNo.ToUpper().ToString();
            Vm.SelfInfo.totalPidQty = _scanningList.Sum(a => a.Reels).ToString();
            _nextList = _nextList.Where(a => !string.Equals(a.PackageID, _currentPackageID, StringComparison.CurrentCultureIgnoreCase));
            if (!_nextList.Any())
            {
                var nextItem = _todoList.Where(a => !string.Equals(a.BinNo, _currentBinNo, StringComparison.CurrentCultureIgnoreCase));
                if (nextItem.Any())
                {
                    var binNoNext = nextItem.First().BinNo;
                    LoadPackageInfo(Vm.SelfInfo.Pxt.ToString(), _currentBinNo, binNoNext);
                }
                else
                {
                    Vm.SelfInfo.next = null;
                    Vm.SelfInfo.nextBinNo = string.Empty;
                }
            }
            else
            {
                Vm.SelfInfo.next = _nextList;
            }
            Vm.SelfInfo.Pid = _currentPackageID;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PART_NO);
        }

        [Ability(6413638872684568576, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPartNo(string scanValue)
        {
            var partNo = string.Empty;
            if (_currentShipper == "C03305")
            {
                partNo = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "{si}")
                    .AddScriptId(DecodeScriptId)
                    .AddBranch("MPN")
                    .AddQueryParameter("mpn", scanValue)
                    .Execute()
                    .To<string>();
            }
            else
            {
                if (scanValue.Contains('{'))
                {
                    dynamic qrCode = RestApi.NewInstance(Method.POST)
                        .SetUrl(RestUrlType.WmsServer, "{si}")
                        .AddScriptId(QrCodeScriptId)
                        .AddJsonBody("qrCode", scanValue)
                        .Execute()
                        .To<object>();
                    partNo = qrCode.opn;
                }
                else
                {
                    partNo = scanValue;
                }

            }
            var isExistPartNo = _scanningList.Any(a => a.FactoryPN.Equals(partNo, StringComparison.CurrentCultureIgnoreCase));
            if (!isExistPartNo)
            {
                WesModernDialog.ShowWesMessage("掃描料號失敗，失敗原因：" + partNo + " 無效！");
                return;
            }

            var currentScanPartNo = _scanningList.First(a => a.FactoryPN.Equals(partNo, StringComparison.CurrentCultureIgnoreCase));
            Vm.SelfInfo.scanningPn = currentScanPartNo.FactoryPN + "     " + currentScanPartNo.DataCode + "     " + currentScanPartNo.LotNos + "     " + currentScanPartNo.Qty;
            dynamic result = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(InsertDataScriptId)
                .AddQueryParameter("pxt", Vm.SelfInfo.Pxt.ToString())
                .AddQueryParameter("pid", _currentPackageID)
                .AddQueryParameter("pn", currentScanPartNo.PartNo)
                .AddQueryParameter("user", WesDesktop.Instance.User.Code)
                .Execute()
                .To<object>();
            Vm.SelfInfo.Qty = 1;
            BindCurrentScanningList(Vm.SelfInfo.Pxt.ToString(), _currentPackageID);
            SetActionValid();
        }

        private void BindCurrentScanningList(string operationNo, string packageID)
        {
            DataTable dsScanPackageId = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(GetPartNoInfoScriptId)
                .AddQueryParameter("pxt", operationNo)
                .AddQueryParameter("pid", packageID)
                .Execute()
                .To<DataTable>();
            _scanningList = dsScanPackageId.ToList<PickModel>();
            if (_scanningList.Any())
            {
                Vm.SelfInfo.NeedAddPlus = false;
                Vm.SelfInfo.scanning = _scanningList;
                _currentShipper = _scanningList.First().Shipper;
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PART_NO);
                var dtResult = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                    .AddScriptId(GetPackageInfoScriptId)
                    .AddQueryParameter("pxt", operationNo)
                    .Execute()
                    .To<DataTable>();
                if (dtResult.Rows.Count == 0) return;
                dtResult = BinNoSort.SortSetpLine(dtResult, "Lane,RackNo", "HorizontalNo", "RackNo", true, "VerticalNo DESC");
                dtResult = BinNoSort.SortTodoList(dtResult, _currentPackageID);
                _todoList = dtResult.ToList<PickModel>();
                Vm.SelfInfo.totalPxtQty = _todoList.Sum(a => a.Reels).ToString();
                Vm.SelfInfo.totalPidQty = _todoList.Where(a => a.PackageID.Equals(packageID, StringComparison.CurrentCultureIgnoreCase)).Sum(a => a.Reels).ToString();
                Vm.SelfInfo.totalPidCount = _todoList.Select(a => a.PackageID).Distinct().Count().ToString();
            }
            else
            {
                //判断是否需要更新储位
                string yushuQty = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                    .AddScriptId(GetYushuInfoScriptId)
                    .AddQueryParameter("pid", packageID)
                    .Execute()
                    .To<string>();
                if (!string.IsNullOrEmpty(yushuQty) && Convert.ToInt32(yushuQty) > 0)
                {
                    //余数Qty大于0，代表该箱还有余数，需更新储位
                    var updateModel = new CommonUpdateBinNoModel
                    {
                        ActionType = "pick",
                        OperationNo = operationNo,
                        PackageId = packageID,
                        YushuQty = yushuQty,
                        UpdateUser = WesDesktop.Instance.User.Code
                    };
                    var updateBinNoWin = new WesUpdateBinNo(updateModel);
                    updateBinNoWin.ShowDialog();
                }
                //已扫完整箱需添加Plu积分
                Vm.SelfInfo.NeedAddPlus = true;

                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
                Vm.SelfInfo.scanning = null;
                Vm.SelfInfo.totalPidCount = string.Empty;
                Vm.SelfInfo.currentPid = string.Empty;
                Vm.SelfInfo.currentBinNo = string.Empty;
                Vm.SelfInfo.totalPidQty = string.Empty;

                if (_nextList.Any())
                {
                    _currentPackageID = _nextList.First().PackageID;
                    LoadPackageInfo(operationNo, Vm.SelfInfo.nextBinNo.ToString(), string.Empty);
                }
                else
                {
                    LoadPackageInfo(operationNo, string.Empty, string.Empty);
                }
            }
            Vm.SelfInfo.next = _nextList;
        }

        [Ability(6412944185795547136, AbilityType.ScanAction)]
        private void LoadPackageInfo(string scanValue, string binNo, string binNoNext)
        {
            var dtResult = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(GetPackageInfoScriptId)
                .AddQueryParameter("pxt", scanValue)
                .Execute()
                .To<DataTable>();
            if (dtResult.Rows.Count == 0)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
                Vm.SelfInfo.scanningPn = string.Empty;
                Vm.SelfInfo.totalPxtQty = string.Empty;
                return;
            }
            dtResult = BinNoSort.SortSetpLine(dtResult, "Lane,RackNo", "HorizontalNo", "RackNo", true, "VerticalNo DESC");
            dtResult = BinNoSort.SortTodoList(dtResult, _currentPackageID);
            _todoList = dtResult.ToList<PickModel>();
            string nextBinNo = string.Empty;
            if (string.IsNullOrEmpty(binNoNext))
            {
                nextBinNo = string.IsNullOrEmpty(binNo) ? _todoList.First().BinNo : binNo;
                var toListItem = _todoList.Where(a => !string.Equals(a.BinNo, nextBinNo, StringComparison.CurrentCultureIgnoreCase));
                if (!toListItem.Any())
                {
                    Vm.SelfInfo.totalPidCount = string.Empty;
                }
                else
                {
                    Vm.SelfInfo.totalPidCount = _todoList.Select(a => a.PackageID).Distinct().Count().ToString();
                }
                Vm.SelfInfo.todo = toListItem;
                Vm.SelfInfo.totalPxtQty = _todoList.Sum(a => a.Reels).ToString();
            }
            else
            {
                nextBinNo = binNoNext;
                var toListItem = _todoList.Where(a => !string.Equals(a.BinNo, binNo, StringComparison.CurrentCultureIgnoreCase))
                    .Where(a => !string.Equals(a.BinNo, binNoNext, StringComparison.CurrentCultureIgnoreCase));
                Vm.SelfInfo.todo = toListItem;
            }
            Vm.SelfInfo.nextBinNo = nextBinNo.ToUpper();
            _nextList = _todoList.Where(a => a.BinNo.Equals(nextBinNo, StringComparison.CurrentCultureIgnoreCase));
            Vm.SelfInfo.next = _nextList;
        }
    }
}
