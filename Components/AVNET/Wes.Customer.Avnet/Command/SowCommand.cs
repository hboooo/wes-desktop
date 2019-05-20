using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Avnet.Action;
using Wes.Customer.Avnet.View;
using Wes.Customer.Avnet.ViewModel;

namespace Wes.Customer.Avnet.Command
{
    [Export(typeof(IViewModelCommand))]
    //[ComponentsCommand(CommandName = "FLOW_OUT_SOW", CommandIndex = 2)]
    public class SowCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<SowAction, SowViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    //[ComponentsCommand(CommandName = "FLOW_OUT_SOW", CommandIndex = 2)]
    public class SowViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new SowView();
        }
    }
}
