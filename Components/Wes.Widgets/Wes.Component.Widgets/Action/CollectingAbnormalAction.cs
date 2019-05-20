using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wes.Component.Widgets.Windows;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Component.Widgets.Action
{
    public class CollectingAbnormalAction : ScanActionBase<WesFlowID, CollectingAbnormalAction>, IScanAction
    {
        [Ability(6440013406173929472, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanReceivingNo(string scanVal)
        {
            scanVal = scanVal.ToUpper();

            //支持掃箱號,轉換為RXT查找異常
            Regex regexPC = new Regex(@"^X\d{10}P\d{3}-C\d{3}$", RegexOptions.IgnoreCase);
            Regex regexC = new Regex(@"^X\d{10}C\d{3}$", RegexOptions.IgnoreCase);
            if (regexPC.IsMatch(scanVal) || regexC.IsMatch(scanVal))
            {
                scanVal = "RXT" + scanVal.Substring(1, 10);
            }
            if (!StringExtensions.IsRxt(scanVal))
            {
                WesModernDialog.ShowWesMessage("Receiving No 無效");
                return;
            }


            GetAbnormalInfos(scanVal);
        }

        public void SearchCommand()
        {
            GetAbnormalInfos(this.Vm.SelfInfo.rcv);
        }

        public void GetAbnormalInfos(string scanVal)
        {
            dynamic abnormalInfos = RestApi.NewInstance(Method.GET)
                        .AddUriParam(RestUrlType.WmsServer, 6442988160094834688, false)
                        .AddQueryParameter("rcv", scanVal)
                        .Execute()
                        .To<object>();
            this.Vm.SelfInfo.rcv = scanVal;
            if (abnormalInfos != null)
            {
                this.Vm.SelfInfo.doneList = abnormalInfos.partReport;
                this.Vm.SelfInfo.undoList = abnormalInfos.notCollectReports;
            }
            else
            {
                this.Vm.SelfInfo.doneList = new List<object>();
                this.Vm.SelfInfo.undoList = new List<object>();
            }
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO);
        }

        public void GetGetherPortNoDetails(dynamic obj)
        {
            if (obj == null) return;

            dynamic details = RestApi.NewInstance(Method.GET)
                       .AddBranch("QUERY_BY_PART_NO")
                       .AddUriParam(RestUrlType.WmsServer, 6442988160094834688, false)
                       .AddQueryParameter("rcv", this.Vm.SelfInfo.rcv)
                       .AddQueryParameter("pn", obj.partNo)
                       .Execute()
                       .To<object>();

            PartNoDetailsWindow detailsWindow = new PartNoDetailsWindow(details);
            detailsWindow.ShowDialog();
        }
    }
}
