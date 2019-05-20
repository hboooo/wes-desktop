using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    public delegate void RoutedPropertyChangingEventHandler<T>(object sender, RoutedPropertyChangingEventArgs<T> e);

    public class RoutedPropertyChangingEventArgs<T> : RoutedEventArgs
    {
        private bool _cancel;
        public DependencyProperty Property
        {
            get;
            private set;
        }
        public T OldValue
        {
            get;
            private set;
        }
        public T NewValue
        {
            get;
            set;
        }
        public bool IsCancelable
        {
            get;
            private set;
        }
        public bool Cancel
        {
            get
            {
                return this._cancel;
            }
            set
            {
                if (this.IsCancelable)
                {
                    this._cancel = value;
                    return;
                }
                if (value)
                {
                    throw new InvalidOperationException("Cancel is Fail");
                }
            }
        }
        public bool InCoercion
        {
            get;
            set;
        }
        public RoutedPropertyChangingEventArgs(DependencyProperty property, T oldValue, T newValue, bool isCancelable)
        {
            this.Property = property;
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.IsCancelable = isCancelable;
            this.Cancel = false;
        }
        public RoutedPropertyChangingEventArgs(DependencyProperty property, T oldValue, T newValue, bool isCancelable, RoutedEvent routedEvent) : base(routedEvent)
        {
            this.Property = property;
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.IsCancelable = isCancelable;
            this.Cancel = false;
        }
    }
}
