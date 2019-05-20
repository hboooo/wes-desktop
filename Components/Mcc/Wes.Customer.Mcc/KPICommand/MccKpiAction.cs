using Wes.Core;
using Wes.Desktop.Windows.KPI;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Customer.Mcc.KPICommand
{
    public class MccKpiAction : AmoebaKpiAction
    {
        protected override void AddKpiLSCartonLabeling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.sxt, e.DynaminData.pid, e.DynaminData.labelCount, actionType, e);
        }

        protected override void AddKpiLSCartonLabelingPlus(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.sxt, e.DynaminData.pid, e.DynaminData.labelCount, actionType, e);
        }

        protected override void AddKpiLSDataCollection(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.rxt, e.DynaminData.pid, e.DynaminData.labelCount, actionType, e);
        }

        protected override void AddKpiLSLabeling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.sxt, e.DynaminData.pid, e.DynaminData.labelCount, actionType, e);
        }

        protected override void AddKpiLSCombineCarton(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            if (e.DynaminData.kpiPackages != null)
            {
                var packages = DynamicJson.DeserializeObject<object>(DynamicJson.SerializeObject(e.DynaminData.kpiPackages));
                foreach (var item in packages)
                {
                    amoebaProductivity.AddTeamKPI(e.DynaminData.truckOrder, item.packageID, e.DynaminData.labelCount, actionType, e);
                }
            }
        }
    }
}
