using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Wes.Addins.ICommand;
using Wes.Core;
using Wes.Customer.FitiPower.Action;
using Wes.Customer.FitiPower.View;
using Wes.Customer.FitiPower.ViewModel;

namespace Wes.Customer.FitiPower.Command
{
    [Export(typeof(IViewModelCommand))]
    [ComponentsCommand(CommandName = "FLOW_IN_STORAGE_MOVE_COLLECTING", CommandIndex = 3)]
    public class StorageMoveCollectingViewModelCommand : IViewModelCommand
    {
        public object Execute(object obj)
        {
            return ViewModelFactory.CreateViewModel<StorageMoveCollectingAction, StorageMoveCollectingViewModel>();
        }
    }

    [Export(typeof(IViewCommand))]
    [ComponentsCommand(CommandName = "FLOW_IN_STORAGE_MOVE_COLLECTING", CommandIndex = 3)]
    public class StorageMoveCollectingViewCommand : IViewCommand
    {
        public object Execute(object obj)
        {
            return new StorageMoveCollecting();
        }
    }
}
