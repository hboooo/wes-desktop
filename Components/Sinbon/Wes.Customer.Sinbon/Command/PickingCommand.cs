using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Sinbon.Action;
using Wes.Customer.Sinbon.View;
using Wes.Customer.Sinbon.ViewModel;

namespace Wes.Customer.Sinbon.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKING", CommandIndex = 1)]
    public class PickingCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<PickingAction, PickingViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKING", CommandIndex = 1)]
    public class PickingViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new PickingView();
        }
    }
}
