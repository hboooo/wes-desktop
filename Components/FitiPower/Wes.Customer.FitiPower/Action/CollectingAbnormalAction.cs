using System;
using System.Collections.Generic;
using Wes.Component.Widgets.Windows;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.FitiPower.API;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Wrapper;

namespace Wes.Customer.FitiPower.Action
{
    public class CollectingAbnormalAction : ScanActionBase<WesFlowID, CollectingAbnormalAction>, IScanAction
    {
        [Ability(6440013406173929472, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanReceivingNo(string scanVal)
        {
            GetAbnormalInfos(scanVal);
        }

        public void SearchClickCommand()
        {
            if (string.IsNullOrEmpty(this.Vm.ScanValue))
            {
                WesModernDialog.ShowWesMessage("Receiving No 不能为空");
                return;
            }
            GetAbnormalInfos(this.Vm.ScanValue);
        }

        public void GetAbnormalInfos(string scanVal)
        {
            if (String.IsNullOrWhiteSpace(scanVal))
            {
                WesModernDialog.ShowWesMessage("Receiving No 不能为空");
                return;
            }
            dynamic abnormalInfos = RestApi.NewInstance(Method.GET)
                        .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.COLLECTING_EXCEPTION, false)
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
                       .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.COLLECTING_EXCEPTION, false)
                       .AddQueryParameter("rcv", this.Vm.SelfInfo.rcv)
                       .AddQueryParameter("pn", obj.partNo)
                       .Execute()
                       .To<object>();

            PartNoDetailsWindow detailsWindow = new PartNoDetailsWindow(details);
            detailsWindow.ShowDialog();
        }
    }
}
