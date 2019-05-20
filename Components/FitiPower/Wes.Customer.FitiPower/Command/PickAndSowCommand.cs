using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.FitiPower.Action;
using Wes.Customer.FitiPower.View;
using Wes.Customer.FitiPower.ViewModel;

namespace Wes.Customer.FitiPower.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKING_AND_SOW", CommandIndex = 0)]
    public class PickAndSowCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<PickAndSowAction, PickAndSowViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKING_AND_SOW", CommandIndex = 0)]
    public class PickAndSowViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new PickAndSowView();
        }
    }
}
