using CommonServiceLocator;
using Unity;
using Unity.Interception.ContainerIntegration;
using Unity.ServiceLocation;

namespace Wes.Core.Bean
{
    public class BeanFactory
    {
        private static IUnityContainer _container = new UnityContainer();
        private static Interception _interceptor = null;

        static BeanFactory()
        {
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(_container));
        }
        
        public static void Clear()
        {
            if (_container != null)
            {
                _container.Dispose();
                _container = null;
                _interceptor = null;
            }
        }

        public static IUnityContainer Default
        {
            get
            {
                if (_container == null)
                {
                    _container = new UnityContainer();
                }
                return _container;
            }
        }

        public static Interception Interceptor
        {
            get
            {
                if (_interceptor == null)
                {
                    _interceptor = BeanFactory.Default.AddNewExtension<Interception>().Configure<Interception>();
                }

                return _interceptor;
            }
        }
    }
}