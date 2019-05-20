using System;
using System.Threading;
using Unity;
using Unity.Interception.PolicyInjection.Pipeline;
using Unity.Interception.PolicyInjection.Policies;
using Wes.Core.Service;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class AbilityAttribute : HandlerAttribute
    {
        protected static readonly string DefaultName = "consignee";

        public AbilityAttribute()
            : this(null, 0, AbilityType.Null, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAttribute(AbilityType contractType)
            : this(null, 0, contractType, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAttribute(long contractId, AbilityType contractType)
            : this(null, contractId, contractType, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAttribute(string abilityName)
            : this(abilityName, 0, AbilityType.Null, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAttribute(string abilityName, AbilityType contractType)
            : this(abilityName, 0, contractType, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAttribute(string abilityName, AbilityType contractType, bool score)
            : this(abilityName, 0, contractType, score, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAttribute(long contractId, AbilityType contractType, bool score)
            : this(null, contractId, contractType, score, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAttribute(long contractId, AbilityType contractType, bool score, KPIActionType kpiType, string consigneePropertyName)
           : this(null, contractId, contractType, score, kpiType, consigneePropertyName, false)
        {
        }
        public AbilityAttribute(long contractId, AbilityType contractType, bool score, KPIActionType kpiType, string consigneePropertyName, bool isDeleteKpi)
           : this(null, contractId, contractType, score, kpiType, consigneePropertyName, isDeleteKpi)
        {
        }

        public AbilityAttribute(string abilityName, long abilityId, AbilityType abilityType, bool score, KPIActionType kpiType, string consigneePropertyName, bool isDeleteKpi)
        {
            this.AbilityName = abilityName;
            this.AbilityType = abilityType;
            this.AbilityId = abilityId;
            this.Score = score;
            this.KPIType = kpiType;
            this.ConsigneePropertyName = consigneePropertyName;
            this.IsDeleteKPI = isDeleteKpi;
        }

        public string AbilityName { get; private set; }
        public long AbilityId { get; private set; }
        public AbilityType AbilityType { get; private set; }
        /// <summary>
        /// 是否计分
        /// </summary>
        public bool Score { get; private set; }
        /// <summary>
        /// KPI类型
        /// </summary>
        public KPIActionType KPIType { get; private set; }
        /// <summary>
        /// Consignee 属性名称
        /// </summary>
        public string ConsigneePropertyName { get; set; }
        /// <summary>
        /// 是否为删除KPI
        /// </summary>
        public bool IsDeleteKPI { get; set; } = false;

        public override ICallHandler CreateHandler(IUnityContainer container)
        {
            return new AbilityAttributeCallHandler(this);
        }
    }

    public class AbilityAttributeCallHandler : ICallHandler
    {
        public AbilityAttribute Attr { get; private set; }

        public AbilityAttributeCallHandler(AbilityAttribute attr)
        {
            this.Attr = attr;
        }

        IMethodReturn ICallHandler.Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            LoggingService.Debug($"Avnet Ability invoke {input.Target.GetType().Name}");
            IMethodReturn result = getNext().Invoke(input, getNext);
            var param = ActionListenParams.PrepareDefinition(input, result, Attr);
            if (param != null)
            {
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    WS.ActionAvnetListenInvokerService.Invoker(param);
                });
            }

            return result;
        }

        int ICallHandler.Order { get; set; }
    }
}