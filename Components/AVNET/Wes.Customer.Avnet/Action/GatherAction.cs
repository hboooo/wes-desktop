using System;
using System.Windows;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Common;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Avnet.Action
{
    /**
     * 采集动作集合
     */
    public class GatherAction : ScanActionBase<WesFlowID, GatherAction>, IScanAction
    {
        public const long GatherDeleteDataScriptId = 6402348645173170180;
        public const long GatherInsertDataScriptId = 6402348645173174275;
        public const long GatherPackageInfoScriptId = 6402348645173174274;

        [Ability(6402348645168979969, AbilityType.ScanAction)]
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

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
            base.Vm.SelfInfo.Target = (String) result.shipper;
            base.SetActionValid();
        }

        [Ability(6402348645168975874, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanMpn(string scanVal)
        {
            Vm.SelfInfo.spn = scanVal;
            var result = RestApi.NewInstance(Method.POST)
                .SetWmsCustomerUri("/resolver/spn")
                .AddParams(Vm.GetSelfInfo())
                .Execute()
                .To<dynamic>();
            Vm.SelfInfo.apn = result.partNo;
            Vm.SelfInfo.mpn = result.spn;
            Vm.SelfInfo.mpnOrigin = result.spn;

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QRCODE);
        }

        [Ability(6402348645168979970, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanQrcode(string scanVal)
        {
            Vm.SelfInfo.qrCode = scanVal;
            var qcResult = RestApi.NewInstance(Method.POST)
                .SetWmsCustomerUri("/resolver/qc")
                .AddParams(Vm.GetSelfInfo())
                .Execute()
                .To<dynamic>();
            Vm.SelfInfo.lot = qcResult.lot;
            Vm.SelfInfo.opn = qcResult.opn;
            Vm.SelfInfo.dc = qcResult.dc;
            Vm.SelfInfo.qty = (int) qcResult.qty;
            Vm.SelfInfo.dt = qcResult.dt;

            PrivateSave();

            try
            {
                this.LoadPackageInfo(Vm.SelfInfo.pid);
            }
            catch (WesRestException ex)
            {
                if (ex.MessageCode != 6438394657708707840)
                {
                    throw ex;
                }
            }

            Vm.SelfInfo.integralTotal = 1;
            Vm.SelfInfo.integralIsPlus = false;
            Vm.SelfInfo.integralPid = (string) this.Vm.SelfInfo.pid;
            SetActionValid();

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        [Ability(6433216211709861888, AbilityType.ScanAction)]
        public virtual void PalletEnd(string scanVal)
        {
            this.CartonEnd(scanVal);
        }

        [Ability(6437959472475283456, AbilityType.ScanAction)]
        public virtual void RcvEnd(string scanVal)
        {
            PrivateRcvEnd();
        }

        [Ability(6438317547052863488, AbilityType.ScanAction)]
        public virtual void CartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, 6438317941883674624, true)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .AddJsonBody("rcv", base.Vm.SelfInfo.rcv)
                .Execute();

            dynamic isComplete = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, 6435375914481094656L, true)
                .AddJsonBody("rcv", (string)this.Vm.SelfInfo.rcv)
                .Execute()
                .To<object>();

            if ((bool)isComplete)
            {
                this.PrivateRcvEnd();
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            }

            base.Vm.SelfInfo.integralTotal = 1;
            base.Vm.SelfInfo.integralIsPlus = true;
            base.Vm.SelfInfo.integralPid = (string) this.Vm.SelfInfo.pid;
            base.SetActionValid();
        }

        [Ability(6402348645173170178, AbilityType.ScanAction)]
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
                    .AddUriParam(RestUrlType.WmsServer, GatherDeleteDataScriptId, true)
                    .AddJsonBody("rid", rid)
                    .Execute();
            }

            LoadPackageInfo(this.Vm.SelfInfo.pid);
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
            WesUpdateBinNo binNo = new WesUpdateBinNo(binNoModel);
            binNo.ShowDialog();
            base.Vm.CleanScanValue();
        }

        private dynamic PrivateSave()
        {
            dynamic saveResult = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, GatherInsertDataScriptId, true)
                .AddJsonBody("rcv", (string) this.Vm.SelfInfo.rcv)
                .AddJsonBody("pid", (string) this.Vm.SelfInfo.pid)
                .AddJsonBody("apn", (string) this.Vm.SelfInfo.apn)
                .AddJsonBody("lot", (string) this.Vm.SelfInfo.lot)
                .AddJsonBody("dc", (string) this.Vm.SelfInfo.dc)
                .AddJsonBody("qty", (int) this.Vm.SelfInfo.qty)
                .AddJsonBody("mpn", (string) this.Vm.SelfInfo.mpn)
                .AddJsonBody("dt", (string) this.Vm.SelfInfo.dt)
                .AddJsonBody("opn", (string) this.Vm.SelfInfo.opn)
                .AddJsonBody("dim", (string) this.Vm.SelfInfo.dim)
                .AddJsonBody("gw", (string) this.Vm.SelfInfo.gw)
                .Execute()
                .To<object>();

            return saveResult;
        }

        private dynamic LoadPackageInfo(string pid)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, GatherPackageInfoScriptId, false)
                .AddQueryParameter("pid", pid)
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.pid = (string) pid;
            base.Vm.SelfInfo.rcv = (string) result.receivingNo;
            base.Vm.SelfInfo.total = (int) result.cartonSum;
            base.Vm.SelfInfo.palletId = (string) result.palletId;
            base.Vm.SelfInfo.minPid = (string) result.minPid;
            base.Vm.SelfInfo.cartons = result.cartons;
            base.Vm.SelfInfo.isMasterDelete = false;
            return result;
        }
    }
}