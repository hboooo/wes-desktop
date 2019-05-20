using System;
using Unity;
using Unity.Interception.Interceptors.TypeInterceptors.VirtualMethodInterception;
using Wes.Core.Bean;

namespace Wes.Core
{
    public class ViewModelFactory
    {
        public static object CreateViewModel<A, V>()
        {
            return BeanFactory.Default.Resolve<V>();
        }

        public static object CreateActoin<VA>()
        {
            BeanFactory.Default.RegisterType<VA>();
            BeanFactory.Interceptor.SetInterceptorFor<VA>(new VirtualMethodInterceptor());

            return BeanFactory.Default.Resolve<VA>();
        }

        public static object CreateActionForType(Type type)
        {
            BeanFactory.Default.RegisterType(type);
            BeanFactory.Interceptor.SetInterceptorFor(type, new VirtualMethodInterceptor());

            return BeanFactory.Default.Resolve(type);
        }
    }
}
