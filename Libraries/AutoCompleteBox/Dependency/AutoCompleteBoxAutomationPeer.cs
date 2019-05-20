using System;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
namespace System.Windows.Automation.Peers
{
    public sealed class AutoCompleteBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, IExpandCollapseProvider, ISelectionProvider
    {
        private const string AutoCompleteBoxClassNameCore = "AutoCompleteBox";
        private AutoCompleteBox OwnerAutoCompleteBox
        {
            get
            {
                return (AutoCompleteBox)base.Owner;
            }
        }
        bool ISelectionProvider.CanSelectMultiple
        {
            get
            {
                return false;
            }
        }
        bool ISelectionProvider.IsSelectionRequired
        {
            get
            {
                return false;
            }
        }
        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get
            {
                if (!this.OwnerAutoCompleteBox.IsDropDownOpen)
                {
                    return ExpandCollapseState.Collapsed;
                }
                return ExpandCollapseState.Expanded;
            }
        }
        bool IValueProvider.IsReadOnly
        {
            get
            {
                return !this.OwnerAutoCompleteBox.IsEnabled;
            }
        }
        string IValueProvider.Value
        {
            get
            {
                return this.OwnerAutoCompleteBox.Text ?? string.Empty;
            }
        }
        public AutoCompleteBoxAutomationPeer(AutoCompleteBox owner) : base(owner)
        {
        }
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ComboBox;
        }
        protected override string GetClassNameCore()
        {
            return "AutoCompleteBox";
        }
        public override object GetPattern(PatternInterface patternInterface)
        {
            object obj = null;
            AutoCompleteBox ownerAutoCompleteBox = this.OwnerAutoCompleteBox;
            if (patternInterface == PatternInterface.Value)
            {
                obj = this;
            }
            else if (patternInterface == PatternInterface.ExpandCollapse)
            {
                obj = this;
            }
            else if (ownerAutoCompleteBox.SelectionAdapter != null)
            {
                AutomationPeer automationPeer = ownerAutoCompleteBox.SelectionAdapter.CreateAutomationPeer();
                if (automationPeer != null)
                {
                    obj = automationPeer.GetPattern(patternInterface);
                }
            }
            if (obj == null)
            {
                obj = base.GetPattern(patternInterface);
            }
            return obj;
        }
        void IExpandCollapseProvider.Expand()
        {
            if (!base.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
            this.OwnerAutoCompleteBox.IsDropDownOpen = true;
        }
        void IExpandCollapseProvider.Collapse()
        {
            if (!base.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
            this.OwnerAutoCompleteBox.IsDropDownOpen = false;
        }
        internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
        {
            base.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed, newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
        }
        void IValueProvider.SetValue(string value)
        {
            this.OwnerAutoCompleteBox.Text = value;
        }
        protected override List<AutomationPeer> GetChildrenCore()
        {
            List<AutomationPeer> list = new List<AutomationPeer>();
            AutoCompleteBox ownerAutoCompleteBox = this.OwnerAutoCompleteBox;
            TextBox textBox = ownerAutoCompleteBox.TextBox;
            if (textBox != null)
            {
                AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(textBox);
                if (automationPeer != null)
                {
                    list.Insert(0, automationPeer);
                }
            }
            if (ownerAutoCompleteBox.SelectionAdapter != null)
            {
                AutomationPeer automationPeer2 = ownerAutoCompleteBox.SelectionAdapter.CreateAutomationPeer();
                if (automationPeer2 != null)
                {
                    List<AutomationPeer> children = automationPeer2.GetChildren();
                    if (children != null)
                    {
                        foreach (AutomationPeer current in children)
                        {
                            list.Add(current);
                        }
                    }
                }
            }
            return list;
        }
        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            if (this.OwnerAutoCompleteBox.SelectionAdapter != null)
            {
                object selectedItem = this.OwnerAutoCompleteBox.SelectionAdapter.SelectedItem;
                if (selectedItem != null)
                {
                    UIElement uIElement = selectedItem as UIElement;
                    if (uIElement != null)
                    {
                        AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(uIElement);
                        if (automationPeer != null)
                        {
                            return new IRawElementProviderSimple[]
                            {
                                base.ProviderFromPeer(automationPeer)
                            };
                        }
                    }
                }
            }
            return new IRawElementProviderSimple[0];
        }
    }
}