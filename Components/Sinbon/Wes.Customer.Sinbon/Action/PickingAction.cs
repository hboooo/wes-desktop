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
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Controls;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Utilities.Sort;
using Wes.Wrapper;

namespace Wes.Customer.Sinbon.Action
{
    /// <summary>
    /// 拣货业务集合
    /// </summary>
    public class PickingAction : ScanActionBase<WesFlowID, PickingAction>, IScanAction
    {
        #region 成員變量初始化
        private IEnumerable<PickModel> _todoList = new List<PickModel>(); //所有待作業
        private IEnumerable<PickModel> _nextList = new List<PickModel>(); //將要進行作業
        private IEnumerable<PickModel> _scanningList = new List<PickModel>(); //作業中
        private string _currentPackageID = string.Empty;
        private string _currentBinNo = string.Empty;
        private string _currentShipper = string.Empty;
        private string _currentConsignee = string.Empty;
        #endregion

        #region 掃描PXT
        [Ability(6445541963906158592, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {
            if (!scanValue.IsPxt())
            {
                throw new WesException("Pxt 不合法");
            }
            string result = RestApi.NewInstance(Method.GET)
                .AddUri("/picking/check-pxt")
                .AddQueryParameter("operationNo", scanValue)
                .Execute()
                .To<string>();
            Vm.SelfInfo.Target = result;
            Vm.SelfInfo.Pxt = scanValue;
            Vm.SelfInfo.useIntelligent = false;
            LoadPackageInfo(scanValue, string.Empty, string.Empty);
            if (string.IsNullOrEmpty(Vm.SelfInfo.totalPxtQty)) return;
            BindingAutoComplateData();
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }
        #endregion

        #region 掃描PID
        [Ability(6445531042307645440, AbilityType.ScanAction)]
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
            string isOriginalCarton = "";

            if (isExistTodoList && !isExistNextList)
            {
                isOriginalCarton = _todoList.First(a => a.PackageID.Equals(_currentPackageID, StringComparison.CurrentCultureIgnoreCase)).IsOriginalCarton;
                currentScanningBinNo = _todoList.First(a => a.PackageID.Equals(_currentPackageID, StringComparison.CurrentCultureIgnoreCase)).BinNo;
                LoadPackageInfo(Vm.SelfInfo.Pxt.ToString(), currentScanningBinNo, string.Empty);
            }

            if (isExistNextList)
            {
                isOriginalCarton = _nextList.First(a => a.PackageID.Equals(_currentPackageID, StringComparison.CurrentCultureIgnoreCase)).IsOriginalCarton;
            }
            _currentBinNo = currentScanningBinNo;
            Vm.SelfInfo.currentPid = _currentPackageID.ToUpper();
            Vm.SelfInfo.currentBinNo = currentScanningBinNo.ToUpper().ToString();
            _nextList = _nextList.Where(a => !string.Equals(a.PackageID, _currentPackageID, StringComparison.CurrentCultureIgnoreCase));
            if (isOriginalCarton.Equals("Y"))
            {
                //直接扫箱号即完成，无需扫料号
                ScanPackageIdEnd();
                return;
            }

            DataTable dtResult = RestApi.NewInstance(Method.GET)
                .AddUri("/picking/load-partNo")
                .AddQueryParameter("operationNo", Vm.SelfInfo.Pxt.ToString())
                .AddQueryParameter("packageId", scanValue)
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

            Vm.SelfInfo.totalPidQty = _scanningList.Sum(a => a.Reels).ToString();
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

        //扫箱号直接结束
        [AbilityAble(6454896045640916992, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCarton, "Target", false)]
        protected virtual bool ScanPackageIdEnd()
        {
            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUri("/picking/save-pid")
                .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                .AddJsonBody("PackageID", _currentPackageID)
                .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                .Execute()
                .To<object>();
            Vm.SelfInfo.Qty = 1;
            Vm.SelfInfo.Pid = _currentPackageID;
            BindCurrentScanningList(Vm.SelfInfo.Pxt.ToString(), _currentPackageID);
            return true;
        }

        #endregion

        #region 掃描PN
        [AbilityAble(6445531246654132224, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCarton, "Target", false)]
        public virtual bool ScanFlowActionScanPartNo(string scanValue)
        {
            var partNo = string.Empty;
            PickModel currentScanItem = null;
            //C03690需支持扫QRCode解析出CLCode和手输CLCode(手输需支持自动完成)
            //C02973和C02975 需支持掃QRCode解析出PN和手輸PN
            if (scanValue.Length > 30)
            {
                #region 解析QRCode
                dynamic qrCode = RestApi.NewInstance(Method.POST)
                    .SetUrl(RestUrlType.WmsServer, "{si}")
                    .AddScriptId((long)ScriptSid.QR_CODE_DISPATCH)
                    .AddJsonBody("qrCode", scanValue)
                    .AddJsonBody("shipper", _currentShipper)
                    .Execute()
                    .To<object>();
                if (qrCode != null)
                {
                    switch (_currentShipper)
                    {
                        case "C03690":
                            partNo = qrCode.clCode;
                            currentScanItem = _scanningList.First(a => a.CLCode.Equals(partNo, StringComparison.CurrentCultureIgnoreCase));
                            Vm.SelfInfo.scanningPn = currentScanItem.CLCode + "     " + currentScanItem.DateCode + "     " + currentScanItem.Qty;
                            break;
                        case "C02973":
                        case "C02975":
                            partNo = qrCode.pn;
                            currentScanItem = _scanningList.First(a => a.PartNo.Equals(partNo, StringComparison.CurrentCultureIgnoreCase));
                            Vm.SelfInfo.scanningPn = currentScanItem.PartNo + "     " + currentScanItem.DateCode + "     " + currentScanItem.Qty;
                            break;
                    }
                }
                else
                {
                    WesModernDialog.ShowWesMessage("QRCode解析失敗,請重新掃描！");
                    return false;
                }
                #endregion
            }
            else
            {
                #region 處理CLCode或者PN
                if (_currentShipper == "C03690")
                {
                    scanValue = scanValue.Trim();
                    if (scanValue.IndexOf(" ", StringComparison.Ordinal) >= 0)
                    {
                        scanValue = scanValue.Replace(" ", "-");
                    }
                    partNo = scanValue;
                    var isExist = _scanningList.Any(a => a.CLCode.Equals(partNo, StringComparison.CurrentCultureIgnoreCase));
                    if (!isExist)
                    {
                        WesModernDialog.ShowWesMessage("掃描CLCode失敗，失敗原因：" + partNo + " 無效！");
                        return false;
                    }
                    currentScanItem = _scanningList.First(a => a.CLCode.Equals(partNo, StringComparison.CurrentCultureIgnoreCase));
                    Vm.SelfInfo.scanningPn = currentScanItem.CLCode + "     " + currentScanItem.DateCode + "     " + currentScanItem.Qty;
                }
                else
                {
                    partNo = scanValue;
                    var isExist = _scanningList.Any(a => a.PartNo.Equals(partNo, StringComparison.CurrentCultureIgnoreCase));
                    if (!isExist)
                    {
                        WesModernDialog.ShowWesMessage("掃描料號失敗，失敗原因：" + partNo + " 無效！");
                        return false;
                    }
                    currentScanItem = _scanningList.First(a => a.PartNo.Equals(partNo, StringComparison.CurrentCultureIgnoreCase));
                    Vm.SelfInfo.scanningPn = currentScanItem.PartNo + "     " + currentScanItem.DateCode + "     " + currentScanItem.Qty;
                }


                #endregion
            }
            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUri("/picking/save")
                .AddJsonBody("OperationNo", Vm.SelfInfo.Pxt.ToString())
                .AddJsonBody("PackageID", _currentPackageID)
                .AddJsonBody("PartNo", currentScanItem.PartNo)
                .AddJsonBody("CLCode", currentScanItem.CLCode)
                .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                .Execute()
                .To<object>();
            Vm.SelfInfo.Qty = 1;
            BindCurrentScanningList(Vm.SelfInfo.Pxt.ToString(), _currentPackageID);
            return true;
        }
        #endregion

        #region 加載當前掃描箱料號信息
        private void BindCurrentScanningList(string operationNo, string packageID)
        {
            DataTable dsScanPackageId = RestApi.NewInstance(Method.GET)
                .AddUri("/picking/load-partNo")
                .AddQueryParameter("operationNo", operationNo)
                .AddQueryParameter("packageId", packageID)
                .Execute()
                .To<DataTable>();
            _scanningList = dsScanPackageId.ToList<PickModel>();
            if (_scanningList.Any())
            {
                Vm.SelfInfo.scanning = _scanningList;
                _currentShipper = _scanningList.First().Shipper;
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PART_NO);
                var dtResult = RestApi.NewInstance(Method.GET)
                    .AddUri("/picking/load-package")
                    .AddQueryParameter("operationNo", operationNo)
                    .Execute()
                    .To<DataTable>();
                if (dtResult.Rows.Count == 0) return;
                dtResult.DefaultView.Sort = "Sort";
                dtResult = dtResult.DefaultView.ToTable();
                dtResult = BinNoSort.SortTodoList(dtResult, _currentPackageID);
                _todoList = dtResult.ToList<PickModel>();
                Vm.SelfInfo.totalPxtQty = _todoList.Sum(a => a.Reels).ToString();
                Vm.SelfInfo.totalPidQty = _todoList.Where(a => a.PackageID.Equals(packageID, StringComparison.CurrentCultureIgnoreCase)).Sum(a => a.Reels).ToString();
                Vm.SelfInfo.totalPidCount = _todoList.Select(a => a.PackageID).Distinct().Count().ToString();
            }
            else
            {
                UpdateStorage(packageID);
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
        #endregion

        #region 更新儲位
        [AbilityAble(6445564117091622912, AbilityType.ScanAction, true, KPIActionType.LSPickingSplitCartonPlus, "Target", false)]
        protected virtual bool UpdateStorage(string packageID)
        {
            //判断是否需要更新储位
            string yushuQty = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId((long)ScriptSid.GET_YUSHU_QTY)
                .AddQueryParameter("pid", packageID)
                .Execute()
                .To<string>();
            if (!string.IsNullOrEmpty(yushuQty) && Convert.ToInt32(yushuQty) > 0)
            {
                //余数Qty大于0，代表该箱还有余数，需更新储位
                var updateModel = new CommonUpdateBinNoModel
                {
                    ActionType = "pick",
                    OperationNo = Vm.SelfInfo.Pxt,
                    PackageId = packageID,
                    YushuQty = yushuQty,
                    UpdateUser = WesDesktop.Instance.User.Code
                };
                var updateBinNoWin = new WesUpdateBinNo(updateModel);
                updateBinNoWin.ShowDialog();
            }
            return true;
        }
        #endregion

        #region 加載TodoList和NextList
        [Ability(6445542024295751680, AbilityType.ScanAction)]
        private void LoadPackageInfo(string scanValue, string binNo, string binNoNext)
        {
            var dtResult = RestApi.NewInstance(Method.GET)
                .AddUri("/picking/load-package")
                .AddQueryParameter("operationNo", scanValue)
                .Execute()
                .To<DataTable>();
            if (dtResult.Rows.Count == 0)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PICKING_NO);
                Vm.SelfInfo.scanningPn = string.Empty;
                Vm.SelfInfo.totalPxtQty = string.Empty;
                return;
            }
            dtResult.DefaultView.Sort = "Sort ASC";
            dtResult = dtResult.DefaultView.ToTable();
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
        #endregion
    }
}
