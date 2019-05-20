using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Component.Widgets.View;
using Wes.Core;
using Wes.Customer.Sinbon.Action;
using Wes.Customer.Sinbon.ViewModel;

namespace Wes.Customer.Sinbon.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_BOARDING", CommandIndex = 5)]
    public class PalletizationViewCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<SinbonPalletizationAction, SinbonPalletizationViewModel>();
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
