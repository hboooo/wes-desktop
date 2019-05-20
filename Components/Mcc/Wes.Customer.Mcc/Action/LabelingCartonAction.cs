using System.Collections.Generic;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Mcc.Action
{
    public class LabelingCartonAction : ScanActionBase<WesFlowID, LabelingCartonAction>, IScanAction
    {
        public virtual void ScanLoadingNo(string scanVal)
        {
            base.Vm.SelfInfo.sxt = scanVal;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-operation")
                .AddQueryParameter("sxt", scanVal)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.consignee = (string) result.consignee;
            LabelingAction.BindImage(base.Vm, "CARTON");

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        public virtual void ScanPackageId(string scanVal)
        {
            base.Vm.SelfInfo.pid = scanVal;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-carton")
                .AddQueryParameter(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();

            dynamic isNeedGwDim = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/is-need-dim-gw")
                .AddQueryParameter(base.Vm.GetSelfInfo())
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.isNeedGwDim = (bool) isNeedGwDim;
            base.Vm.SelfInfo.overQty = (int) result.overQty;
            base.Vm.SelfInfo.isLabeling = (bool) result.isLabeling;
            base.Vm.SelfInfo.cartonList = result.cartons;
            base.Vm.SelfInfo.reelsList = result.reels;
            base.Vm.SelfInfo.isMasterReprint = false;
            base.Vm.SelfInfo.isMasterDelete = false;

            if (isNeedGwDim)
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_GW);
            }
            else
            {
                this.Save(scanVal);
            }
        }

        public virtual void ScanFlowActionEntryGw(string scanVal)
        {
            base.Vm.SelfInfo.gw = scanVal;
            
            base.Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_DIM);
        }

        public virtual void ScanFlowActionEntryDim(string scanVal)
        {
            base.Vm.SelfInfo.dim = scanVal;
            this.Save(scanVal);
        }

        [AbilityAble(true, KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "consignee")]
        public virtual bool Save(string scanVal)
        {
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/carton-end")
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();

            LabelingAction.PrivatePrint(base.Vm.SelfInfo, labels);
            base.Vm.SelfInfo.labelCount = labels.Count;
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);

            return true;
        }
    }
}