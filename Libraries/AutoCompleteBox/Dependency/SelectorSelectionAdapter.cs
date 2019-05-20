using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
namespace System.Windows.Controls
{
    public class SelectorSelectionAdapter : ISelectionAdapter
    {
        private Selector _selector;

        private SelectionChangedEventHandler SelectionChangedEvent;
        public event SelectionChangedEventHandler SelectionChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.SelectionChangedEvent += (SelectionChangedEventHandler)Delegate.Combine(this.SelectionChangedEvent, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.SelectionChangedEvent -= (SelectionChangedEventHandler)Delegate.Remove(this.SelectionChangedEvent, value);
            }
        }

        private RoutedEventHandler CommitEvent;
        public event RoutedEventHandler Commit
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.CommitEvent += (RoutedEventHandler)Delegate.Combine(this.CommitEvent, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.CommitEvent -= (RoutedEventHandler)Delegate.Remove(this.CommitEvent, value);
            }
        }

        private RoutedEventHandler CancelEvent;
        public event RoutedEventHandler Cancel
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.CancelEvent += (RoutedEventHandler)Delegate.Combine(this.CancelEvent, value);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.CancelEvent -= (RoutedEventHandler)Delegate.Remove(this.CancelEvent, value);
            }
        }

        private bool IgnoringSelectionChanged
        {
            get;
            set;
        }

        public Selector SelectorControl
        {
            get
            {
                return this._selector;
            }
            set
            {
                if (this._selector != null)
                {
                    this._selector.SelectionChanged -= new SelectionChangedEventHandler(this.OnSelectionChanged);
                    this._selector.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnSelectorMouseLeftButtonUp);
                }
                this._selector = value;
                if (this._selector != null)
                {
                    this._selector.SelectionChanged += new SelectionChangedEventHandler(this.OnSelectionChanged);
                    this._selector.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSelectorMouseLeftButtonUp);
                }
            }
        }

        public object SelectedItem
        {
            get
            {
                if (this.SelectorControl != null)
                {
                    return this.SelectorControl.SelectedItem;
                }
                return null;
            }
            set
            {
                this.IgnoringSelectionChanged = true;
                if (this.SelectorControl != null)
                {
                    this.SelectorControl.SelectedItem = value;
                }
                if (value == null)
                {
                    this.ResetScrollViewer();
                }
                this.IgnoringSelectionChanged = false;
            }
        }

        public IEnumerable ItemsSource
        {
            get
            {
                if (this.SelectorControl != null)
                {
                    return this.SelectorControl.ItemsSource;
                }
                return null;
            }
            set
            {
                if (this.SelectorControl != null)
                {
                    this.SelectorControl.ItemsSource = value;
                }
            }
        }
        public SelectorSelectionAdapter()
        {
        }
        public SelectorSelectionAdapter(Selector selector)
        {
            this.SelectorControl = selector;
        }
        private void ResetScrollViewer()
        {
            if (this.SelectorControl != null)
            {
                ScrollViewer scrollViewer = this.SelectorControl.GetLogicalChildrenBreadthFirst().OfType<ScrollViewer>().FirstOrDefault<ScrollViewer>();
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToTop();
                }
            }
        }

        private void OnSelectorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.OnCommit();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IgnoringSelectionChanged)
            {
                return;
            }
            SelectionChangedEvent?.Invoke(sender, e);
        }

        protected void SelectedIndexIncrement()
        {
            if (this.SelectorControl != null)
            {
                this.SelectorControl.SelectedIndex = ((this.SelectorControl.SelectedIndex + 1 >= this.SelectorControl.Items.Count) ? -1 : (this.SelectorControl.SelectedIndex + 1));
            }
        }

        protected void SelectedIndexDecrement()
        {
            if (this.SelectorControl != null)
            {
                int selectedIndex = this.SelectorControl.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    this.SelectorControl.SelectedIndex--;
                    return;
                }
                if (selectedIndex == -1)
                {
                    this.SelectorControl.SelectedIndex = this.SelectorControl.Items.Count - 1;
                }
            }
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            Key key = e.Key;
            if (key != Key.Return)
            {
                if (key != Key.Escape)
                {
                    switch (key)
                    {
                        case Key.Up:
                            this.SelectedIndexDecrement();
                            e.Handled = true;
                            return;
                        case Key.Right:
                            break;
                        case Key.Down:
                            if ((ModifierKeys.Alt & Keyboard.Modifiers) == ModifierKeys.None)
                            {
                                this.SelectedIndexIncrement();
                                e.Handled = true;
                                return;
                            }
                            break;
                        default:
                            return;
                    }
                }
                else
                {
                    this.OnCancel();
                    e.Handled = true;
                }
                return;
            }
            this.OnCommit();
            e.Handled = true;
        }

        protected virtual void OnCommit()
        {
            this.OnCommit(this, new RoutedEventArgs());
        }

        private void OnCommit(object sender, RoutedEventArgs e)
        {
            CommitEvent?.Invoke(sender, e);
            this.AfterAdapterAction();
        }

        protected virtual void OnCancel()
        {
            this.OnCancel(this, new RoutedEventArgs());
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            CancelEvent?.Invoke(sender, e);
            this.AfterAdapterAction();
        }

        private void AfterAdapterAction()
        {
            this.IgnoringSelectionChanged = true;
            if (this.SelectorControl != null)
            {
                this.SelectorControl.SelectedItem = null;
                this.SelectorControl.SelectedIndex = -1;
            }
            this.IgnoringSelectionChanged = false;
        }

        public AutomationPeer CreateAutomationPeer()
        {
            if (this._selector == null)
            {
                return null;
            }
            return UIElementAutomationPeer.CreatePeerForElement(this._selector);
        }
    }
}
