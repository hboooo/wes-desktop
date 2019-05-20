using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Core;
using Wes.Desktop.Windows.KPI;
using Wes.Flow;

namespace Wes.Customer.Allsor.KPICommand
{
    public class AllsorKpiAction : AmoebaKpiAction
    {
        protected override void AddKpiLSCartonLabeling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.sxt, e.DynaminData.pid, e.DynaminData.labelCount, actionType, e);
        }

        protected override void AddKpiLSCartonLabelingPlus(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.sxt, e.DynaminData.pid, 1, actionType, e);
        }

        protected override void AddKpiLSDataCollection(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.rxt, e.DynaminData.pid, 1, actionType, e);
        }

        protected override void AddKpiLSLabeling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.sxt, e.DynaminData.pid, e.DynaminData.labelCount, actionType, e);
        }
    }
}
