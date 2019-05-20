using System;
using System.Threading;
using Unity;
using Unity.Interception.PolicyInjection.Pipeline;
using Wes.Core.Service;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class AbilityAbleAttribute : AbilityAttribute
    {
        public AbilityAbleAttribute()
            : base(null, 0, AbilityType.Null, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(bool score)
            : base(null, 0, AbilityType.Null, score, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(AbilityType contractType)
            : base(null, 0, contractType, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(long contractId, AbilityType contractType)
            : base(null, contractId, contractType, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(string abilityName)
            : base(abilityName, 0, AbilityType.Null, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(string abilityName, AbilityType contractType)
            : base(abilityName, 0, contractType, false, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(string abilityName, AbilityType contractType, bool score)
            : base(abilityName, 0, contractType, score, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(long contractId, AbilityType contractType, bool score)
            : base(null, contractId, contractType, score, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(AbilityType contractType, bool score)
            : base(null, 0, contractType, score, KPIActionType.Null, DefaultName, false)
        {
        }

        public AbilityAbleAttribute(bool score, KPIActionType kpiType, string consigneePropertyName)
            : base(null, 0, AbilityType.Null, score, kpiType, consigneePropertyName, false)
        {
        }

        public AbilityAbleAttribute(long contractId, AbilityType contractType, bool score, KPIActionType kpiType, string consigneePropertyName)
           : base(null, contractId, contractType, score, kpiType, consigneePropertyName, false)
        {
        }

        public AbilityAbleAttribute(long contractId, AbilityType contractType, bool score, KPIActionType kpiType, string consigneePropertyName, bool isDelete)
           : base(null, contractId, contractType, score, kpiType, consigneePropertyName, isDelete)
        {
        }

        public AbilityAbleAttribute(string abilityName, long abilityId, AbilityType abilityType, bool score) : base(abilityName, abilityId, abilityType, score, KPIActionType.Null, DefaultName, false)
        {

        }

        public override ICallHandler CreateHandler(IUnityContainer container)
        {
            return new AbilityAbleAttributeCallHandler(this);
        }
    }

    public class AbilityAbleAttributeCallHandler : ICallHandler
    {
        public AbilityAbleAttribute Attr { get; private set; }

        public AbilityAbleAttributeCallHandler(AbilityAbleAttribute attr)
        {
            this.Attr = attr;
        }

        IMethodReturn ICallHandler.Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            LoggingService.Debug($"Able Ability invoke {input.Target.GetType().Name}");
            IMethodReturn result = getNext().Invoke(input, getNext);
            if (this.Attr != null && this.Attr.Score && result.ReturnValue != null && result.ReturnValue is Boolean)
            {
                if (true == Boolean.Parse(result.ReturnValue.ToString()))
                {
                    var param = ActionListenParams.PrepareKPIDefinition(input, result, Attr);
                    if (param != null)
                    {
                        ThreadPool.QueueUserWorkItem((obj) =>
                        {
                            WS.ActionListenInvokerService.Invoker(param);
                        });
                    }
                }
            }
            return result;
        }

        int ICallHandler.Order { get; set; }
    }
}
