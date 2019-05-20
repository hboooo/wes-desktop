using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Avnet.Action;
using Wes.Customer.Avnet.View;
using Wes.Customer.Avnet.ViewModel;

namespace Wes.Customer.Avnet.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKING_AND_SOW", CommandIndex = 1)]
    public class PickDispatchingCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<PickDispatchingAction, PickDispatchingViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKING_AND_SOW", CommandIndex = 1)]
    public class PickDispatchingViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new PickDispatchingView();
        }
    }
}
