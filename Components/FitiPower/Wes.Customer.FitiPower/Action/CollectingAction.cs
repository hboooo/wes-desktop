using System;
using System.Linq;
using System.Windows;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.FitiPower.API;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.FitiPower.Action
{
    /**
     * 采集动作集合
     */
    public class CollectingAction : ScanActionBase<WesFlowID, CollectingAction>, IScanAction
    {
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

            LoadPackageInfo(scanVal);

            if ((string) base.Vm.SelfInfo.shipper == "C03707" ||
                (string) base.Vm.SelfInfo.shipper == "C02191" ||
                (string) base.Vm.SelfInfo.shipper == "C03697" ||
                (string) base.Vm.SelfInfo.shipper == "C02195" ||
                (string) base.Vm.SelfInfo.shipper == "C03698" ||
                (string) base.Vm.SelfInfo.shipper == "C03699" ||
                (string) base.Vm.SelfInfo.shipper == "C04433"
            )
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
            }
            else if ((string) base.Vm.SelfInfo.shipper == "C04351"
                     || (string) base.Vm.SelfInfo.shipper == "C03700"
                     || (string) base.Vm.SelfInfo.shipper == "C03701"
                     || (string) base.Vm.SelfInfo.shipper == "C03702"
                     || (string) base.Vm.SelfInfo.shipper == "C03703"
                     || (string) base.Vm.SelfInfo.shipper == "C03704"
                     || (string) base.Vm.SelfInfo.shipper == "C03861"
                     || (string) base.Vm.SelfInfo.shipper == "C04434"
                     || (string) base.Vm.SelfInfo.shipper == "C04432"
            )
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QRCODE);
            }
            else
            {
                throw new WesException($"暂不支持的此供应商{Vm.SelfInfo.shipper}");
            }
        }

        public virtual void ScanFlowActionScanMpn(string scanVal)
        {
            //resolver mpn
            string mpn = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DECODE_DISPATCH)
                .AddBranch("MPN")
                .AddQueryParameter("mpn", scanVal)
                .AddQueryParameter("rxt", base.Vm.SelfInfo.rcv)
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .Execute()
                .To<string>();

            //check mpn
            dynamic rules = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.PART_RULES)
                .AddQueryParameter("mpn", mpn)
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.mpn = rules.mpn;
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
        }

        public virtual void ScanFlowActionScanLotNo(string scanVal)
        {
            //decode lot
            dynamic lot = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DECODE_DISPATCH)
                .AddBranch("LOT")
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("mpn", base.Vm.SelfInfo.mpn)
                .AddQueryParameter("qty", base.Vm.SelfInfo.qty)
                .AddQueryParameter("dc", base.Vm.SelfInfo.dc)
                .AddQueryParameter("lot", scanVal)
                .Execute()
                .To<string>();

            base.Vm.SelfInfo.lot = (string) lot;
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY);
        }

        public virtual void ScanFlowActionScanQty(string scanVal)
        {
            //decode qty
            dynamic qty = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DECODE_DISPATCH)
                .AddBranch("QTY")
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("mpn", base.Vm.SelfInfo.mpn)
                .AddQueryParameter("qty", scanVal)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.qty = (int) qty;

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO);
        }

        [AbilityAble(true, KPIActionType.LSDataCollection, "shipper")]
        public virtual bool ScanFlowActionScanDcNo(string scanVal)
        {
            //decode dc
            dynamic dc = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DECODE_DISPATCH)
                .AddBranch("DC")
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("mpn", base.Vm.SelfInfo.mpn)
                .AddQueryParameter("qty", base.Vm.SelfInfo.qty)
                .AddQueryParameter("dc", scanVal)
                .Execute()
                .To<string>();

            //resolver dc
            dynamic dt = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DATE_CODE_DISPATCH)
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("dc", dc)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.dc = (string) dc;
            base.Vm.SelfInfo.dt = (string) dt;

            this.PrivateSave(scanVal);

            return true;
        }

        public virtual void ScanFlowActionScanQrcode(string scanVal)
        {
            //解析QrCode
            dynamic qrCode = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.QR_CODE_DISPATCH)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .AddJsonBody("qrCode", scanVal)
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.cpo = qrCode.cpo;
            base.Vm.SelfInfo.cpn = qrCode.cpn;
            base.Vm.SelfInfo.mpn = qrCode.mpn;
            base.Vm.SelfInfo.isQrcodeQty = true;

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        public virtual void PalletEnd(string scanVal)
        {
            this.CartonEnd(scanVal);
        }

        public virtual void RcvEnd(string scanVal)
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

            WesUpdateBinNo binNoWindow = new WesUpdateBinNo(binNoModel, BinAction.Add);
            binNoWindow.ShowDialog();

            dynamic result = RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "/1/{si}")
                .AddScriptId(ScriptID.UPDATE_BIN_NO)
                .AddJsonBody("pxt", binNoModel.OperationNo)
                .AddJsonBody("actionType", binNoModel.ActionType)
                .AddJsonBody("pid", binNoModel.PackageId)
                .AddJsonBody("binNo", binNoWindow.BinNo)
                .AddJsonBody("user", binNoModel.UpdateUser)
                .Execute()
                .To<object>();

            base.Vm.CleanScanValue();
        }

        [AbilityAble(true, KPIActionType.LSDataCollectionPlus, "shipper")]
        public virtual bool CartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.COLLECTING_CARTON_END, true)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .AddJsonBody("rcv", base.Vm.SelfInfo.rcv)
                .Execute();

            dynamic isComplete = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.COLLECTING_IS_COMPLETE, true)
                .AddJsonBody("rcv", (string) this.Vm.SelfInfo.rcv)
                .Execute()
                .To<object>();

            if ((bool) isComplete)
            {
                this.RcvEnd(scanVal);
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            }

            base.Vm.SelfInfo.integralPid = (string) this.Vm.SelfInfo.pid;

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
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.COLLECTING_DELETE, true)
                    .AddJsonBody("rid", rid)
                    .Execute();
            }

            LoadPackageInfo(this.Vm.SelfInfo.pid);
        }

        private void PrivateSave(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.COLLECTING_SAVE, true)
                .AddJsonBody("rcv", (string) this.Vm.SelfInfo.rcv)
                .AddJsonBody("pid", (string) this.Vm.SelfInfo.pid)
                .AddJsonBody("lot", (string) this.Vm.SelfInfo.lot)
                .AddJsonBody("dc", (string) this.Vm.SelfInfo.dc)
                .AddJsonBody("qty", (int) this.Vm.SelfInfo.qty)
                .AddJsonBody("mpn", (string) this.Vm.SelfInfo.mpn)
                .AddJsonBody("dt", (string) this.Vm.SelfInfo.dt)
                .AddJsonBody("cpo",
                    (string) (this.Vm.SelfInfo.cpo == DependencyProperty.UnsetValue ? null : this.Vm.SelfInfo.cpo))
                .AddJsonBody("cpn",
                    (string) (this.Vm.SelfInfo.cpn == DependencyProperty.UnsetValue ? null : this.Vm.SelfInfo.cpn))
                .Execute();

            try
            {
                this.LoadPackageInfo(this.Vm.SelfInfo.pid);
            }
            catch (WesRestException ex)
            {
                //保存完畢查詢箱不提示異常
                if (ex.MessageCode != 6438394657708707840)
                {
                    throw ex;
                }
            }

            base.Vm.SelfInfo.integralPid = (string) this.Vm.SelfInfo.pid;
            var isExists = (new String[] {"C03700", "C03701", "C03702", "C03703", "C03704", "C03861"}).Count(m =>
                m.ToUpper().Equals(base.Vm.SelfInfo.shipper.ToString().ToUpper()));
            base.Vm.Next(isExists > 0 ? WesFlowID.FLOW_ACTION_SCAN_QRCODE : WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        private dynamic LoadPackageInfo(string pid)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.COLLECTING_PACKAGE, false)
                .AddQueryParameter("pid", pid)
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