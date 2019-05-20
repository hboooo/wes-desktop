using System.ComponentModel.Design;

namespace Wes.Core.Service
{
    /// <summary>
    /// WES服务注册/提取
    /// </summary>
    public static class WS
    {
        public static IServiceContainer Services
        {
            get { return ServicesSingleton.ServiceContainer; }
        }

        public static T GetService<T>() where T : class
        {
            return ServicesSingleton.ServiceProvider.GetService<T>();
        }

        public static IActionNotity ActionNotityService
        {
            get { return Services.GetRequiredService<IActionNotity>(); }
        }

        public static IActionListenInvoker ActionListenInvokerService
        {
            get { return Services.GetRequiredService<IActionListenInvoker>(); }
        }

        public static IActionAvnetListenInvoker ActionAvnetListenInvokerService
        {
            get { return Services.GetRequiredService<IActionAvnetListenInvoker>(); }
        }

    }
}
