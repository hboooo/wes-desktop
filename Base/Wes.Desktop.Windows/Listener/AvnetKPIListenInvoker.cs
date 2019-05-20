using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Desktop.Windows.View;
using Wes.Utilities;

namespace Wes.Desktop.Windows.Listener
{
    /// <summary>
    /// Avnet仓库作业Action处理
    /// </summary>
    public class AvnetKPIListenInvoker : IActionAvnetListenInvoker
    {
        public void Invoker(ActionDefinition obj)
        {
            if (obj == null) return;
            LoggingService.DebugFormat("Avnet KPI Listen Invoker,AbilityID:{0}", obj.AbilityID);

            var kpiItems = LoadFlowMudule.Instance.KPICommands;
            if (kpiItems != null)
            {
                foreach (var item in kpiItems)
                {
                    IKPICommand command = item.Key;
                    if (command.ActionHandlerId != null && command.ActionHandlerId.Contains(obj.AbilityID))
                    {
                        command.Execute(obj);
                    }
                    else
                    {
                        LoggingService.DebugFormat("Avnet KPI Listen ActionHandlerId UnRegister,AbilityID:{0}", obj.AbilityID);
                    }
                }
            }
        }
    }
}
