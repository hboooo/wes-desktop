using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.View;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;

namespace Wes.Component.Widgets.Command
{

    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_GATHER_ABNORMAL", CommandIndex = 2)]
    public class CollectingAbnormalCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<CollectingAbnormalAction, CollectingAbnormalViewModel>();
        }

    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_GATHER_ABNORMAL", CommandIndex = 2)]
    public class CollectingAbnormalViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new CollectingAbnormalView();
        }
    }
}
