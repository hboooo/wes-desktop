﻿using System.ComponentModel.Composition;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.Promaster.Action;
using Wes.Customer.Promaster.View;
using Wes.Customer.Promaster.ViewModel;

namespace Wes.Customer.Promaster.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKING_AND_SOW", CommandIndex = 0)]
    public class PickDispatchingCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<PickDispatchingAction, PickDispatchingViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_OUT_PICKING_AND_SOW", CommandIndex = 0)]
    public class PickDispatchingViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new PickDispatchingView();
        }
    }
}
