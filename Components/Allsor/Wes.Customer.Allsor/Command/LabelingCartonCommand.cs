using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Allsor.Action;
using Wes.Customer.Allsor.View;
using Wes.Customer.Allsor.ViewModel;

namespace Wes.Customer.Allsor.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_CARTON_LABELING", CommandIndex = 4)]
    public class LabelingCartonCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<LabelingCartonAction, LabelingCartonViewModel>();
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
