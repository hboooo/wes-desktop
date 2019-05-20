using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Sinbon.Action;
using Wes.Customer.Sinbon.View;
using Wes.Customer.Sinbon.ViewModel;

namespace Wes.Customer.Sinbon.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_CARTON_LABELING", CommandIndex = 4)]
    public class CartonLabelCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<CartonLabelAction, CartonLabelViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_CARTON_LABELING", CommandIndex = 4)]
    public class CartonLabelViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new CartonLabelView();
        }
    }
}
