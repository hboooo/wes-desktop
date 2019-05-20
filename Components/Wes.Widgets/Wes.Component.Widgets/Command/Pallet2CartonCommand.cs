using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.View;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;

namespace Wes.Component.Widgets.Command
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
