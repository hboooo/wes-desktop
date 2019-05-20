using System;
using System.ComponentModel.Design;

namespace Wes.Core.Service
{
    public static class WSExtensions
    {
        public static T GetService<T>(this IServiceProvider provider) where T : class
        {
            return (T)provider.GetService(typeof(T));
        }

        public static T GetRequiredService<T>(this IServiceContainer container) where T : class
        {
            return (T)container.GetService(typeof(T));
        }
    }
}
