using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Wes.Component.Widgets.APIAddr;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Controls;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Sinbon.Action
{
    public class CollectingAction : ScanActionBase<WesFlowID, CollectingAction>, IScanAction
    {
        //rxt->packageid-pn or qrcode-> qty -> lot or dc 
        public virtual void ScanFlowActionScanPackageId(string scanVal)
        {
            //自动CartonEnd上一箱
            if (DependencyProperty.UnsetValue != base.Vm.SelfInfo.pid &&
                !string.IsNullOrEmpty(base.Vm.SelfInfo.pid) &&
                scanVal != base.Vm.SelfInfo.pid)
            {
                this.CartonEnd(scanVal);
                WesDesktopSounds.Success();
            }

            dynamic result = LoadPackageInfo(scanVal);
            base.Vm.SelfInfo.shipper = (string) result.shipper;

            //如果已完成, 提示就好,正常顯示數據
            if ((bool) result.isCompleted)
            {
                WesModernDialog.ShowWesMessage("已完成");
                return;
            }

            if (base.Vm.SelfInfo.shipper.ToString().ToUpper() == "C03690")
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_ClCode_NO);
            }
            else
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
            }

            #region 绑定CLCODE 数据源 

            BindingAutoComplateData();

            #endregion
        }

        private void BindingAutoComplateData()
        {
            RestApi.NewInstance(Method.GET)
                .AddUri("/collecting/part-end")
                .AddQueryParameter("rcv", (string) base.Vm.SelfInfo.rcv)
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

                        WesApp.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            base.Vm.SelfInfo.UseIntelligent = true;
                            base.Vm.SelfInfo.IntelligentItems = souce;
                        }));
                    }
                });
        }

        public virtual void ScanFlowActionScanClcodeNo(string scanVal)
        {
            base.Vm.SelfInfo.UseIntelligent = false;
            //验证当前扫描的是PN还是QRCODE
            if (scanVal.Length > 30)
            {
                ScanFlowActionScanQrcode(scanVal);
            }
            else
            {
                dynamic rules = RestApi.NewInstance(Method.POST)
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.Other_DISPATCH)
                    .AddBranch("CL_CODE")
                    .AddJsonBody("clCode", scanVal)
                    .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                    .Execute()
                    .To<object>();
                base.Vm.SelfInfo.mpn = rules.pn;
                base.Vm.SelfInfo.clCode = rules.clCode;
                base.Vm.SelfInfo.minQty = rules.minQty;

                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
            }
        }

        public virtual void ScanFlowActionScanMpn(string scanVal)
        {
            base.Vm.SelfInfo.UseIntelligent = false;
            string tempPartNo = string.Empty;
            dynamic rules = null;
            //验证当前扫描的是PN还是QRCODE
            if (scanVal.Length > 30)
            {
                ScanFlowActionScanQrcode(scanVal);
            }
            else
            {
                //check cl_code 
                
                
                
                if (base.Vm.SelfInfo.shipper.ToString().ToUpper() == "C03690")
                {
                    rules = RestApi.NewInstance(Method.POST)
                        .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.Other_DISPATCH)
                        .AddBranch("CL_CODE")
                        .AddJsonBody("clCode", scanVal)
                        .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                        .Execute()
                        .To<object>();
                    base.Vm.SelfInfo.mpn = rules.pn;
                    base.Vm.SelfInfo.clCode = rules.clCode;
                    base.Vm.SelfInfo.minQty = rules.minQty;
                    //1. 添加对品牌为HRS的料号进行包装判断
//                    base.Vm.SelfInfo.textTips = "Please collect box";
                    if (rules.packageUnit != null && rules.packageUnit ==1)
                    {
                        base.Vm.SelfInfo.textTips = "Please collect box";
                    }

                    this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
                }
                else if (base.Vm.SelfInfo.shipper.ToString().ToUpper() == "C02973" ||
                         base.Vm.SelfInfo.shipper.ToString().ToUpper() == "C02975")
                {
                    rules = RestApi.NewInstance(Method.POST)
                        .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.Other_DISPATCH)
                        .AddBranch("PN")
                        .AddJsonBody("pn", scanVal)
                        .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                        .Execute()
                        .To<object>();
                    base.Vm.SelfInfo.mpn = rules.pn;
                    //1. 添加对品牌为HRS的料号进行包装判断
//                    base.Vm.SelfInfo.textTips = "Please collect box";
                    if (rules.packageUnit != null && rules.packageUnit ==1)
                    {
                        base.Vm.SelfInfo.textTips = "Please collect box";
                    }
                    
                    this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO);
                }
            }
        }

        public virtual void ScanFlowActionScanLotNo(string scanVal)
        {
            if (base.Vm.SelfInfo.shipper.ToString().ToUpper() != "C02975")
            {
                ScanFlowActionScanDcNo(scanVal);
            }
            base.Vm.SelfInfo.lot = scanVal;
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY);
        }

        public virtual bool ScanFlowActionScanQty(string scanVal)
        {
            int ScalQty = Convert.ToInt32(scanVal);
            dynamic qty;
            if (!string.IsNullOrEmpty(base.Vm.SelfInfo.mpn.ToString()))
            {
                //decode qty
                qty = RestApi.NewInstance(Method.POST)
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.Other_DISPATCH)
                    .AddBranch("QTY")
                    .AddJsonBody("qty", ScalQty)
                    .AddJsonBody("pn", base.Vm.SelfInfo.mpn.ToString())
                    .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                    .Execute()
                    .To<object>();
            }
            else
            {
                //decode qty
                qty = RestApi.NewInstance(Method.POST)
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.Other_DISPATCH)
                    .AddBranch("QTY")
                    .AddJsonBody("qty", ScalQty)
                    .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                    .Execute()
                    .To<object>();
            }

            //TODO:如果有暗码，此处处理
            int intQty = Convert.ToInt32(qty.qty.Value);
            base.Vm.SelfInfo.qty = intQty;
            //Qty 必須大於0
            if (intQty <= 0)
            {
                throw new WesException("無效 Qty");
            }

            this.Save();
            return true;
        }

        public virtual void ScanFlowActionScanDcNo(string scanVal)
        {
            ScanFlowDateCode(scanVal);
            if (base.Vm.SelfInfo.shipper.ToString().ToUpper() != "C02975")
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY);
            }
        }

        public void ScanFlowDateCode(string scanVal)
        {
            //decode dc
            dynamic dcRule = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DATE_CODE_DISPATCH)
                .AddJsonBody("dc", scanVal)
                .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                .Execute()
                .To<object>();
            Vm.SelfInfo.dc = (object) dcRule.dc;
            Vm.SelfInfo.dt = (object) dcRule.dt;
            Vm.SelfInfo.originDc = dcRule.originDc;
            Vm.SelfInfo.formatDc = dcRule.formatDc;

            if (Vm.SelfInfo.shipper.ToString().ToUpper() == "C02975")
            {
                if (Vm.SelfInfo.lot != null )
                {
                    Save();
                }
                else
                {
                    Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
                }
                
            }
        }

        public virtual void ScanFlowActionScanQrcode(string scanVal)
        {
            //解析QrCode
            dynamic qrCode = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.QR_CODE_DISPATCH)
                .AddJsonBody("qrCode", scanVal)
                .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                .Execute()
                .To<object>();

            if ((int) qrCode.qty <= 0)
            {
                throw new WesException("無效 Qty");
            }
            //添加对HRS品牌的料号信息的校验，并判断包装类型的返回值

            //1. 添加对品牌为HRS的料号进行包装判断
