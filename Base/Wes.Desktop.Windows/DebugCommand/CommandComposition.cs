using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Wes.Core;

namespace Wes.Desktop
{
    public class CommandComposition
    {
        private static CompositionContainer _controlContainer = null;
        private static CompositionContainer _desktopContainer = null;

        /// <summary>
        /// Wes.Desktop.Windows容器
        /// </summary>
        public static CompositionContainer ControlContainer
        {
            get
            {
                if (_controlContainer == null)
                {
                    _controlContainer = CoreComposition.ExportComposition(Assembly.GetExecutingAssembly());
                }
                return _controlContainer;
            }
        }

        /// <summary>
        /// Desktop組合容器
        /// </summary>
        public static CompositionContainer DesktopContainer
        {
            get
            {
                if (_desktopContainer == null)
                {
                    _desktopContainer = CoreComposition.ExportComposition(Assembly.GetCallingAssembly());
                }
                return _desktopContainer;
            }
        }
    }
}
