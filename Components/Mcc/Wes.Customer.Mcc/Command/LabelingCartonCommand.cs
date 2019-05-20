using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Mcc.Action;
using Wes.Customer.Mcc.View;
using Wes.Customer.Mcc.ViewModel;

namespace Wes.Customer.Mcc.Command
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
