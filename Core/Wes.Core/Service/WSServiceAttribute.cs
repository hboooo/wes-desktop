using System;
using System.Collections.Generic;
using Wes.Utilities;

namespace Wes.Core.Service
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
    public class WSServiceAttribute : System.Attribute
    {
        public WSServiceAttribute()
        {
        }

        public WSServiceAttribute(string propertyNamespace)
        {
            this.PropertyNamespace = propertyNamespace;
        }

        public string PropertyNamespace { get; set; }

        public Type Implementation { get; set; }

    }

    /// <summary>
    /// 实例化WES对象
    /// </summary>
    sealed class WDServiceProvider : IServiceProvider
    {
        Dictionary<Type, object> serviceDict = new Dictionary<Type, object>();

        public object GetService(Type serviceType)
        {
            object instance;
            lock (serviceDict)
            {
                if (!serviceDict.TryGetValue(serviceType, out instance))
                {
                    var attrs = serviceType.GetCustomAttributes(typeof(WSServiceAttribute), false);
                    if (attrs.Length == 1)
                    {
                        var attr = (WSServiceAttribute)attrs[0];
                        if (attr.Implementation != null)
                        {
                            LoggingService.Debug($"CreateInstance type:{attr.Implementation}");
                            instance = Activator.CreateInstance(attr.Implementation);
                        }
                    }
                    serviceDict.Add(serviceType, instance);
                }
            }
            return instance;
        }
    }
}