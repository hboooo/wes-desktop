using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.View;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;

namespace Wes.Component.Widgets.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_TRANSPORTATION_LABELING", CommandIndex = 0xE0)]
    public class TransportationCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<TransportationLabelingAction, TransportationLabelingViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_TRANSPORTATION_LABELING", CommandIndex = 0xE0)]
    public class TransportationViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new TransportationLabelingView();
        }
    }
}
