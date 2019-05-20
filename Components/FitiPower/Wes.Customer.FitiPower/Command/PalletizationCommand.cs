using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Component.Widgets.View;
using Wes.Core;
using Wes.Customer.FitiPower.Action;
using Wes.Customer.FitiPower.ViewModel;

namespace Wes.Customer.FitiPower.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_BOARDING", CommandIndex = 5)]
    public class PalletizationViewCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<FitipowerPalletizationAction, FitipowerPalletizationViewModel>();
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
