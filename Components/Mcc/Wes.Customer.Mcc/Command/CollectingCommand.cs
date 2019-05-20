using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Mcc.Action;
using Wes.Customer.Mcc.View;
using Wes.Customer.Mcc.ViewModel;

namespace Wes.Customer.Mcc.Command
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
