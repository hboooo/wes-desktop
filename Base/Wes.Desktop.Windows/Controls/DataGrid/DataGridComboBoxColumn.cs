using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Wes.Desktop.Windows.Controls
{
    public class DataGridComboBoxColumn : System.Windows.Controls.DataGridComboBoxColumn
    {
        public DataGridComboBoxColumn()
        {
            this.EditingElementStyle = Application.Current.Resources["WesDataGridEditingComboBoxStyle"] as Style;
            this.MinWidth = 55;
        }
    }
}
