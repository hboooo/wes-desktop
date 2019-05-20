using System.Collections.Generic;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.Promaster.Action
{
    /// <summary>
    /// 标签纸箱入口类
    /// </summary>
    public class LabelingCartonAction : ScanActionBase<WesFlowID, LabelingCartonAction>, IScanAction
    {
        private static readonly string GwPropertyName = "GW";

        /// <summary>
        /// 扫描  LoadingNo
        /// </summary>
        /// <param name="scanVal"></param>
        public virtual void ScanLoadingNo(string scanVal)
        {
            Vm.SelfInfo.sxt = scanVal;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-operation")
                .AddQueryParameter("sxt", scanVal)
                .Execute()
                .To<object>();

            Vm.SelfInfo.consignee = (string) result.consignee;
            LabelingAction.BindImage(Vm, "CARTON");

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        /// <summary>
        /// 扫描 PackageId
        /// </summary>
        /// <param name="scanVal"></param>
        public virtual void ScanPackageId(string scanVal)
        {
            Vm.SelfInfo.pid = scanVal;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/load-carton")
                .AddQueryParameter(Vm.GetSelfInfo())
                .Execute()
                .To<object>();

            dynamic isNeedGwDim = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "labeling/is-need-dim-gw")
                .AddQueryParameter(Vm.GetSelfInfo())
                .Execute()
                .To<object>();

            Vm.SelfInfo.isNeedGwDim = (bool) isNeedGwDim;
            Vm.SelfInfo.overQty = (int) result.overQty;
            Vm.SelfInfo.isLabeling = (bool) result.isLabeling;
            Vm.SelfInfo.cartonList = result.cartons;
            Vm.SelfInfo.reelsList = result.reels;
            Vm.SelfInfo.isMasterReprint = false;
            Vm.SelfInfo.isMasterDelete = false;

            if (isNeedGwDim)
            {
                Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_GW, GwPropertyName);
            }
            else
            {
                Save(scanVal);
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="scanVal"></param>
        [AbilityAble(true, KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "consignee")]
        protected virtual void Save(string scanVal)
        {
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "labeling/carton-end")
                .AddJsonBody(Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            LabelingAction.PrivatePrint(Vm.SelfInfo, labels);
            Vm.SelfInfo.labelCount = labels.Count;
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        /// <summary>
        /// 扫描FlowActionEntryDim
        /// </summary>
        /// <param name="scanVal"></param>
        public virtual void ScanFlowActionEntryDim(string scanVal)
        {
            Vm.SelfInfo.dim = scanVal;
            Save(scanVal);
        }

        /// <summary>
        /// 扫描 FlowActionEntryGw
        /// </summary>
        /// <param name="scanVal"></param>
        public virtual void ScanFlowActionEntryGw(string scanVal)
        {
            Vm.SelfInfo.gw = scanVal;
            Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_DIM);
        }
    }
}