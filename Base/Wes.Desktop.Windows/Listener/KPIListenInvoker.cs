using System;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Desktop.Windows.View;
using Wes.Utilities;

namespace Wes.Desktop.Windows.Listener
{
    /// <summary>
    ///通用仓库作业Action处理
    /// </summary>
    public class KPIListenInvoker : IActionListenInvoker
    {
        public void Invoker(ActionDefinition obj)
        {
            if (obj == null) return;
            LoggingService.DebugFormat("KPI Listen Invoker, KPIType:{0}", obj.KPIType);

            var kpiItems = LoadFlowMudule.Instance.KPICommands;
            if (kpiItems != null)
            {
                foreach (var item in kpiItems)
                {
                    try
                    {
                        IKPICommand command = item.Key;
                        command.Execute(obj);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(ex);
                    }
                }
            }
        }
    }
}
