using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Wes.Desktop.Windows.Controls
{
    public class DataGridTextColumn : System.Windows.Controls.DataGridTextColumn
    {
        public DataGridTextColumn()
        {
            this.ElementStyle = Application.Current.Resources["WesDataGridTextStyle"] as Style;
            this.EditingElementStyle = Application.Current.Resources["WesDataGridEditingTextStyle"] as Style;
            this.MinWidth = 55;
        }
    }
}
