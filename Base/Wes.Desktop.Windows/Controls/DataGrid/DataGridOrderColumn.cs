using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Wes.Utilities;

namespace Wes.Desktop.Windows.Controls
{
    public class DataGridOrderColumn : System.Windows.Controls.DataGridTextColumn
    {
        public DataGridOrderColumn()
        {
            this.ElementStyle = Application.Current.Resources["WesDataGridOrderTextStyle"] as Style;
            this.IsReadOnly = true;
            this.MinWidth = 55;
        }
        
    }
}
