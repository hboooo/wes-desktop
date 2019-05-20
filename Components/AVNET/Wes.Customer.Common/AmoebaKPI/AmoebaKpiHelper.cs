using System;
using Wes.Core;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Customer.Common
{
    public class AmoebaKpiHelper
    {
        public static void AddTeamKpi(ActionDefinition e)
        {
            try
            {
                string name = e.FlowName;
                WesFlowID flowId = WesFlow.Instance.GetFlow(name);
                AddAvnetKpi(e, flowId);
            }
            catch (Exception ex)
            {
                LoggingService.Error(new WesException(e.DynaminData, ex));
            }
        }

        private static void AddAvnetKpi(ActionDefinition e, WesFlowID flowId)
        {
            int flowMask = WesFlow.Instance.GetFlowMask((int)flowId);
            AmoebaProductivity.InitializeOperationType(flowMask);
            AmoebaProductivity.InitializeUser(e.TypeName, e.MethodName);
            AmoebaProductivity.InitializeKpiConfig(e.DynaminData.Target.ToString());

            switch (flowId)
            {
                case WesFlowID.FLOW_OUT_SOW:
                case WesFlowID.FLOW_OUT_PICKING:
                case WesFlowID.FLOW_OUT_PICKING_AND_SOW:
                    AmoebaProductivity.AddTeamKPI(e.DynaminData.Pxt, e.DynaminData.Pid, e.DynaminData.Qty, false, e);
                    if (e.DynaminData.NeedAddPlus == true)
                    {
                        AmoebaProductivity.AddTeamKPI(e.DynaminData.Pxt, e.DynaminData.Pid, e.DynaminData.Qty, true, e);
                    }

                    break;
                case WesFlowID.FLOW_IN_PALLET_TO_CARTON:
                    if (e.MethodName == "DeleteData")
                    {
                        AmoebaProductivity.DeleteKPI(e.DynaminData.ReceivingNo, e.DynaminData.PalletId,
                            KPIActionType.LSReceivingLabelling, 1, e.DynaminData.LabelCount, e);

                        LoggingService.InfoFormat(
                            "Delete Pallet2Carton KPI ReceivingNo:{0} PalletId :{1} ,LabelCount:{2}"
                            , e.DynaminData.ReceivingNo.ToString()
                            , e.DynaminData.PalletId, e.DynaminData.LabelCount, e);
                    }
                    else
                    {
                        AmoebaProductivity.AddTeamKPI(e.DynaminData.ReceivingNo, e.DynaminData.PalletId,
                            e.DynaminData.LabelCount, false, e);

                        LoggingService.InfoFormat("Add Pallet2Carton KPI ReceivingNo:{0} PalletId :{1} ,LabelCount:{2}"
                            , e.DynaminData.ReceivingNo.ToString()
                            , e.DynaminData.PalletId, e.DynaminData.LabelCount, e);

                        if (e.DynaminData.NeedAddPlus == true)
                        {
                            AmoebaProductivity.AddTeamKPI(e.DynaminData.ReceivingNo, e.DynaminData.PalletId,
                                e.DynaminData.LabelCount, true, e);

                            LoggingService.InfoFormat(
                                "Add Pallet2Carton Plus KPI ReceivingNo:{0} PalletId :{1} ,LabelCount:{2}"
                                , e.DynaminData.ReceivingNo.ToString()
                                , e.DynaminData.PalletId, e.DynaminData.LabelCount, e);
                        }
                    }

                    break;
                case WesFlowID.FLOW_IN_GATHER:
                    if ((bool)e.DynaminData.integralIsPlus)
                    {
                        AmoebaProductivity.AddTeamKPI((string)e.DynaminData.rcv, (string)e.DynaminData.integralPid,
                            (int)e.DynaminData.integralTotal, true, e);
                    }
                    else
                    {
                        AmoebaProductivity.AddTeamKPI((string)e.DynaminData.rcv, (string)e.DynaminData.integralPid,
                            (int)e.DynaminData.integralTotal, false, e);
                    }

                    break;
                case WesFlowID.FLOW_OUT_LABELING:
                    if ((bool)e.DynaminData.integralIsPlus)
                    {
                        AmoebaProductivity.AddTeamKPI((string)e.DynaminData.integralOperationNo,
                            (string)e.DynaminData.pid,
                            (int)e.DynaminData.integralTotal, true, e);
                    }
                    else
                    {
                        AmoebaProductivity.AddTeamKPI((string)e.DynaminData.integralOperationNo,
                            (string)e.DynaminData.pid,
                            (int)e.DynaminData.integralTotal, false, e);
                    }

                    break;
                case WesFlowID.FLOW_OUT_CARTON_LABELING:
                    AmoebaProductivity.AddTeamKPI(e.DynaminData.TruckOrder, e.DynaminData.PackageId,
                        e.DynaminData.LabelCount, false, e);

                    LoggingService.InfoFormat("Add CartonLabel KPI TruckOrder:{0} PackageId :{1} ,LabelCount:{2}"
                        , e.DynaminData.TruckOrder.ToString()
                        , e.DynaminData.PackageId, e.DynaminData.LabelCount);

                    if (e.DynaminData.NeedAddPlus == true)
                    {
                        AmoebaProductivity.AddTeamKPI(e.DynaminData.TruckOrder, e.DynaminData.PackageId,
                            1, true, e);

                        LoggingService.InfoFormat(
                            "Add CartonLabel Plus KPI TruckOrder:{0} PackageId :{1} ,LabelCount:{2}"
                            , e.DynaminData.TruckOrder.ToString()
                            , e.DynaminData.PackageId, e.DynaminData.LabelCount);
                    }

                    break;
                case WesFlowID.FLOW_OUT_BOARDING:

                    if (e.MethodName == "CartonEnd")
                    {
                        if (e.DynaminData.kpiPackages != null)
                        {
                            var packages =
                                DynamicJson.DeserializeObject<object>(
                                    DynamicJson.SerializeObject(e.DynaminData.kpiPackages));
                            e.DynaminData.kpiPackages = null;
                            foreach (var item in packages)
                            {
                                LoggingService.InfoFormat("Add CartonEnd KPI packageId:{0}", item.PackageID.ToString());
                                AmoebaProductivity.AddTeamKPI(e.DynaminData.truckNo.ToString(),
                                    item.PackageID.ToString(), 1, false, e);
                                LoggingService.InfoFormat("Add CartonEnd Plus KPI packageId:{0}",
                                    item.PackageID.ToString());
                                AmoebaProductivity.AddTeamKPI(e.DynaminData.truckNo.ToString(),
                                    item.PackageID.ToString(), 1, true, e);
                            }
                        }
                    }
                    else if (e.MethodName == "PalletEnd")
                    {
                        LoggingService.InfoFormat("Add PalletEnd KPI palletNo:{0}", e.DynaminData.palletNo.ToString());
                        AmoebaProductivity.AddTeamKPI(e.DynaminData.truckNo.ToString(),
                            e.DynaminData.palletNo.ToString(), e.DynaminData.kpiPackages.Count, false, e);
                        LoggingService.InfoFormat("Add PalletEnd Plus KPI palletNo:{0}",
                            e.DynaminData.palletNo.ToString());
                        AmoebaProductivity.AddTeamKPI(e.DynaminData.truckNo.ToString(),
                            e.DynaminData.palletNo.ToString(), 1, true, e);
                    }
                    else if (e.MethodName == "ScanFlowActionScanPackageId")
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(e.DynaminData.palletNo.ToString()))
                            {
                                AmoebaProductivity.DeleteShippingPalletKPI(e.DynaminData.truckNo.ToString(),
                                    e.DynaminData.palletNo.ToString(), e);
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Error(ex);
                        }
                    }
                    else if (e.MethodName == "DeleteCarton")
                    {
                        try
                        {
                            if (e.DynaminData.deleteKpiPackages != null)
                            {
                                var packages =
                                    DynamicJson.DeserializeObject<object>(
                                        DynamicJson.SerializeObject(e.DynaminData.deleteKpiPackages));
                                e.DynaminData.deleteKpiPackages = null;
                                foreach (var item in packages)
                                {
                                    if (item.CPkgID == null ||
                                        string.IsNullOrWhiteSpace(item.CPkgID.ToString())) //组箱的记录
                                    {
                                        AmoebaProductivity.DeleteShippingCartonKPI(e.DynaminData.truckNo.ToString(),
                                            item.PackageID.ToString(), e);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Error(ex);
                        }
                    }

                    break;
                default:
                    break;
            }
        }
    }
}