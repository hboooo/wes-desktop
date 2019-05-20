using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Input;

namespace System.Windows.Controls
{
    public interface ISelectionAdapter
    {
        event SelectionChangedEventHandler SelectionChanged;
        event RoutedEventHandler Commit;
        event RoutedEventHandler Cancel;
        object SelectedItem
        {
            get;
            set;
        }
        IEnumerable ItemsSource
        {
            get;
            set;
        }
        void HandleKeyDown(KeyEventArgs e);
        AutomationPeer CreateAutomationPeer();
    }
}