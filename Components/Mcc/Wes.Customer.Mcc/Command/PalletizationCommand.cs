using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.View;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;

namespace Wes.Customer.Mcc.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_BOARDING", CommandIndex = 5)]
    public class PalletizationViewCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<PalletizationAction, PalletizationViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_BOARDING", CommandIndex = 5)]
    public class PalletizationViewViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new PalletizationView();
        }
    }

}
