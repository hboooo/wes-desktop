using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.FitiPower.Action;
using Wes.Customer.FitiPower.View;
using Wes.Customer.FitiPower.ViewModel;

namespace Wes.Customer.FitiPower.Command
{

    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_GATHER_ABNORMAL", CommandIndex = 2)]
    public class CollectingAbnormalCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<CollectingAbnormalAction, CollectingAbnormalViewModel>();
        }

    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_GATHER_ABNORMAL", CommandIndex = 2)]
    public class CollectingAbnormalViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new CollectingAbnormalView();
        }
    }
}
