using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Controls
{
    public delegate void PopulatingEventHandler(object sender, PopulatingEventArgs e);

    public class PopulatingEventArgs : RoutedEventArgs
    {
        public string Parameter
        {
            get;
            private set;
        }
        public bool Cancel
        {
            get;
            set;
        }
        public PopulatingEventArgs(string parameter)
        {
            this.Parameter = parameter;
        }
        public PopulatingEventArgs(string parameter, RoutedEvent routedEvent) : base(routedEvent)
        {
            this.Parameter = parameter;
        }
    }
}
