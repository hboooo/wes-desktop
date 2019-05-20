using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Avnet.Action;
using Wes.Customer.Avnet.View;
using Wes.Customer.Avnet.ViewModel;

namespace Wes.Customer.Avnet.Command
{

    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_GATHER_ABNORMAL", CommandIndex = 2)]
    public class GatherAbnormalCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<GatherAbnormalAction, GatherAbnormalViewModel>();
        }

    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_GATHER_ABNORMAL", CommandIndex = 2)]
    public class GatherAbnormalViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new GatherAbnormalView();
        }
    }
}
