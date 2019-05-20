using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Allsor.Action;
using Wes.Customer.Allsor.View;
using Wes.Customer.Allsor.ViewModel;

namespace Wes.Customer.Allsor.Command
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
    public class DecalsViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new LabelingView();
        }
    }
}