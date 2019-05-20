using System;
using Wes.Core.Attribute;
using Wes.Flow;

namespace Wes.Core
{
    public class ActionDefinition
    {
        /// <summary>
        /// 动作函数名称
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 动作id
        /// </summary>
        public long AbilityID { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        public AbilityType AbilityType { get; set; }

        /// <summary>
        /// 标识在执行动作期间是否发生异常
        /// </summary>
        public bool IsRiseException { get; set; }

        /// <summary>
        /// ViewModel数据集合,对应ScanViewModelBase中的SelfInfo
        /// </summary>
        public dynamic DynaminData { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// ActionBase
        /// </summary>
        public Object ActionBase { get; set; }

        /// <summary>
        /// KPI类型
        /// </summary>
        public KPIActionType KPIType { get; set; }

        /// <summary>
        /// Consignee 属性值
        /// </summary>
        public string Consignee { get; set; }
        /// <summary>
        /// 是否为删除KPI
        /// </summary>
        public Boolean IsDeleteKPI { get; set; }
    }
}
