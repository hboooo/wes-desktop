using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Sinbon.Action;
using Wes.Customer.Sinbon.View;
using Wes.Customer.Sinbon.ViewModel;

namespace Wes.Customer.Sinbon.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_SOW", CommandIndex = 2)]
    public class SowCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<SowAction, SowViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_SOW", CommandIndex = 2)]
    public class SowViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new SowView();
        }
    }
}
