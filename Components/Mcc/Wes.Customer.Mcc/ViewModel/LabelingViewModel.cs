using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Mcc.Action;
using Wes.Desktop.Windows.Common;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.ViewModel
{
    public class LabelingViewModel : ScanViewModelBase<WesFlowID, LabelingAction>, IViewModel
    {
        public ICommand DeleteReelCommand { get; protected set; }
        public ICommand DeleteBoxCommand { get; protected set; }
        public ICommand ReprintCommand { get; protected set; }
        public static bool IsMoreSupplier { get; set; }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_LOADING_NO;
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_LABELING);
            teamSupport.ShowDialog();
        }

        protected override LabelingAction GetAction()
        {
            var labelAction = ViewModelFactory.CreateActoin<LabelingAction>() as LabelingAction;
            return labelAction;
        }

        protected override string GetActionType(string scanValue)
        {
            //一箱多供应商
            if (DynamicUtil.IsExist(SelfInfo, "_routeParams") && IsMoreSupplier && !SelfInfo._routeParams["routeIsHandled"])
            {
                SelfInfo.supplier = SelfInfo._routeParams["supplier"];
            }
            //一箱一供应商
            else if (DynamicUtil.IsExist(SelfInfo, "_routeParams") && !IsMoreSupplier)
            {
                SelfInfo.supplier = SelfInfo._routeParams["supplier"];
            }
            else if (scanValue.IsPackageID())
            {
                var supplierList = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/shipping/supplier-by-pid")
                    .AddQueryParameter("pid", scanValue)
                    .Execute()
                    .To<List<dynamic>>();
                foreach (var supplier in supplierList)
                {
                    if (supplier.shipper=="C04325")
                    {
                       string supplierCode= RestApi.NewInstance(Method.GET)
                            .SetUrl(RestUrlType.WmsServer, "/receiving/supplier-by-part-no")
                            .AddQueryParameter("partNo", supplier.partNo)
                            .Execute()
                            .To<string>();
                        supplier.shipper = supplierCode;

                    }
                }
                if (supplierList.Count == 1)
                {
                    SelfInfo.supplier = supplierList[0].shipper.ToString();
                    SelfInfo.isFirstSkipLocalSpn = false;
                    //action save之后跳到scanBranch
                    //getActionType一箱一供应商
                    IsMoreSupplier = false;
                }
                else
                {
                    SelfInfo.localSupplierList = supplierList;
                    SelfInfo.isFirstSkipLocalSpn = true;
                    //action save之后跳到scanLocalSpn
                    //getActionType一箱多供应商
                    IsMoreSupplier = true;     
                }
            }

            return SelfInfo.supplier;
        }

        protected override bool VirtualActionEnabled()
        {
            return true;
        }

        protected override void ResetAction(LabelingAction scanAction)
        {
            DeleteReelCommand = new RelayCommand<dynamic>(scanAction.DeleteReelLabel);
            DeleteBoxCommand = new RelayCommand<dynamic>(scanAction.DeleteBoxLabel);
            ReprintCommand = new RelayCommand<long>(scanAction.Reprint);
        }

        public override void ResetUiStatus()
        {
            base.ResetUiStatus();
            SelfInfo.pid = null;
            SelfInfo.qrCode = null;
            SelfInfo.spn = null;
            SelfInfo.dc = null;
            SelfInfo.lot = null;
            SelfInfo.qty = 0;
        }

        protected override WesFlowID ScanInterceptor(string scanVal, WesFlowID scanTarget)
        {
            WesFlowID flowId;
            if (scanVal.IsPackageID())
            {
                flowId = WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID;
            }
            else if (scanVal.IsSxt())
            {
                flowId = WesFlowID.FLOW_ACTION_SCAN_LOADING_NO;
            }
            else
            {
                flowId = scanTarget;
            }

            return flowId;
        }
    }
}