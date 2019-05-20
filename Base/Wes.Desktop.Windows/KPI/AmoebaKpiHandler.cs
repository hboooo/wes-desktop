using System;
using Wes.Core;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Desktop.Windows.KPI
{
    public class AmoebaKpiHandler
    {
        protected virtual void ExecuteKpi(object obj)
        {
            try
            {
                if (obj == null) return;
                ActionDefinition actionDefinition = obj as ActionDefinition;
                if (actionDefinition == null) return;
                AmoebaKpiAction amoebaKpiAction = CreateAmoebaKpiAction();
                amoebaKpiAction.CreateDataAdapter = CreateAmoebaKpiDataAdapter;
                amoebaKpiAction.TeamKpiHandler(actionDefinition);
            }
            catch (Exception ex)
            {
                LoggingService.Error(new WesException(obj, ex));
            }
        }

        protected virtual AmoebaKpiAction CreateAmoebaKpiAction()
        {
            return new AmoebaKpiAction();
        }

        protected virtual AmoebaKpiDataAdapter CreateAmoebaKpiDataAdapter()
        {
            return new AmoebaKpiDataAdapter();
        }
    }
}
