using System;
using System.Windows.Input;

namespace Wes.Desktop.Menu
{
    class MenuCommandWrapper : ICommand
    {
        private readonly ICommand wrappedCommand;

        public MenuCommandWrapper(ICommand wrappedCommand)
        {
            this.wrappedCommand = wrappedCommand;
        }

        public static ICommand Unwrap(ICommand command)
        {
            MenuCommandWrapper w = command as MenuCommandWrapper;
            if (w != null)
                return w.wrappedCommand;
            else
                return command;
        }

        public event EventHandler CanExecuteChanged
        {
            add { wrappedCommand.CanExecuteChanged += value; }
            remove { wrappedCommand.CanExecuteChanged -= value; }
        }

        public void Execute(object parameter)
        {
            wrappedCommand.Execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return wrappedCommand.CanExecute(parameter);
        }
    }
}
