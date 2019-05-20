using System;
using System.Windows.Input;

namespace Wes.Desktop.Windows
{
    public class WindowRelayCommand : ICommand
    {
        private Action _execute;

        private object _viewModel;

        public WindowRelayCommand(object viewModel, Action execute)
        {
            _viewModel = viewModel;
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged = null;

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(null, null);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this._execute?.Invoke();
        }
    }

    public class WindowRelayCommand<T> : ICommand
    {
        private Action<T> _execute;
        private Func<T, bool> _canExecute;
        private bool _keepTargetAlive;

        public WindowRelayCommand(Action<T> execute, bool keepTargetAlive = false)
        {
            _execute = execute;
            _keepTargetAlive = keepTargetAlive;
        }

        public WindowRelayCommand(Action<T> execute, Func<T, bool> canExecute, bool keepTargetAlive = false)
        {
            _execute = execute;
            _canExecute = canExecute;
            _keepTargetAlive = keepTargetAlive;
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(null, null);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute.Invoke((T)parameter);
        }

        public void Execute(object parameter)
        {
            this._execute?.Invoke((T)parameter);
        }
    }
}
