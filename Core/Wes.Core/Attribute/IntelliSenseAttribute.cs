using System;
using System.Reflection;
using Unity;
using Unity.Interception.Interceptors.TypeInterceptors.VirtualMethodInterception;
using Unity.Interception.PolicyInjection.Pipeline;
using Unity.Interception.PolicyInjection.Policies;

namespace Wes.Core.Attribute
{
    /// <summary>
    /// 自动完成
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class IntelliSenseAttribute : HandlerAttribute
    {
        public IntelliSenseAttribute()
        {
        }

        public IntelliSenseAttribute(bool isOpen)
        {
            this.IsOpen = isOpen;
        }

        public bool IsOpen { get; set; } = false;

        public override ICallHandler CreateHandler(IUnityContainer container)
        {
            return new IntelliSenseAttributeCallHandler(this);
        }
    }

    public class IntelliSenseAttributeCallHandler : ICallHandler
    {
        public IntelliSenseAttribute Attr { get; private set; }

        public IntelliSenseAttributeCallHandler(IntelliSenseAttribute attr)
        {
            this.Attr = attr;
        }

        IMethodReturn ICallHandler.Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            IMethodReturn result = getNext().Invoke(input, getNext);

            if (result.Exception == null)
            {
                if (Attr.IsOpen) OpenIntelliSense(input);
                if (!Attr.IsOpen) CloseIntelliSense(input);
            }

            return result;
        }

        int ICallHandler.Order { get; set; }

        /// <summary>
        /// 打开智能提示
        /// </summary>
        /// <param name="obj"></param>
        private void OpenIntelliSense(object actionObject)
        {
            SetIntelliSenseValue(actionObject, true);
        }

        /// <summary>
        /// 关闭智能提示
        /// </summary>
        /// <param name="obj"></param>
        private void CloseIntelliSense(object actionObject)
        {
            SetIntelliSenseValue(actionObject, false);
        }

        private void SetIntelliSenseValue(object actionObject, bool value)
        {
            var vmi = actionObject as VirtualMethodInvocation;
            Type type = vmi.Target.GetType();
            PropertyInfo propertyInfo = type.GetProperty("Vm");
            if (propertyInfo != null)
            {
                object viewModel = propertyInfo.GetValue(vmi.Target, null);
                if (viewModel != null)
                {
                    PropertyInfo prop = viewModel.GetType().GetProperty("SelfInfo");
                    if (prop != null)
                    {
                        dynamic useIntelligentObject = prop.GetValue(viewModel, null) as dynamic;
                        if (useIntelligentObject != null)
                        {
                            useIntelligentObject.UseIntelligent = value;
                        }
                    }
                }
            }
        }
    }

}
