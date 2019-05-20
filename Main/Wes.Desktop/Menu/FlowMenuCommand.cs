using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Desktop.Menu
{
    [ExportMenuCommand(Header = "Flow", IsDisplay = false)]
    public class FlowMenuCommand : CommandWrapper
    {
        public override void Execute(object parameter)
        {
            MenuServiceHelper.OpenFlowWindow(parameter);
        }

    }
}
