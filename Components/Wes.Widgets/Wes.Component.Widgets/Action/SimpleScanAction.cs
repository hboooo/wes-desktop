using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Flow;

namespace Wes.Component.Widgets.Action
{
    public abstract class SimpleScanAction<T, A> : ScanActionBase<T, A>
        where T : struct
        where A : IScanAction
    {
        public override void BeginScan(string scanValue)
        {
            if (Convert.ToInt64(WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO) == Convert.ToInt64(this.Vm.CurrentScanTarget))
            {
                this.Vm.SelfInfo.rxt = scanValue;
            }
            else if (Convert.ToInt64(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID) ==
                     Convert.ToInt64(this.Vm.CurrentScanTarget))
            {
                this.Vm.SelfInfo.pid = scanValue;
            }
        }
    }
}