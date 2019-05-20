using System;
using System.ComponentModel.Design;

namespace Wes.Core.Service
{
    /// <summary>
    /// 提供单例服务
    /// </summary>
    public static class ServicesSingleton
    {
        static readonly IServiceContainer serviceContainer = new WSServiceContainer();
        volatile static IServiceContainer containerInstance = serviceContainer;
        static readonly IServiceProvider serviceProvider = new WDServiceProvider();
        volatile static IServiceProvider instance = serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get { return instance; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                instance = value;
            }
        }

        public static IServiceContainer ServiceContainer
        {
            get { return containerInstance; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                containerInstance = value;
            }
        }
    }
}
