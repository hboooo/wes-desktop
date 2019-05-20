using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Windows.Controls
{
    public delegate void PopulatedEventHandler(object sender, PopulatedEventArgs e);

    public class PopulatedEventArgs : RoutedEventArgs
    {
        public IEnumerable Data
        {
            get;
            private set;
        }
        public PopulatedEventArgs(IEnumerable data)
        {
            this.Data = data;
        }
        public PopulatedEventArgs(IEnumerable data, RoutedEvent routedEvent) : base(routedEvent)
        {
            this.Data = data;
        }
    }
}
