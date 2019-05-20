using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.FitiPower.Action;
using Wes.Customer.FitiPower.View;
using Wes.Customer.FitiPower.ViewModel;

namespace Wes.Customer.FitiPower.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_LABELING", CommandIndex = 3)]
    public class LabelingCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<LabelingAction, LabelingViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_LABELING", CommandIndex = 3)]
    public class LabelingViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new LabelingView();
        }
    }
}