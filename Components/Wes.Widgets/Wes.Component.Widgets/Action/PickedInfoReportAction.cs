using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Flow;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Component.Widgets.Action
{
    /// <summary>
    /// 已捡货信息查询
    /// </summary>
    public class PickedInfoReportAction : ScanActionBase<WesFlowID, PickedInfoReportAction>, IScanAction
    {
        [Ability(6480617903103152128, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPickingNo(string scanValue)
        {
            if (!scanValue.IsPxt())
            {
                throw new WesException("只允許輸入PXT查詢");
            }

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddCommonUri(RestUrlType.WmsServer, "pick-report/get-pxt-info")
                .AddQueryParameter("pxt",scanValue)
                .Execute()
                .To<object>();
            this.Vm.SelfInfo.PickedInfo = result;
        }
    }
}