//                    base.Vm.SelfInfo.textTips = "Please collect box";
            if (qrCode.packageUnit != null && qrCode.packageUnit ==1)
            {
                base.Vm.SelfInfo.textTips = "Please collect box";
            }


            if (base.Vm.SelfInfo.shipper == "C03690")
            {
                base.Vm.SelfInfo.lot = qrCode.lot;
                ScanFlowDateCode(qrCode.lot.ToString());
                base.Vm.SelfInfo.qty = (int) qrCode.qty;

                dynamic rules = RestApi.NewInstance(Method.POST)
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.Other_DISPATCH)
                    .AddBranch("CL_CODE")
                    .AddJsonBody("clCode", qrCode.clCode)
                    .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                    .Execute()
                    .To<object>();
                base.Vm.SelfInfo.mpn = rules.pn;
                base.Vm.SelfInfo.clCode = rules.clCode;
                this.Save();
            }
            else if (base.Vm.SelfInfo.shipper == "C02973")
            {
                ScanFlowDateCode(qrCode.dc.Value);
                base.Vm.SelfInfo.lot = qrCode.lot;
                base.Vm.SelfInfo.mpn = qrCode.pn;
                base.Vm.SelfInfo.qty = (int) qrCode.qty;
                base.Vm.SelfInfo.clCode = qrCode.clCode;
                this.Save();
            }
            else if (base.Vm.SelfInfo.shipper == "C02975")
            {
                if ((int) qrCode.meta == 9)
                {
                    Vm.SelfInfo.mpn = qrCode.pn;
                    Vm.SelfInfo.qty = (int) qrCode.qty;
                    Vm.SelfInfo.lot = qrCode.lot;
                    Vm.SelfInfo.supplier = qrCode.supplier;
                    Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO);
                }
                else if ((int) qrCode.meta == 15)
                {
                    base.Vm.SelfInfo.mpn = qrCode.pn;
                    base.Vm.SelfInfo.dc = qrCode.dc;
                    base.Vm.SelfInfo.dt = qrCode.dt;
                    base.Vm.SelfInfo.lot = qrCode.lot;
                    base.Vm.SelfInfo.qty = (int) qrCode.qty;
                    Vm.SelfInfo.supplier = qrCode.supplier;
                    this.Save();
                }
            }
           
        }

        public virtual void PalletEnd(string scanVal)
        {
            PrivateCartonEnd(scanVal);
        }

        public virtual bool RcvEnd(string scanVal)
        {
            PrivateRcvEnd();

            return true;
        }

        [AbilityAble(true, KPIActionType.LSDataCollectionPlus, "shipper")]
        public virtual bool CartonEnd(string scanVal)
        {
            this.PrivateCartonEnd(scanVal);

            return true;
        }

        public virtual void DeleteData(long rid)
        {
            if (!base.Vm.SelfInfo.isMasterDelete)
                base.Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!base.Vm.SelfInfo.isMasterDelete) return;

            var result = WesModernDialog.ShowWesMessage("您确定要删除吗?", "WES_Message".GetLanguage(),
                MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                RestApi.NewInstance(Method.DELETE)
                    .AddUri("/collecting")
                    .AddJsonBody("rid", rid)
                    .Execute();
            }

            LoadPackageInfo(base.Vm.SelfInfo.pid);
        }

        /// <summary>
        /// 添加RePrint指令
        /// </summary>
        public virtual void RePrint(string scanVal)
        {
            if (base.Vm.SelfInfo.cartons == null || base.Vm.SelfInfo.cartons.Count == 0)
            {
                throw new WesException("無數據,指令無效");
            }

            if (base.Vm.SelfInfo.cartons.Count > 1)
            {
                throw new WesException("單條數據才可使用RePrint指令,指令無效");
            }

            foreach (var item in base.Vm.SelfInfo.cartons)
            {
                Print(item);
            }
        }

        /// <summary>
        /// Sinbon采集完成后需要贴采集后的QrCode,避免出货再次手Key
        /// </summary>
        /// <param name="rid"></param>
        public virtual void PrintData(long rid)
        {
            foreach (var item in base.Vm.SelfInfo.cartons)
            {
                if (item.rowNo == rid)
                {
                    Print(item);
                    break;
                }
            }
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="data"></param>
        private void Print(dynamic item)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            DateTime dc = Convert.ToDateTime(item.productionDate.ToString());
            string lot = dc.ToString("yyyyMMdd").Right(6);
            dic.Add("CLCODE", item.batchNo);
            dic.Add("LotNO", lot);
            dic.Add("Qty", item.qty);

            PrintTemplateModel ptm = new PrintTemplateModel();
            ptm.TemplateFileName = "SB_Receiving_PKGID.btw";
            ptm.PrintDataValues = dic;
            LabelPrintBase labelPrint = new LabelPrintBase(ptm);
            labelPrint.Print();
        }

        private void PrivateRcvEnd()
        {
            if (string.IsNullOrEmpty(base.Vm.SelfInfo.pid))
            {
                throw new WesException("箱号不能为空");
            }

            var binNoModel = new CommonUpdateBinNoModel
            {
                ActionType = "rcv",
                OperationNo = base.Vm.SelfInfo.rcv.ToString(),
                PackageId = base.Vm.SelfInfo.rcv.ToString(),
                YushuQty = "",
                UpdateUser = WesDesktop.Instance.User.Code
            };
            WesUpdateBinNo binNo = new WesUpdateBinNo(binNoModel, BinAction.Add);
            binNo.ShowDialog();

            RestApi.NewInstance(Method.PUT)
                .AddUri("/collecting/rcv-end")
                .AddJsonBody("binNo", binNoModel.BinNo)
                .AddJsonBody("rcv", base.Vm.SelfInfo.rcv)
                .Execute();


            base.Vm.CleanScanValue();
        }

        private void PrivateCartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri("/collecting/carton-end")
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .AddJsonBody("rcv", base.Vm.SelfInfo.rcv)
                .Execute();


            base.Vm.SelfInfo.integralPid = (string) base.Vm.SelfInfo.pid;
            dynamic isComplete = RestApi.NewInstance(Method.GET)
                .AddUri("/collecting/is-complete")
                .AddQueryParameter("rcv", (string) base.Vm.SelfInfo.rcv)
                .Execute()
                .To<object>();

            if ((bool) isComplete)
            {
                this.PrivateRcvEnd();
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            }
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "shipper")]
        public virtual bool Save()
        {
            RestApi.NewInstance(Method.POST)
                .AddUri("/collecting")
                .AddJsonBody("rcv", base.Vm.SelfInfo.rcv.ToString())
                .AddJsonBody("pid", (object) base.Vm.SelfInfo.pid)
                .AddJsonBody("lot", base.Vm.SelfInfo.lot)
                .AddJsonBody("qty", Convert.ToInt32(base.Vm.SelfInfo.qty))
                .AddJsonBody("mpn", base.Vm.SelfInfo.mpn.ToString())
                .AddJsonBody("dt", base.Vm.SelfInfo.dt.ToString())
                .AddJsonBody("dc", base.Vm.SelfInfo.dc.ToString())
                .AddJsonBody("clCode", base.Vm.SelfInfo.clCode == null ? "" : base.Vm.SelfInfo.clCode.ToString())
                .Execute();
            try
            {
                this.LoadPackageInfo(base.Vm.SelfInfo.pid);
            }
            catch (WesRestException ex)
            {
                //保存完畢查詢箱不提示異常
                if (ex.MessageCode != 6438394657708707840)
                {
                    throw ex;
                }
            }

            base.Vm.SelfInfo.integralPid = (string) base.Vm.SelfInfo.pid;
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
            return true;
        }

        private dynamic LoadPackageInfo(string pid)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri("/collecting")
                .AddQueryParameter("packageID", pid)
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.rcv = (string) result.receivingNo;
            base.Vm.SelfInfo.cartons = result.cartons;
            base.Vm.SelfInfo.total = (int) result.cartonSum;
            base.Vm.SelfInfo.shipper = (string) result.shipper;
            base.Vm.SelfInfo.palletId = (string) result.palletId;
            base.Vm.SelfInfo.minPid = (string) result.minPid;
            base.Vm.SelfInfo.pid = (string) pid;
            base.Vm.SelfInfo.isMasterDelete = false;
            return result;
        }
    }
}