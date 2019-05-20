using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Wes.Utilities;

namespace Wes.Core.Service
{
    /// <summary>
    /// WES容器
    /// </summary>
    public class WSServiceContainer : IServiceProvider, IServiceContainer, IDisposable
    {
        readonly ConcurrentStack<IServiceProvider> fallbackProviders = new ConcurrentStack<IServiceProvider>();
        readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        readonly List<Type> servicesToDispose = new List<Type>();

        public WSServiceContainer()
        {
            services.Add(typeof(WSServiceContainer), this);
            services.Add(typeof(IServiceContainer), this);
        }

        public void AddFallbackProvider(IServiceProvider provider)
        {
            this.fallbackProviders.Push(provider);
        }

        public object GetService(Type serviceType)
        {
            object instance;
            lock (services)
            {
                if (services.TryGetValue(serviceType, out instance))
                {
                    LoggingService.Debug($"Get Instance type:{serviceType.Name}");
                    ServiceCreatorCallback callback = instance as ServiceCreatorCallback;
                    if (callback != null)
                    {
                        LoggingService.Debug("Service startup: " + serviceType);
                        instance = callback(this, serviceType);
                        if (instance != null)
                        {
                            services[serviceType] = instance;
                            OnServiceInitialized(serviceType, instance);
                        }
                        else
                        {
                            services.Remove(serviceType);
                        }
                    }
                }
            }
            if (instance != null)
                return instance;
            foreach (var fallbackProvider in fallbackProviders)
            {
                instance = fallbackProvider.GetService(serviceType);
                if (instance != null)
                    return instance;
            }
            return null;
        }

        public void Dispose()
        {
            Type[] disposableTypes;
            lock (services)
            {
                disposableTypes = servicesToDispose.ToArray();
                servicesToDispose.Clear();
            }

            for (int i = disposableTypes.Length - 1; i >= 0; i--)
            {
                IDisposable disposable = null;
                lock (services)
                {
                    object serviceInstance;
                    if (services.TryGetValue(disposableTypes[i], out serviceInstance))
                    {
                        disposable = serviceInstance as IDisposable;
                        if (disposable != null)
                            services.Remove(disposableTypes[i]);
                    }
                }
                if (disposable != null)
                {
                    LoggingService.Debug("Service shutdown: " + disposableTypes[i]);
                    disposable.Dispose();
                }
            }
        }

        void OnServiceInitialized(Type serviceType, object serviceInstance)
        {
            IDisposable disposableService = serviceInstance as IDisposable;
            if (disposableService != null)
                servicesToDispose.Add(serviceType);
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            lock (services)
            {
                services.Add(serviceType, serviceInstance);
                OnServiceInitialized(serviceType, serviceInstance);
            }
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            AddService(serviceType, serviceInstance);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            lock (services)
            {
                services.Add(serviceType, callback);
            }
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            AddService(serviceType, callback);
        }

        public void RemoveService(Type serviceType)
        {
            lock (services)
            {
                object instance;
                if (services.TryGetValue(serviceType, out instance))
                {
                    services.Remove(serviceType);
                    IDisposable disposableInstance = instance as IDisposable;
                    if (disposableInstance != null)
                        servicesToDispose.Remove(serviceType);
                }
            }
        }

        public void RemoveService(Type serviceType, bool promote)
        {
            RemoveService(serviceType);
        }

    }
}
