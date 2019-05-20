using System;
using Wes.Core.Base;

namespace Wes.Core.ViewModel
{
    public abstract class ScanActionBase<T, A>
        where T : struct
        where A : IScanAction
    {
        public ScanViewModelBase<T, A> Vm { get; private set; }

        /// <summary>
        /// 标记当前Action是否正常执行完成
        /// </summary>
        /// 
        private bool _isValid = false;

        public void SetContext(object instance)
        {
            this.Vm = instance as ScanViewModelBase<T, A>;
        }

        public virtual void BeginScan(string scanValue)
        {
        }

        /// <summary>
        /// 标记当前Action正常执行完成
        /// </summary>
        [Obsolete("请使用AbilityAbleAttribute计分")]
        public void SetActionValid()
        {
            _isValid = true;
        }

        [Obsolete("请使用AbilityAbleAttribute计分")]
        public void ClearActionValid()
        {
            _isValid = false;
        }

        [Obsolete("请使用AbilityAbleAttribute计分")]
        public bool IsActionVaild()
        {
            return _isValid;
        }
    }
}