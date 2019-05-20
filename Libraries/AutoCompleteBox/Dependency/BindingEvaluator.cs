using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace System.Windows.Controls
{
    internal class BindingEvaluator<T> : FrameworkElement
    {
        private Binding _binding;
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(BindingEvaluator<T>), new PropertyMetadata(default(T)));
        public T Value
        {
            get
            {
                return (T)((object)base.GetValue(BindingEvaluator<T>.ValueProperty));
            }
            set
            {
                base.SetValue(BindingEvaluator<T>.ValueProperty, value);
            }
        }
        public Binding ValueBinding
        {
            get
            {
                return this._binding;
            }
            set
            {
                this._binding = value;
                base.SetBinding(BindingEvaluator<T>.ValueProperty, this._binding);
            }
        }
        public BindingEvaluator()
        {
        }
        public BindingEvaluator(Binding binding)
        {
            base.SetBinding(BindingEvaluator<T>.ValueProperty, binding);
        }
        public void ClearDataContext()
        {
            base.DataContext = null;
        }
        public T GetDynamicValue(object o, bool clearDataContext)
        {
            base.DataContext = o;
            T value = this.Value;
            if (clearDataContext)
            {
                base.DataContext = null;
            }
            return value;
        }
        public T GetDynamicValue(object o)
        {
            base.DataContext = o;
            return this.Value;
        }
    }
}
