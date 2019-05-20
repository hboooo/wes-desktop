using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Avnet.Action;
using Wes.Customer.Avnet.View;
using Wes.Customer.Avnet.ViewModel;

namespace Wes.Customer.Avnet.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_IN_PALLET_TO_CARTON", CommandIndex = 0)]
    public class Pallet2CartonCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<Pallet2CartonAction, Pallet2CartonViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_IN_PALLET_TO_CARTON", CommandIndex = 0)]
    public class Pallet2CartonViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new Pallet2CartonView();
        }
    }
}
