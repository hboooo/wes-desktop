using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Wes.Core;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Common
{
    public class AmoebaProductivity
    {
        private const long GetIdleTimeScriptId = 6422283473431564288;
        private const long InsertProductInfoScriptId = 6422286720447815680;
        private const long GetKPIMappingInfoScriptId = 6422697081516855296;
        private const long DeleteKPIScriptId = 6432872283785666560;

        private static Dictionary<string, List<AmoebaDifficultyModel>> _cacheAmoebaModels = new Dictionary<string, List<AmoebaDifficultyModel>>();
        private static Dictionary<string, List<AmoebaDifficultyModel>> _cacheAmoebaPlusModels = new Dictionary<string, List<AmoebaDifficultyModel>>();

        private static List<AmoebaDifficultyModel> _amoebaDifficultyModels = new List<AmoebaDifficultyModel>();
        private static List<AmoebaDifficultyModel> _amoebaDifficultyPlusModels = new List<AmoebaDifficultyModel>();
        private static string _actionType = string.Empty;
        private static string _actionTypePlus = string.Empty;
        private static string _operationType = string.Empty;


        public static void InitializeOperationType(int flowMask)
        {
            if (flowMask == (int)WesFlowID.FLOW_IN)
            {
                _operationType = KPIOperationType.Receiving.ToString();
            }
            else if (flowMask == (int)WesFlowID.FLOW_OUT)
            {
                _operationType = KPIOperationType.Shipping.ToString();
            }
            else
            {
                _operationType = KPIOperationType.Others.ToString();
            }
        }

        /// <summary>
        /// 根据命名空间初始化支援列表
        /// </summary>
        /// <param name="typeName"></param>
        public static void InitializeUser(string typeName, string methodName)
        {
            _actionType = GetActionName(typeName, methodName);
            _actionTypePlus = _actionType + "Plus";
        }

        public static void InitializeKpiConfig(string target)
        {
            string targetType = string.Empty;
            if (_operationType == KPIOperationType.Receiving.ToString())
            {
                targetType = "Supplier";
            }
            else if (_operationType == KPIOperationType.Shipping.ToString())
            {
                targetType = "Consignee";
            }

            string md5 = string.Format("{0}-{1}-{2}", _actionType, target, targetType).GetStringMd5String();

            if (_cacheAmoebaModels.ContainsKey(md5))
            {
                _amoebaDifficultyModels = _cacheAmoebaModels[md5];
            }
            else
            {
                _amoebaDifficultyModels = RestApi.NewInstance(Method.GET)
                    .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                    .AddScriptId(GetKPIMappingInfoScriptId)
                    .AddQueryParameter("actionType", _actionType)
                    .AddQueryParameter("target", target)
                    .AddQueryParameter("targetType", targetType)
                    .Execute()
                    .To<List<AmoebaDifficultyModel>>();
                _cacheAmoebaModels[md5] = _amoebaDifficultyModels;
            }

            if (_cacheAmoebaPlusModels.ContainsKey(md5))
            {
                _amoebaDifficultyPlusModels = _cacheAmoebaPlusModels[md5];
            }
            else
            {
                try
                {
                    _amoebaDifficultyPlusModels = RestApi.NewInstance(Method.GET)
                        .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                        .AddScriptId(GetKPIMappingInfoScriptId)
                        .AddQueryParameter("actionType", _actionTypePlus)
                        .AddQueryParameter("target", target)
                        .AddQueryParameter("targetType", targetType)
                        .Execute()
                        .To<List<AmoebaDifficultyModel>>();
                    _cacheAmoebaPlusModels[md5] = _amoebaDifficultyPlusModels;
                }
                catch (Exception ex)
                {
                    LoggingService.Error("获取kpi配置失败", ex);
                    throw ex;
                }
            }

        }



        /// <summary>
        /// 添加KPI
        /// </summary>
        /// <param name="operationNo">操作单号</param>
        /// <param name="packageID">箱号</param>
        /// <param name="qty">数量</param>
        /// <param name="isPlus">是否附加分</param>
        public static void AddTeamKPI(object operationNo, object packageID, object qty, object isPlus, ActionDefinition e)
        {
            string _operationNo = operationNo.ToString();
            string _packageId = packageID.ToString();
            int _qty = Convert.ToInt32(qty);
            bool _isPlus = Convert.ToBoolean(isPlus.ToString());
            var amoebaProductivityModel = new AmoebaProductivityModel
            {
                OperationNo = _operationNo,
                OperationDate = DateTime.Now,
                OperationType = _operationType,
                ActionType = _isPlus ? _actionTypePlus : _actionType,
                PackageId = _packageId.ToString(),
                Qty = _qty,
                Operator = WesDesktop.Instance.User.Code,
                VersionNo = WesDesktop.Instance.Version,
                DeviceNo = WesDesktop.Instance.User.Station,
                MacAddress = WesDesktop.Instance.MacAddress
            };
            RecordProductInfo(amoebaProductivityModel, _isPlus ? _amoebaDifficultyPlusModels : _amoebaDifficultyModels, e);
        }

        public static bool DeleteKPI(string operationNo, string packageId, KPIActionType actionType, int topNum, int qty, ActionDefinition e)
        {
            Dictionary<string, SupportUser> supports = WesDesktop.Instance.AddIn.Supports.GetSupports(WesFlow.Instance.GetFlow(e.FlowName));
            foreach (var item in supports)
            {
                if (!item.Value.Flows.Contains(WesFlow.Instance.GetFlow(e.FlowName)))
                    continue;
                bool res = RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(DeleteKPIScriptId)
                .AddJsonBody("operationNo", operationNo)
                .AddJsonBody("packageId", packageId)
                .AddJsonBody("actionType", actionType.ToString())
                .AddJsonBody("operationType", _operationType)
                .AddJsonBody("operator", item.Key)
                .AddJsonBody("qty", qty)
                .AddJsonBody("topNum", topNum)
                .Execute()
                .ToBoolean();
                if (!res) return false;
            }
            return true;
        }

        /// <summary>
        /// 刪除打板KPI
        /// </summary>
        /// <param name="operationNo"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static bool DeleteShippingPalletKPI(string operationNo, string packageId, ActionDefinition e)
        {
            int count = e.DynaminData.palletPackageCount;
            return DeleteKPI(operationNo, packageId, KPIActionType.LSCombinePallet, 1, count, e) && DeleteKPI(operationNo, packageId, KPIActionType.LSCombinePalletPlus, 10, 1, e);
        }

        public static bool DeleteShippingCartonKPI(string operationNo, string packageId, ActionDefinition e)
        {
            return DeleteKPI(operationNo, packageId, KPIActionType.LSCombineCarton, 1, 1, e) && DeleteKPI(operationNo, packageId, KPIActionType.LSCombineCartonPlus, 1, 1, e);
        }

        /// <summary>
        /// 插入作业明细
        /// </summary>
        /// <param name="amoebaProductivityModel"></param>
        /// <param name="amoebaDifficultyModels"></param>
        /// <returns></returns>
        public static bool RecordProductInfo(AmoebaProductivityModel amoebaProductivityModel, List<AmoebaDifficultyModel> amoebaDifficultyModels, ActionDefinition e)
        {
            Dictionary<string, int> dicUserCode = new Dictionary<string, int>();
            Dictionary<string, SupportUser> supports = WesDesktop.Instance.AddIn.Supports.GetSupports(WesFlow.Instance.GetFlow(e.FlowName));
            foreach (var item in supports)
            {
                if (!item.Value.Flows.Contains(WesFlow.Instance.GetFlow(e.FlowName)))
                    continue;
                if (!dicUserCode.ContainsKey(item.Key))
                {
                    int idleTime = 0;
                    //plus 不计算间隔时间
                    if (!amoebaProductivityModel.ActionType.Right(4)
                        .Equals("plus", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var result = RestApi.NewInstance(Method.GET)
                              .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                              .AddScriptId(GetIdleTimeScriptId)
                              .AddQueryParameter("user", item.Key)
                              .Execute()
                              .To<string>();
                        idleTime = Convert.ToInt32(result);
                    }
                    dicUserCode.Add(item.Key, idleTime / amoebaDifficultyModels.Count);
                }
            }
            var dtProductInfo = GetProductInfo(dicUserCode, amoebaProductivityModel, amoebaDifficultyModels);
            return InsertProductInfo(dtProductInfo);
        }

        /// <summary>
        /// 获取生产力记录明细
        /// </summary>
        /// <param name="userCodeDic"></param>
        /// <param name="productivityModel"></param>
        /// <param name="amoebaDifficultyModels"></param>
        /// <returns></returns>
        public static List<AmoebaProductivityModel> GetProductInfo(Dictionary<string, int> userCodeDic, AmoebaProductivityModel productivityModel, List<AmoebaDifficultyModel> amoebaDifficultyModels)
        {
            List<AmoebaProductivityModel> amoebaProductivityModels = new List<AmoebaProductivityModel>();
            foreach (var amoebaDifficultyModel in amoebaDifficultyModels)
            {
                decimal volume = Math.Round(productivityModel.Qty / userCodeDic.Count, 2, MidpointRounding.AwayFromZero);
                foreach (var userCode in userCodeDic)
                {
                    var amoebaProductivityModel = new AmoebaProductivityModel
                    {
                        OperationNo = productivityModel.OperationNo,
                        OperationDate = productivityModel.OperationDate,
                        OperationType = productivityModel.OperationType,
                        ActionType = productivityModel.ActionType,
                        DifficultyGrade = amoebaDifficultyModel.DifficultyGrade,
                        DifficultyWeight = amoebaDifficultyModel.DifficultyWeight,
                        DifficultyTimes = amoebaDifficultyModel.DifficultyTimes,
                        PackageId = productivityModel.PackageId,
                        Qty = productivityModel.Qty,
                        Volume = volume,
                        Productivity = Math.Round(volume * amoebaDifficultyModel.DifficultyWeight * amoebaDifficultyModel.DifficultyTimes, 2, MidpointRounding.AwayFromZero),
                        Operator = userCode.Key,
                        DeviceNo = productivityModel.DeviceNo,
                        MacAddress = productivityModel.MacAddress,
                        EndCustomer = amoebaDifficultyModel.EndCustomer,
                        TargetType = amoebaDifficultyModel.TargetType,
                        Target = amoebaDifficultyModel.Target,
                        VersionNo = productivityModel.VersionNo,
                        IdleTime = userCode.Value
                    };
                    amoebaProductivityModels.Add(amoebaProductivityModel);
                }
            }

            return amoebaProductivityModels;
        }

        public static bool InsertProductInfo(List<AmoebaProductivityModel> list)
        {
            var result = RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId(InsertProductInfoScriptId)
                .AddJsonBody("list", list)
                .Execute()
                .To<object>();
            return true;
        }

        /// <summary>
        /// 根据命名空间获取操作类型
        /// </summary>
        /// <param name="typeName"></param>
        public static string GetActionName(string typeName, string viewAction = null)
        {
            string actionName = string.Empty;
            if (typeName.Contains("Picking")||typeName.Contains("PickDispatching"))
            {
                //捡货
                actionName = KPIActionType.LSPickingSplitCarton.ToString();
            }
            else if (typeName.Contains("Sow"))
            {
                //分播
                actionName = KPIActionType.LSPacking.ToString();
            }
            else if (typeName.Contains("Gather"))
            {
                actionName = KPIActionType.LSDataCollection.ToString();
            }
            else if (typeName.Contains("CartonLabel"))
            {
                actionName = KPIActionType.LSCartonLabeling.ToString();
            }
            else if (typeName.Contains("Pallet2Carton"))
            {
                actionName = KPIActionType.LSReceivingLabelling.ToString();
            }
            else if (typeName.Contains("Labeling"))
            {
                actionName = KPIActionType.LSLabeling.ToString();
            }
            else if (typeName.Contains("Shipping"))
            {
                //组板
                if (viewAction != null)
                {
                    if (viewAction == "PalletEnd" || viewAction == "ScanFlowActionScanPackageId")
                    {
                        actionName = KPIActionType.LSCombinePallet.ToString();
                    }
                    else if (viewAction == "CartonEnd" || viewAction == "DeleteCarton")
                    {
                        actionName = KPIActionType.LSCombineCarton.ToString();
                    }
                }
            }
            return actionName;
        }

    }
}
