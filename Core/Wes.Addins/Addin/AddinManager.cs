using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Addins.Addin
{
    public class AddinManager
    {
        private static CompositionContainer _container;

        public static CompositionContainer AddinContainer
        {
            get { return _container; }
        }
        
        public static T LoadAddins<T>(string dir) where T : new()
        {
            var obj = new T();
            ThreadPool.QueueUserWorkItem((threadObj) =>
            {
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new DirectoryCatalog(dir));
                _container = new CompositionContainer(catalog);
                try
                {
                    _container.ComposeParts(obj);
                }
                catch (CompositionException compositionException)
                {
                    LoggingService.Error("Addin compose Failed.", compositionException);
                }
            });
            WesApp.WaitThreadPool();
            return obj;
        }
    }
}
