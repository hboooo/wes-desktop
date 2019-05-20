using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Allsor.Action;
using Wes.Customer.Allsor.View;
using Wes.Customer.Allsor.ViewModel;

namespace Wes.Customer.Allsor.Command
{

    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_IN_GATHER", CommandIndex = 1)]
    public class CollectingCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<CollectingAction, CollectingViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_IN_GATHER", CommandIndex = 1)]
    public class GatherViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new CollectingView();
        }
    }

}
