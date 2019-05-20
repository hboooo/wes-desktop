using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.View;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;

namespace Wes.Component.Widgets.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PACKAGEIDINFO_REPORT", CommandIndex = 0xF0)]
    public class PackageInfoReportCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<PackageInfoReportAction, PackageInfoReportViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PACKAGEIDINFO_REPORT", CommandIndex = 0xF0)]
    public class PackageInfoReportViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new PackageInfoReportView();
        }
    }
}
