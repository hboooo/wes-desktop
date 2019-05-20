using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Wes.Utilities.Extends;

namespace Wes.Desktop.Windows.DebugCommand
{
    public abstract class BaseDebugCommand : ICommand
    {
        private List<string> _parameters = null;
        private string _mainPara = null;

        protected List<string> CommandParameters
        {
            get { return _parameters; }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public abstract void Execute(object parameter);

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        protected bool ExecuteParameter(object obj, object parameter)
        {
            GetParameters(parameter);
            return ExecuteParameterCommand(obj, _mainPara);
        }

        /// <summary>
        /// 構建參數
        /// </summary>
        /// <param name="parameter"></param>
        protected virtual void GetParameters(object parameter)
        {
            _parameters = null;
            _mainPara = null;

            dynamic para = parameter as dynamic;
            string input = para.input.ToString();
            List<string> paras = input.Split(' ').ToList<string>();
            if (paras != null && paras.Count > 1)
            {
                _parameters = paras.GetRange(1, paras.Count - 1);
                _mainPara = _parameters[0];
            }
        }

        /// <summary>
        /// 執行指定參數前
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual bool BeforeExecuteParameter(List<string> parameters)
        {
            return false;
        }

        /// <summary>
        /// 執行指定參數后
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual bool AfterExecuteParameter(List<string> parameters)
        {
            return false;
        }

        /// <summary>
        /// 執行指定參數
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected virtual bool ExecuteParameterCommand(object obj, string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
                return false;

            if (BeforeExecuteParameter(_parameters)) return true;

            var methodName = "CommandParam" + parameter.Underline2Hump();
            Type type = obj.GetType();
            var method = type.GetMethod(methodName);
            if (method != null)
            {
                try
                {
                    method.Invoke(obj, new List<object>() { _parameters }.ToArray());
                }
                catch { }
                return true;
            }
            else
            {
                return AfterExecuteParameter(_parameters);
            }
        }
    }
}
