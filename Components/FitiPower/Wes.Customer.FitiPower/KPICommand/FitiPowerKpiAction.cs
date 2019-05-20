using Wes.Core;
using Wes.Desktop.Windows.KPI;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Customer.FitiPower.KPICommand
{
    public class FitiPowerKpiAction : AmoebaKpiAction
    {
        protected override void AddKpiLSLabeling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.sxt, e.DynaminData.pid, e.DynaminData.labelCount, actionType, e);
        }
        protected override void AddKpiLSCombinePallet(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.loadingNo, e.DynaminData.palletNo, e.DynaminData.kpiPackages.Count, actionType, e);
        }

        protected override void AddKpiLSCombinePalletPlus(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.loadingNo, e.DynaminData.palletNo, 1, actionType, e);
        }

        protected override void AddKpiLSCombineCarton(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            if (e.DynaminData.kpiPackages != null)
            {
                var packages = DynamicJson.DeserializeObject<object>(DynamicJson.SerializeObject(e.DynaminData.kpiPackages));
                foreach (var item in packages)
                {
                    amoebaProductivity.AddTeamKPI(e.DynaminData.loadingNo, item.PackageID, 1, actionType, e);
                }
            }
        }

        protected override void DeleteKpiLSCombinePallet(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.DeleteKPI(e.DynaminData.loadingNo, e.DynaminData.palletNo, actionType, e.DynaminData.labelCount, e.DynaminData.palletPackageCount, e);
        }

        protected override void DeleteKpiLSCombinePalletPlus(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.DeleteKPI(e.DynaminData.loadingNo, e.DynaminData.palletNo, actionType, 10, 1, e);
        }

        protected override void DeleteKpiLSCombineCarton(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            if (e.DynaminData.deleteKpiPackages != null)
            {
                var packages = DynamicJson.DeserializeObject<object>(DynamicJson.SerializeObject(e.DynaminData.deleteKpiPackages));
                foreach (var item in packages)
                {
                    if (item.CPkgID == null ||
                        string.IsNullOrWhiteSpace(item.CPkgID.ToString()))
                    {
                        amoebaProductivity.DeleteKPI(e.DynaminData.loadingNo, item.PackageID, actionType, 1, 1, e);
                    }
                }
            }
        }
    }
}
