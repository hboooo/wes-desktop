using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Wes.Core.Api;
using Wes.Core.Attribute;
using Wes.Core.ViewModel;
using Wes.Customer.Mcc.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Controls;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action
{
    public class CollectingAction : ScanActionContextBase<WesFlowID, CollectingAction>, IScanActionContext
    {
        public virtual void RouteMe(Dictionary<string, object> routeParamsMap)
        {
            ScanPackageId(Vm.SelfInfo.pid);
        }

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
            if (!string.IsNullOrEmpty(base.Vm.SelfInfo.pid) && scanVal != base.Vm.SelfInfo.pid)
            {
                try
                {
                    this.CartonEnd(scanVal);
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

            CommonLoadPackageInfo(scanVal);

            if ((bool) base.Vm.SelfInfo.isCompleted)
            {
                WesModernDialog.ShowWesMessage("該票已經採集完成, 請使用RCVEND完成收貨");
                return;
            }

            #region 绑定CLCODE 数据源 

            BindingAutoCompleteData();

            #endregion
        }

        public virtual void ScanBranch(string scanVal)
        {
            if (scanVal.Length > 30)
            {
                ScanQrCode(scanVal);
            }
            else
            {
                ScanSpn(scanVal);
            }
        }

        [IntelliSense]
        public virtual void ScanSpn(string scanVal)
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
            base.Vm.SelfInfo.lot = scanVal;
            dynamic lotResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/lot")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.lot = lotResult.lot;
            if (lotResult.dc != null)
            {
                base.Vm.SelfInfo.dc = lotResult.dc;
                base.Vm.SelfInfo.dt = lotResult.dt;
                base.Vm.SelfInfo.originDc = lotResult.originDc;
            }

            if (lotResult.qty != null)
            {
                base.Vm.SelfInfo.qty = lotResult.qty;
            }
        }

        public virtual void ScanDcNo(string scanVal)
        {
            base.Vm.SelfInfo.dc = scanVal;
            dynamic dcResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/dc")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.dc = dcResult.dc;
            base.Vm.SelfInfo.dt = dcResult.dt;
            base.Vm.SelfInfo.originDc = dcResult.originDc;
        }

        public virtual void ScanQty(string scanVal)
        {
            base.Vm.SelfInfo.strQty = scanVal;

            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/qty")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.qty = qtyResult.qty;
        }

        public virtual void ScanQrCode(string scanVal)
        {
            base.Vm.SelfInfo.qrCode = scanVal;
            dynamic qtyResult = RestApi.NewInstance(Method.POST)
                .AddUri("resolver/qc")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.pn = qtyResult.pn;
            base.Vm.SelfInfo.dc = qtyResult.dc;
            base.Vm.SelfInfo.dt = qtyResult.dt;
            base.Vm.SelfInfo.lot = qtyResult.lot;
            base.Vm.SelfInfo.qty = qtyResult.qty;
            base.Vm.SelfInfo.originDc = qtyResult.originDc;
        }

        public virtual void ScanFlowActionEntryGw(string scanVal)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri("collecting/part-pcs-gw")
                .AddJsonBody("rid", base.Vm.SelfInfo.dimGw.rowID)
                .AddJsonBody("pcsGw", double.Parse(scanVal) / double.Parse(base.Vm.SelfInfo.minQty))
                .Execute()
                .To<object>();
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
        }

        public virtual void ScanMinQty(string scanVal)
        {
            base.Vm.SelfInfo.minQty = scanVal;
            base.Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_GW, GwPropertyName);
        }

        [AbilityAble(true, KPIActionType.LSDataCollectionPlus, "supplier")]
        public virtual bool PalletEnd(string scanVal)
        {
            this.PrivateCartonEnd(scanVal);

            return true;
        }

        public virtual bool RcvEnd(string scanVal)
        {
            PrivateRcvEnd(scanVal);

            return true;
        }

        [AbilityAble(true, KPIActionType.LSDataCollectionPlus, "supplier")]
        public virtual bool CartonEnd(string scanVal)
        {
            this.PrivateCartonEnd(scanVal);

            return true;
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "supplier")]
        public virtual bool Save(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUri("collecting")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute();

            this.CommonLoadPackageInfo(base.Vm.SelfInfo.pid);

            base.Vm.SelfInfo.integralPid = (string) base.Vm.SelfInfo.pid;
            base.Vm.SelfInfo.dimGw = this.GetPartNo((string) base.Vm.SelfInfo.pn);
            if ((double) base.Vm.SelfInfo.dimGw.pcsgw == 0)
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MIN_QTY, MinQtyPropertyName);
            }
            else
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_BRANCH, PnPropertyName);
            }

            return true;
        }

        public virtual void DeleteCartonPart(long rid)
        {
            if (!base.Vm.SelfInfo.isMasterDelete)
                base.Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!base.Vm.SelfInfo.isMasterDelete) return;

            var result =
                WesModernDialog.ShowWesMessage("您确定要删除吗?", "WES_Message".GetLanguage(), MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                RestApi.NewInstance(Method.DELETE)
                    .AddUri("collecting")
                    .AddJsonBody(base.Vm.GetSelfInfo())
                    .AddJsonBody("rid", rid)
                    .Execute();
            }

            CommonLoadPackageInfo(base.Vm.SelfInfo.pid);
        }

        private void PrivateRcvEnd(string scanVal)
        {
            var binWindow = new WesUpdateBinNo($"單號 {base.Vm.SelfInfo.rxt}");
            binWindow.ShowDialog();
            base.Vm.SelfInfo.binNo = binWindow.BinNo;

            RestApi.NewInstance(Method.PUT)
                .AddUri("collecting/rcv-end")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute();

            base.Vm.CleanScanValue();
        }

        private void PrivateCartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri("collecting/carton-end")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute();
            base.Vm.SelfInfo.integralPid = (string) base.Vm.SelfInfo.pid;
            dynamic isComplete = RestApi.NewInstance(Method.GET)
                .AddUri("collecting/is-completed")
                .AddQueryParameter(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            if ((bool) isComplete)
            {
                this.PrivateRcvEnd(scanVal);
            }
            else
            {
                //解决输入carton end 之后后台返回结果后界面无反应的问题
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PID_OR_PN_OR_CLCODE_OR_QRCODE, PnOrQrPropertyName);
            }
        }

        protected virtual dynamic CommonLoadPackageInfo(string scanVal)
        {
            base.Vm.SelfInfo.pid = scanVal;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri("collecting")
                .AddQueryParameter(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.isCompleted = result.isCompleted;
            base.Vm.SelfInfo.rxt = result.rxt;

            base.Vm.SelfInfo.cartons = result.cartons;
            base.Vm.SelfInfo.total = result.total;
            base.Vm.SelfInfo.isMasterReprint = false;
            base.Vm.SelfInfo.isMasterDelete = false;
            return result;
        }

        protected virtual dynamic GetPartNo(string partNo)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri("collecting/part-number")
                .AddQueryParameter(Vm.GetSelfInfo())
                .Execute()
                .To<object>();
            return result;
        }

        protected virtual string PnOrQrPropertyName => "Part No. Or QrCode";
        protected virtual string PnPropertyName => "Part NO.";
        protected virtual string GwPropertyName => "Gw.";
        protected virtual string DcPropertyName => "DC";
        protected virtual string QtyPropertyName => "QTY";
        protected virtual string MinQtyPropertyName => "Min Qty";
        protected virtual string LotPropertyName => "Lot";
    }
}