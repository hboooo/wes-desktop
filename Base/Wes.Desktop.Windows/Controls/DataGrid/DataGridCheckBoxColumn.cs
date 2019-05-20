using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Wes.Desktop.Windows.Controls
{
    public class DataGridCheckBoxColumn : System.Windows.Controls.DataGridCheckBoxColumn
    {
        public DataGridCheckBoxColumn()
        {
            this.ElementStyle = Application.Current.Resources["WesDataGridCheckBoxStyle"] as Style;
            this.EditingElementStyle = Application.Current.Resources["WesDataGridEditingCheckBoxStyle"] as Style;
            this.MinWidth = 55;
        }
    }
}
