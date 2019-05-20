using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Wes.Utilities;

namespace Wes.Core
{
    public class CoreComposition
    {
        public static CompositionContainer ExportComposition(Assembly assembly)
        {
            try
            {
                var catalog = new AssemblyCatalog(assembly);
                CompositionContainer compositionContainer = new CompositionContainer(catalog);
                compositionContainer.ComposeParts(typeof(CoreComposition));
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
