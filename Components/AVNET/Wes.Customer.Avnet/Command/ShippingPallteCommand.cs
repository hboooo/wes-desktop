using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Avnet.Action;
using Wes.Customer.Avnet.View;
using Wes.Customer.Avnet.ViewModel;

namespace Wes.Customer.Avnet.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_BOARDING", CommandIndex = 5)]
    public class ShippingPallteCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<ShippingPallteAction, ShippingPallteViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_BOARDING", CommandIndex = 5)]
    public class ShippingPallteViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new ShippingPalltesView();
        }
    }

}
