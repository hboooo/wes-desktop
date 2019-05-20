using System.Collections.ObjectModel;
using System.Windows;
using Wes.Core.Api;
using Wes.Core.Attribute;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Controls;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action
{
    /// <summary>
    /// Promaster(彦阳) 采集所用实体基类
    /// </summary>
    public class CollectingAction : ScanActionContextBase<WesFlowID, CollectingAction>, IScanActionContext
    {
        protected bool isScanQrCode = false;

        protected virtual string PnOrQrPropertyName => "PN or QrCode";
        protected virtual string PnPropertyName => "PN";
        protected virtual string GwPropertyName => "GW";
        protected virtual string MinQtyPropertyName => "Min Qty";
        protected virtual string LotPropertyName => "Lot";
        protected virtual string DcPropertyName => "DC";
        protected virtual string QtyPropertyName => "Qty";
        protected virtual string CooPropertyName => "Coo";
        protected virtual string FwPropertyName => "Fw";
        protected virtual string TraceCodePropertyName => "Trace Code";

        /// <summary>
        /// 返回全局变量集合
        /// </summary>
        /// <returns></returns>
        public object getContext()
        {
            return Vm.SelfInfo;
        }

        private void BindingAutoCompleteData()
        {
            RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "/receiving/part-end")
                .AddJsonBody(Vm.GetSelfInfo())
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
                                Name = listCode[i].ToString(),
                                Type = null,
                                Code = null
                            });
                        }

                        WesApp.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            Vm.SelfInfo.IntelligentItems = souce;
                        }));
                    }
                });
        }

        [IntelliSense(true)]
        public virtual void ScanPackageId(string scanVal)
        {
            //自动CartonEnd上一箱
            if (!string.IsNullOrEmpty(Vm.SelfInfo.pid) && scanVal != Vm.SelfInfo.pid)
            {
                try
                {
                    CartonEnd(scanVal);
                    WesDesktopSounds.Success();
                }
                catch (WesRestException ex)
                {
                    if (ex.MessageCode == 6468026496215683072 || ex.MessageCode == 6433968117520539648)
                    {
                        //ignore
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            isScanQrCode = false;
            CommonLoadPackageInfo(scanVal);

            if ((bool) Vm.SelfInfo.isCompleted)
            {
                WesModernDialog.ShowWesMessage("該票已經採集完成, 請使用RCVEND完成收貨");
                return;
            }

            #region 绑定CLCODE 数据源 

            BindingAutoCompleteData();

            #endregion
        }

        public void ScanBranch(string scanVal)
        {
            int len = 29;
            if (scanVal.ToUpper().StartsWith("1P"))
            {
                len = 31;
            }

            if (scanVal.Length >= len)
            {
                ScanQrCode(scanVal);
            }
            else
            {
                ScanPartNo(scanVal);
            }
        }

        [IntelliSense]
        protected virtual void ScanPartNo(string scanVal)
        {
            Vm.SelfInfo.spn = scanVal;

            dynamic partResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/spn")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
          
            Vm.SelfInfo.spn = partResult.spn.ToString();
            Vm.SelfInfo.pn = partResult.partNo.ToString();
            Vm.SelfInfo.isNeedGw = (bool) partResult.isNeedGw;
            Vm.SelfInfo.qty = partResult.qty;
        }

        public virtual void ScanLotNo(string scanVal)
        {
            Vm.SelfInfo.lot = scanVal;
            dynamic lotResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/lot")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.lot = lotResult.lot;
            if (lotResult.dc != null)
            {
                Vm.SelfInfo.dc = lotResult.dc;
                Vm.SelfInfo.dt = lotResult.dt;
                Vm.SelfInfo.originDc = lotResult.originDc;
            }

            if (lotResult.qty != null)
            {
                Vm.SelfInfo.qty = lotResult.qty;
            }
        }

        public virtual void ScanDcNo(string scanVal)
        {
            Vm.SelfInfo.dc = scanVal;
            dynamic dcResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/dc")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.dc = dcResult.dc;
            Vm.SelfInfo.dt = dcResult.dt;
            Vm.SelfInfo.originDc = dcResult.originDc;
        }

        public virtual void ScanQty(string scanVal)
        {
            Vm.SelfInfo.strQty = scanVal;

            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/qty")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.qty = qtyResult.qty;
        }

        public virtual void ScanQrCode(string scanVal)
        {
            Vm.SelfInfo.qrCode = scanVal;
            dynamic qcResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/qc")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.pn = qcResult.pn;
            Vm.SelfInfo.dc = qcResult.dc;
            Vm.SelfInfo.dt = qcResult.dt;
            Vm.SelfInfo.lot = qcResult.lot;
            Vm.SelfInfo.qty = qcResult.qty;
            Vm.SelfInfo.originDc = qcResult.originDc;
            
            //客户定制
            Vm.SelfInfo.coo = qcResult.coo;
            Vm.SelfInfo.fw = qcResult.fw;
            Vm.SelfInfo.seriesNO = qcResult.seriesNO;    //用来判断是否扫描fw
            
            isScanQrCode = true;
        }

        public virtual void ScanCoo(string coo)
        {
            Vm.SelfInfo.coo = coo;
            dynamic otherResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/coo")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.coo = otherResult.coo;
        }

        public void ScanFlowActionEntryGw(string scanVal)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri("collecting/part-pcs-gw")
                .AddJsonBody("rid", Vm.SelfInfo.dimGw.rowID)
                .AddJsonBody("pcsGw", double.Parse(scanVal) / double.Parse(Vm.SelfInfo.minQty))
                .Execute()
                .To<object>();
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
        }

        public void ScanMinQty(string scanVal)
        {
            Vm.SelfInfo.minQty = scanVal;
            Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_GW, GwPropertyName);
        }

        [AbilityAble(true, KPIActionType.LSDataCollectionPlus, "supplier")]
        public bool PalletEnd(string scanVal)
        {
            PrivateCartonEnd(scanVal);

            return true;
        }

        public bool RcvEnd(string scanVal)
        {
            PrivateRcvEnd(scanVal);

            return true;
        }

        [AbilityAble(true, KPIActionType.LSDataCollectionPlus, "supplier")]
        public void CartonEnd(string scanVal)
        {
            PrivateCartonEnd(scanVal);
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "supplier")]
        public virtual void Save(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUri("collecting")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute();

            CommonLoadPackageInfo(Vm.SelfInfo.pid);

            Vm.SelfInfo.integralPid = (string) Vm.SelfInfo.pid;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO, CooPropertyName);
        }

        public void DeleteCartonPart(long rid)
        {
            if (!Vm.SelfInfo.isMasterDelete)
                Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!Vm.SelfInfo.isMasterDelete) return;

            var result =
                WesModernDialog.ShowWesMessage("您确定要删除吗?", "WES_Message".GetLanguage(), MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                RestApi.NewInstance(Method.DELETE)
                    .AddUri("collecting")
                    .AddJsonBody(Vm.GetSelfInfo())
                    .AddJsonBody("rid", rid)
                    .Execute();
            }

            CommonLoadPackageInfo(Vm.SelfInfo.pid);
        }

        private void PrivateRcvEnd(string scanVal)
        {
            var binWindow = new WesUpdateBinNo($"單號 {Vm.SelfInfo.rxt}");
            binWindow.ShowDialog();
            Vm.SelfInfo.binNo = binWindow.BinNo;

            RestApi.NewInstance(Method.PUT)
                .AddUri("collecting/rcv-end")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute();

            Vm.CleanScanValue();
        }

        private void PrivateCartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri("collecting/carton-end")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute();
            Vm.SelfInfo.integralPid = (string) Vm.SelfInfo.pid;
            dynamic isComplete = RestApi.NewInstance(Method.GET)
                .AddUri("collecting/is-completed")
                .AddQueryParameter(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            if ((bool) isComplete)
            {
                PrivateRcvEnd(scanVal);
            }
            else
            {
                //解决输入carton end 之后后台返回结果后界面无反应的问题
                Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PID_OR_PN_OR_CLCODE_OR_QRCODE, PnOrQrPropertyName);
            }
        }


        protected void CommonLoadPackageInfo(string scanVal)
        {
            Vm.SelfInfo.pid = scanVal;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri("collecting")
                .AddQueryParameter(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            Vm.SelfInfo.isCompleted = result.isCompleted;
            Vm.SelfInfo.rxt = result.rxt;

            Vm.SelfInfo.cartons = result.cartons;
            Vm.SelfInfo.total = result.total;
            Vm.SelfInfo.isMasterReprint = false;
            Vm.SelfInfo.isMasterDelete = false;
        }

        protected dynamic GetPartNo(string partNo)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri("collecting/part-number")
                .AddQueryParameter(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            return result;
        }
    }
}