using System;
using Wes.Core;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Desktop.Windows.KPI
{
    public class AmoebaKpiAction
    {
        public Func<AmoebaKpiDataAdapter> CreateDataAdapter;

        public virtual void TeamKpiHandler(ActionDefinition e)
        {
            KPIActionType actionType = e.KPIType;
            AmoebaKpiDataAdapter amoebaProductivity = null;

            foreach (var item in Enum.GetValues(typeof(KPIActionType)))
            {
                if (IsMatch(actionType, item))
                {
                    KPIActionType kpiType = (KPIActionType)item;
                    amoebaProductivity = CreateDataAdapter();
                    amoebaProductivity.Initialize(e, kpiType);
                    if (e.IsDeleteKPI)
                        DeleteKpi(e, amoebaProductivity, kpiType);
                    else
                        AddKpi(e, amoebaProductivity, kpiType);
                }
            }
        }

        protected virtual bool IsMatch(KPIActionType actionType, object item)
        {
            ulong type = (ulong)actionType;
            ulong action = (ulong)item;
            KPIActionType kpiType = (KPIActionType)action;

            if (kpiType == KPIActionType.Null || kpiType == KPIActionType.Shipping || kpiType == KPIActionType.Receiving)
                return false;
            if ((type & (ulong)KPIActionType.Receiving) != (action & (ulong)KPIActionType.Receiving))
                return false;
            if ((type & action) != action)
                return false;
            return true;
        }

        private void AddKpi(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            LoggingService.InfoFormat("Add KPI [{0}]", actionType);
            switch (actionType)
            {
                case KPIActionType.Null:
                    break;
                //採集
                case KPIActionType.LSReceivingLabelling:
                case KPIActionType.LSReceivingLabellingPlus:
                    this.AddKpiLSReceivingLabelling(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSDataCollection:
                case KPIActionType.LSDataCollectionPlus:
                    this.AddKpiLSDataCollection(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSPickingSplitCarton:
                case KPIActionType.LSPickingSplitCartonPlus:
                case KPIActionType.LSPacking:
                case KPIActionType.LSPackingPlus:
                    this.AddKpiLSPicking(e, amoebaProductivity, actionType);
                    break;
                //帖內標
                case KPIActionType.LSChecking:
                case KPIActionType.LSLabeling:
                case KPIActionType.LSLabelingPlus:
                    this.AddKpiLSLabeling(e, amoebaProductivity, actionType);
                    break;
                //帖外標
                case KPIActionType.LSCartonLabeling:
                    this.AddKpiLSCartonLabeling(e, amoebaProductivity, actionType);
                    break;
                //帖外標
                case KPIActionType.LSCartonLabelingPlus:
                    this.AddKpiLSCartonLabelingPlus(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSCombinePallet:
                    this.AddKpiLSCombinePallet(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSCombinePalletPlus:
                    this.AddKpiLSCombinePalletPlus(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSCombineCarton:
                case KPIActionType.LSCombineCartonPlus:
                    this.AddKpiLSCombineCarton(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.PDAAirLabel:
                    this.AddKpiPDAAirLabel(e, amoebaProductivity, actionType);
                    break;
                default:
                    break;
            }
        }

        private void DeleteKpi(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            LoggingService.InfoFormat("Delete KPI [{0}]", actionType);
            switch (actionType)
            {
                case KPIActionType.Null:
                    break;
                case KPIActionType.LSReceivingLabelling:
                    this.DeleteKpiLSReceivingLabelling(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSDataCollection:
                    break;
                case KPIActionType.LSDataCollectionPlus:
                    break;
                case KPIActionType.LSPickingSplitCarton:
                    break;
                case KPIActionType.LSPickingSplitCartonPlus:
                    break;
                case KPIActionType.LSPacking:
                case KPIActionType.LSPackingPlus:
                    this.DeleteKpiLSPacking(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSLabeling:
                    break;
                case KPIActionType.LSLabelingPlus:
                    break;
                case KPIActionType.LSChecking:
                    break;
                case KPIActionType.LSCartonLabeling:
                    break;
                case KPIActionType.LSCartonLabelingPlus:
                    break;
                case KPIActionType.LSCombinePallet:
                    this.DeleteKpiLSCombinePallet(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSCombinePalletPlus:
                    this.DeleteKpiLSCombinePalletPlus(e, amoebaProductivity, actionType);
                    break;
                case KPIActionType.LSCombineCarton:
                case KPIActionType.LSCombineCartonPlus:
                    this.DeleteKpiLSCombineCarton(e, amoebaProductivity, actionType);
                    break;
                default:
                    break;
            }
        }


        #region AddKPI By Type
        protected virtual void AddKpiLSReceivingLabelling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.ReceivingNo, e.DynaminData.PalletId, e.DynaminData.LabelCount, actionType, e);
        }

        protected virtual void AddKpiLSDataCollection(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.rcv, e.DynaminData.integralPid, 1, actionType, e);
        }

        protected virtual void AddKpiLSPicking(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.Pxt, e.DynaminData.Pid, e.DynaminData.Qty, actionType, e);
        }

        protected virtual void AddKpiLSLabeling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.sxt, e.DynaminData.pid, e.DynaminData.labelCount, actionType, e);
        }

        protected virtual void AddKpiLSCartonLabeling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.LoadingNo, e.DynaminData.PackageId, e.DynaminData.LabelCount, actionType, e);
        }

        protected virtual void AddKpiLSCartonLabelingPlus(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.LoadingNo, e.DynaminData.PackageId, 1, actionType, e);
        }

        protected virtual void AddKpiLSCombinePallet(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.truckOrder, e.DynaminData.palletNo, e.DynaminData.kpiPackages.Count, actionType, e);
        }

        protected virtual void AddKpiLSCombinePalletPlus(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.truckOrder, e.DynaminData.palletNo, 1, actionType, e);
        }

        protected virtual void AddKpiLSCombineCarton(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            if (e.DynaminData.kpiPackages != null)
            {
                var packages = DynamicJson.DeserializeObject<object>(DynamicJson.SerializeObject(e.DynaminData.kpiPackages));
                foreach (var item in packages)
                {
                    amoebaProductivity.AddTeamKPI(e.DynaminData.truckOrder, item.packageID, 1, actionType, e);
                }
            }
        }

        protected virtual void AddKpiPDAAirLabel(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.AddTeamKPI(e.DynaminData.txt, e.DynaminData.palletId, e.DynaminData.labelCount, actionType, e);
        }
        #endregion

        #region DeleteKPI By Type
        protected virtual void DeleteKpiLSReceivingLabelling(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.DeleteKPI(e.DynaminData.ReceivingNo, e.DynaminData.PalletId, actionType, 1, e.DynaminData.LabelCount, e);
        }

        protected virtual void DeleteKpiLSPacking(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.DeleteKPI(e.DynaminData.Pxt, e.DynaminData.Pid, actionType, 1, 1, e);
        }

        protected virtual void DeleteKpiLSCombinePallet(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.DeleteKPI(e.DynaminData.truckOrder, e.DynaminData.palletNo, actionType, 1, e.DynaminData.palletPackageCount, e);
        }

        protected virtual void DeleteKpiLSCombinePalletPlus(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            amoebaProductivity.DeleteKPI(e.DynaminData.truckOrder, e.DynaminData.palletNo, actionType, 10, 1, e);
        }

        protected virtual void DeleteKpiLSCombineCarton(ActionDefinition e, AmoebaKpiDataAdapter amoebaProductivity, KPIActionType actionType)
        {
            if (e.DynaminData.deleteKpiPackages != null)
            {
                var packages = DynamicJson.DeserializeObject<object>(DynamicJson.SerializeObject(e.DynaminData.deleteKpiPackages));
                foreach (var item in packages)
                {
                    if (item.cPkgID == null || string.IsNullOrWhiteSpace(item.cPkgID.ToString()))
                    {
                        amoebaProductivity.DeleteKPI(e.DynaminData.truckOrder, item.packageID, actionType, 1, 1, e);
                    }
                }
            }
        }
        #endregion
    }
}