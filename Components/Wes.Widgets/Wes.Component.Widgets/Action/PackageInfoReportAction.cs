using System.Data;
using Wes.Component.Widgets.Model;
using Wes.Component.Widgets.ViewModel;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Converters;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Component.Widgets.Action
{
    /// <summary>
    /// 箱号信息查询：储位移动和进出仓记录
    /// </summary>
    public class PackageInfoReportAction : ScanActionBase<WesFlowID, PackageInfoReportAction>, IScanAction
    {
        private const long SearchPackageInfoScriptId = 6439771886913064960;


        [Ability(6439772969144164352, AbilityType.ScanAction)]
        public virtual void SearchData()
        {
            var packageInfoReportView = Vm as PackageInfoReportViewModel;
            string searchPid = packageInfoReportView.SearchPid;
            string searchPn = packageInfoReportView.SearchPN;
            if (string.IsNullOrWhiteSpace(searchPid))
            {
                WesModernDialog.ShowWesMessage("請先輸入完整箱號！");
                return;
            }

            DataSet result = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(SearchPackageInfoScriptId)
                .AddQueryParameter("pid", searchPid)
                .AddQueryParameter("pn", searchPn)
                .Execute()
                .To<DataSet>();
            if (result != null && result.Tables.Count == 2)
            {
                Vm.SelfInfo.binRemoveList = result.Tables[0].ToList<BinNoInfoModel>();
                Vm.SelfInfo.inOutList = result.Tables[1].ToList<PackageInfoModel>();
            }
        }
    }
}
