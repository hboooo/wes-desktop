using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Wes.Core;
using Wes.Desktop.Windows.Model;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Desktop.Windows.KPI
{
    public class AmoebaKpiDataAdapter
    {
        private static ConcurrentDictionary<string, List<AmoebaDifficultyModel>> _cacheAmoebaModels = new ConcurrentDictionary<string, List<AmoebaDifficultyModel>>();

        private List<AmoebaDifficultyModel> _amoebaDifficultyModels = new List<AmoebaDifficultyModel>();

        private string _operationType = string.Empty;

        /// <summary>
        /// 根据命名空间初始化支援列表
        /// </summary>
        /// <param name="typeName"></param>
        public void Initialize(ActionDefinition e, KPIActionType actionType)
        {
            string typeName = e.TypeName;
            string methodName = e.MethodName;
            string target = e.Consignee;

            WesFlowID flowId = WesFlow.Instance.GetFlow(e.FlowName);
            int flowMask = WesFlow.Instance.GetFlowMask((int)flowId);

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

            string targetType = string.Empty;
            if (_operationType == KPIActionType.Receiving.ToString())
            {
                targetType = "Supplier";
            }
            else if (_operationType == KPIActionType.Shipping.ToString())
            {
                targetType = "Consignee";
            }

            string md5 = string.Format("{0}-{1}-{2}", actionType.ToString(), target, targetType).GetStringMd5String();

            if (_cacheAmoebaModels.ContainsKey(md5))
            {
                _amoebaDifficultyModels = _cacheAmoebaModels[md5];
            }
            else
            {
                try
                {
                    _amoebaDifficultyModels = RestApi.NewInstance(Method.GET)
                        .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                        .AddScriptId((long)KPIScriptID.KPI_AMOEBA_DIFFICULTY_GRADE)
                        .AddQueryParameter("actionType", actionType.ToString())
                        .AddQueryParameter("target", target)
                        .AddQueryParameter("targetType", targetType)
                        .Execute()
                        .To<List<AmoebaDifficultyModel>>();
                    _cacheAmoebaModels[md5] = _amoebaDifficultyModels;
                }
                catch (Exception ex)
                {
                    LoggingService.Error("获取kpi配置失败", ex);
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
        public void AddTeamKPI(object operationNo, object packageID, object qty, KPIActionType actionType, ActionDefinition e)
        {
            try
            {
                string _operationNo = operationNo.ToString();
                string _packageId = packageID.ToString();
                int _qty = Convert.ToInt32(qty);
                string action = actionType.ToString();
                var amoebaProductivityModel = new AmoebaProductivityModel
                {
                    OperationNo = _operationNo,
                    OperationDate = DateTime.Now,
                    OperationType = _operationType,
                    ActionType = action,
                    PackageId = _packageId.ToString(),
                    Qty = _qty,
                    Operator = WesDesktop.Instance.User.Code,
                    VersionNo = WesDesktop.Instance.Version,
                    DeviceNo = WesDesktop.Instance.User.Station,
                    MacAddress = WesDesktop.Instance.MacAddress
                };
                RecordProductInfo(amoebaProductivityModel, _amoebaDifficultyModels, e);
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        public void DeleteKPI(object operationNo, object packageId, KPIActionType actionType, object topNum, object qty, ActionDefinition e)
        {
            try
            {
                string opNo = operationNo.ToString();
                string pkgId = packageId.ToString();
                int topNumInt = Convert.ToInt32(topNum);
                int qtyInt = Convert.ToInt32(qty);

                Dictionary<string, SupportUser> supports = WesDesktop.Instance.AddIn.Supports.GetSupports(WesFlow.Instance.GetFlow(e.FlowName));
                foreach (var item in supports)
                {
                    if (!item.Value.Flows.Contains(WesFlow.Instance.GetFlow(e.FlowName)))
                        continue;
                    RestApi.NewInstance(Method.POST)
                    .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                    .AddScriptId((long)KPIScriptID.DELETE_KPI)
                    .AddJsonBody("operationNo", opNo)
                    .AddJsonBody("packageId", pkgId)
                    .AddJsonBody("actionType", actionType.ToString())
                    .AddJsonBody("operationType", _operationType)
                    .AddJsonBody("operator", item.Key)
                    .AddJsonBody("qty", qtyInt)
                    .AddJsonBody("topNum", topNumInt)
                    .Execute();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        /// <summary>
        /// 插入作业明细
        /// </summary>
        /// <param name="amoebaProductivityModel"></param>
        /// <param name="amoebaDifficultyModels"></param>
        /// <returns></returns>
        private bool RecordProductInfo(AmoebaProductivityModel amoebaProductivityModel, List<AmoebaDifficultyModel> amoebaDifficultyModels, ActionDefinition e)
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
                              .AddScriptId((long)KPIScriptID.GET_IDLE_TIME)
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
        private List<AmoebaProductivityModel> GetProductInfo(Dictionary<string, int> userCodeDic, AmoebaProductivityModel productivityModel, List<AmoebaDifficultyModel> amoebaDifficultyModels)
        {
            List<AmoebaProductivityModel> amoebaProductivityModels = new List<AmoebaProductivityModel>();
            foreach (var amoebaDifficultyModel in amoebaDifficultyModels)
            {
                decimal volume = Math.Round(productivityModel.Qty / userCodeDic.Count, 2, MidpointRounding.AwayFromZero);
                foreach (var userCode in userCodeDic)
                {
                    //特殊加成
                    var weight = RestApi.NewInstance(Method.GET)
                        .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                        .AddScriptId((long)KPIScriptID.GET_SPECIAL_DIFFICULTY_WEIGHT)
                        .AddQueryParameter("endCustomer", amoebaDifficultyModel.EndCustomer)
                        .AddQueryParameter("user", userCode.Key)
                        .AddQueryParameter("difficultyGrade", amoebaDifficultyModel.DifficultyGrade)
                        .Execute()
                        .To<string>();
                    decimal specialDifficultyWeight = Convert.ToDecimal(weight);
                    var amoebaProductivityModel = new AmoebaProductivityModel
                    {
                        OperationNo = productivityModel.OperationNo,
                        OperationDate = productivityModel.OperationDate,
                        OperationType = productivityModel.OperationType,
                        ActionType = productivityModel.ActionType,
                        DifficultyGrade = amoebaDifficultyModel.DifficultyGrade,
                        DifficultyWeight = amoebaDifficultyModel.DifficultyWeight * specialDifficultyWeight,
                        DifficultyTimes = amoebaDifficultyModel.DifficultyTimes,
                        PackageId = productivityModel.PackageId,
                        Qty = productivityModel.Qty,
                        Volume = volume,
                        Productivity = Math.Round(volume * amoebaDifficultyModel.DifficultyWeight * amoebaDifficultyModel.DifficultyTimes * specialDifficultyWeight, 2, MidpointRounding.AwayFromZero),
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

        private bool InsertProductInfo(List<AmoebaProductivityModel> list)
        {
            var result = RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "/0/{si}")
                .AddScriptId((long)KPIScriptID.INSERT_PRODUCT_INFO)
                .AddJsonBody("list", list)
                .Execute()
                .To<object>();
            return true;
        }
    }
}
