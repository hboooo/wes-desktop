using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.View;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;

namespace Wes.Component.Widgets.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKEDINFO_REPORT", CommandIndex = 0xF1)]
    public class PickedInfoReportCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<PickedInfoReportAction, PickedInfoReportViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKEDINFO_REPORT", CommandIndex = 0xF1)]
    public class PickedInfoReportViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new PickedInfoReportView();
        }
    }
}
