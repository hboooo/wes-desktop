using System;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls
{
    internal class PopupHelper
    {
        private EventHandler CloseEvent;
        public event EventHandler Closed
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.CloseEvent += (EventHandler)Delegate.Combine(this.CloseEvent, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.CloseEvent = (EventHandler)Delegate.Remove(this.CloseEvent, value);
            }
        }

        private EventHandler FocusChangedEvent;
        public event EventHandler FocusChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.FocusChangedEvent += (EventHandler)Delegate.Combine(this.FocusChangedEvent, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.FocusChangedEvent -= (EventHandler)Delegate.Remove(this.FocusChangedEvent, value);
            }
        }

        private EventHandler UpdateVisualStatesEvent;
        public event EventHandler UpdateVisualStates
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.UpdateVisualStatesEvent += (EventHandler)Delegate.Combine(this.UpdateVisualStatesEvent, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.UpdateVisualStatesEvent -= (EventHandler)Delegate.Remove(this.UpdateVisualStatesEvent, value);
            }
        }

        public bool UsesClosingVisualState
        {
            get;
            private set;
        }

        private Control Parent
        {
            get;
            set;
        }

        public double MaxDropDownHeight
        {
            get;
            set;
        }

        public Popup Popup
        {
            get;
            private set;
        }

        public bool IsOpen
        {
            get
            {
                return this.Popup.IsOpen;
            }
            set
            {
                this.Popup.IsOpen = value;
            }
        }

        private FrameworkElement PopupChild
        {
            get;
            set;
        }

        public PopupHelper(Control parent)
        {
            this.Parent = parent;
        }

        public PopupHelper(Control parent, Popup popup) : this(parent)
        {
            this.Popup = popup;
        }

        public void Arrange()
        {
            if (this.Popup == null || this.PopupChild == null || Application.Current == null)
            {
                return;
            }
            UIElement uIElement = this.Parent;
            if (Application.Current.Windows.Count > 0)
            {
                uIElement = Application.Current.Windows[0];
            }
            while (!(uIElement is Window) && uIElement != null)
            {
                uIElement = (VisualTreeHelper.GetParent(uIElement) as UIElement);
            }
            Window window = uIElement as Window;
            if (window == null)
            {
                return;
            }
            double actualWidth = window.ActualWidth;
            double actualHeight = window.ActualHeight;
            double num = this.PopupChild.ActualWidth;
            double num2 = this.PopupChild.ActualHeight;
            if (actualHeight == 0.0 || actualWidth == 0.0 || num == 0.0 || num2 == 0.0)
            {
                return;
            }
            double num3 = 0.0;
            double num4 = 0.0;
            double actualHeight2 = this.Parent.ActualHeight;
            double actualWidth2 = this.Parent.ActualWidth;
            double num5 = this.MaxDropDownHeight;
            if (double.IsInfinity(num5) || double.IsNaN(num5))
            {
                num5 = (actualHeight - actualHeight2) * 3.0 / 5.0;
            }
            num = Math.Min(num, actualWidth);
            num2 = Math.Min(num2, num5);
            num = Math.Max(actualWidth2, num);
            double num6 = num3;
            if (actualWidth < num6 + num)
            {
                num6 = actualWidth - num;
                num6 = Math.Max(0.0, num6);
            }
            bool flag = true;
            double num7 = num4 + actualHeight2;
            if (actualHeight < num7 + num2)
            {
                flag = false;
                num7 = num4 - num2;
                if (num7 < 0.0)
                {
                    if (num4 < (actualHeight - actualHeight2) / 2.0)
                    {
                        flag = true;
                        num7 = num4 + actualHeight2;
                    }
                    else
                    {
                        flag = false;
                        num7 = num4 - num2;
                    }
                }
            }
            num5 = (flag ? Math.Min(actualHeight - num7, num5) : Math.Min(num4, num5));
            this.Popup.HorizontalOffset = 0.0;
            this.Popup.VerticalOffset = 0.0;
            this.PopupChild.MinWidth = actualWidth2;
            this.PopupChild.MaxWidth = actualWidth;
            this.PopupChild.MinHeight = 0.0;
            this.PopupChild.MaxHeight = Math.Max(0.0, num5);
            this.PopupChild.Width = num;
            this.PopupChild.HorizontalAlignment = HorizontalAlignment.Left;
            this.PopupChild.VerticalAlignment = VerticalAlignment.Top;
            Canvas.SetLeft(this.PopupChild, num6 - num3);
            Canvas.SetTop(this.PopupChild, num7 - num4);
        }

        private void OnClosed(EventArgs e)
        {
            CloseEvent?.Invoke(this, e);
        }

        private void OnPopupClosedStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e != null && e.NewState != null && e.NewState.Name == "PopupClosed")
            {
                if (this.Popup != null)
                {
                    this.Popup.IsOpen = false;
                }
                this.OnClosed(EventArgs.Empty);
            }
        }

        public void BeforeOnApplyTemplate()
        {
            if (this.UsesClosingVisualState)
            {
                VisualStateGroup visualStateGroup = VisualStates.TryGetVisualStateGroup(this.Parent, "PopupStates");
                if (visualStateGroup != null)
                {
                    visualStateGroup.CurrentStateChanged -= new EventHandler<VisualStateChangedEventArgs>(this.OnPopupClosedStateChanged);
                    this.UsesClosingVisualState = false;
                }
            }
            if (this.Popup != null)
            {
                this.Popup.Closed -= new EventHandler(this.Popup_Closed);
            }
        }

        public void AfterOnApplyTemplate()
        {
            if (this.Popup != null)
            {
                this.Popup.Closed += new EventHandler(this.Popup_Closed);
            }
            VisualStateGroup visualStateGroup = VisualStates.TryGetVisualStateGroup(this.Parent, "PopupStates");
            if (visualStateGroup != null)
            {
                visualStateGroup.CurrentStateChanged += new EventHandler<VisualStateChangedEventArgs>(this.OnPopupClosedStateChanged);
                this.UsesClosingVisualState = true;
            }
            if (this.Popup != null)
            {
                this.PopupChild = (this.Popup.Child as FrameworkElement);
                if (this.PopupChild != null)
                {
                    this.PopupChild.GotFocus += new RoutedEventHandler(this.PopupChild_GotFocus);
                    this.PopupChild.LostFocus += new RoutedEventHandler(this.PopupChild_LostFocus);
                    this.PopupChild.MouseEnter += new MouseEventHandler(this.PopupChild_MouseEnter);
                    this.PopupChild.MouseLeave += new MouseEventHandler(this.PopupChild_MouseLeave);
                    this.PopupChild.SizeChanged += new SizeChangedEventHandler(this.PopupChild_SizeChanged);
                }
            }
        }

        private void PopupChild_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Arrange();
        }

        private void OutsidePopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Popup != null)
            {
                this.Popup.IsOpen = false;
            }
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            this.OnClosed(EventArgs.Empty);
        }

        private void OnFocusChanged(EventArgs e)
        {
            FocusChangedEvent?.Invoke(this, e);
        }

        private void OnUpdateVisualStates(EventArgs e)
        {
            UpdateVisualStatesEvent?.Invoke(this, e);
        }

        private void PopupChild_GotFocus(object sender, RoutedEventArgs e)
        {
            this.OnFocusChanged(EventArgs.Empty);
        }

        private void PopupChild_LostFocus(object sender, RoutedEventArgs e)
        {
            this.OnFocusChanged(EventArgs.Empty);
        }

        private void PopupChild_MouseEnter(object sender, MouseEventArgs e)
        {
            this.OnUpdateVisualStates(EventArgs.Empty);
        }

        private void PopupChild_MouseLeave(object sender, MouseEventArgs e)
        {
            this.OnUpdateVisualStates(EventArgs.Empty);
        }
    }
}
