using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Wes.Utilities;

namespace Wes.Server.Listener
{
    public class HttpRequestComposition
    {
        public static CompositionContainer ExportComposition()
        {
            try
            {
                var catalog = new AssemblyCatalog(Assembly.GetEntryAssembly());
                CompositionContainer compositionContainer = new CompositionContainer(catalog);
                compositionContainer.ComposeParts(typeof(HttpRequestComposition));
                return compositionContainer;
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return null;
        }
    }
}
