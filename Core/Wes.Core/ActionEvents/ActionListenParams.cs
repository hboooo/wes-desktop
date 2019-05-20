using System;
using System.Reflection;
using Unity.Interception.Interceptors.TypeInterceptors.VirtualMethodInterception;
using Unity.Interception.PolicyInjection.Pipeline;
using Wes.Core.Attribute;
using Wes.Utilities;

namespace Wes.Core
{
    public static class ActionListenParams
    {
        #region Prepare Params

        public static ActionDefinition PrepareDefinition(IMethodInvocation input, IMethodReturn methodReturn, AbilityAttribute attr)
        {
            ActionDefinition actionDefinition = new ActionDefinition();
            try
            {
                var vmi = input as VirtualMethodInvocation;
                object sender = vmi.Target;
                var methodInfo = vmi.MethodBase as MethodInfo;

                dynamic objectParams = default(dynamic);
                string flowName = string.Empty;
                Type type = vmi.Target.GetType();

                MethodInfo isActionVaildMethodInfo = type.GetMethod("IsActionVaild");
                MethodInfo clearActionValidMethodInfo = type.GetMethod("ClearActionValid");
                if (isActionVaildMethodInfo != null && clearActionValidMethodInfo != null)
                {
                    bool isValid = (bool)isActionVaildMethodInfo.Invoke(vmi.Target, null);
                    clearActionValidMethodInfo.Invoke(vmi.Target, null);
                    if (!isValid) return null;
                }
                else
                {
                    return null;
                }

                PropertyInfo propertyInfo = type.GetProperty("Vm");
                if (propertyInfo != null)
                {
                    object viewModel = propertyInfo.GetValue(vmi.Target, null);
                    Type viewModelType = viewModel.GetType();
                    PropertyInfo prop = viewModelType.GetProperty("SelfInfo");
                    if (prop != null)
                    {
                        objectParams = prop.GetValue(viewModel, null);
                    }
                    PropertyInfo flowProp = viewModelType.GetProperty("FlowName");
                    if (flowProp != null)
                    {
                        flowName = flowProp.GetValue(viewModel, null).ToString();
                    }
                }

                bool isException = false;
                if (methodReturn.Exception != null)
                {
                    isException = true;
                }
                string typeName = methodInfo.DeclaringType.FullName;

                actionDefinition.ActionBase = sender;
                actionDefinition.DynaminData = DynamicJson.DeserializeObject<object>(DynamicJson.SerializeObject(objectParams));
                actionDefinition.MethodName = methodInfo.Name;
                actionDefinition.TypeName = typeName;
                actionDefinition.AbilityID = attr.AbilityId;
                actionDefinition.AbilityType = attr.AbilityType;
                actionDefinition.IsRiseException = isException;
                actionDefinition.FlowName = flowName;
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return actionDefinition;
        }

        public static ActionDefinition PrepareKPIDefinition(IMethodInvocation input, IMethodReturn methodReturn, AbilityAbleAttribute attr)
        {
            ActionDefinition actionDefinition = new ActionDefinition();
            try
            {
                var vmi = input as VirtualMethodInvocation;
                object sender = vmi.Target;
                var methodInfo = vmi.MethodBase as MethodInfo;

                dynamic objectParams = default(dynamic);
                string flowName = string.Empty;
                string consignee = string.Empty;
                Type type = vmi.Target.GetType();

                PropertyInfo propertyInfo = type.GetProperty("Vm");
                if (propertyInfo != null)
                {
                    object viewModel = propertyInfo.GetValue(vmi.Target, null);
                    Type viewModelType = viewModel.GetType();
                    PropertyInfo prop = viewModelType.GetProperty("SelfInfo");
                    if (prop != null)
                    {
                        objectParams = prop.GetValue(viewModel, null);
                        consignee = objectParams.GetMember(attr.ConsigneePropertyName);
                    }
                    PropertyInfo flowProp = viewModelType.GetProperty("FlowName");
                    if (flowProp != null)
                    {
                        flowName = flowProp.GetValue(viewModel, null).ToString();
                    }
                }

                bool isException = false;
                if (methodReturn.Exception != null)
                {
                    isException = true;
                }
                string typeName = methodInfo.DeclaringType.FullName;

                actionDefinition.ActionBase = sender;
                actionDefinition.DynaminData = DynamicJson.DeserializeObject<object>(DynamicJson.SerializeObject(objectParams));
                actionDefinition.MethodName = methodInfo.Name;
                actionDefinition.TypeName = typeName;
                actionDefinition.AbilityID = attr.AbilityId;
                actionDefinition.AbilityType = attr.AbilityType;
                actionDefinition.IsRiseException = isException;
                actionDefinition.FlowName = flowName;
                actionDefinition.KPIType = attr.KPIType;
                actionDefinition.Consignee = consignee;
                actionDefinition.IsDeleteKPI = attr.IsDeleteKPI;
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return actionDefinition;
        }

        #endregion

    }
}
