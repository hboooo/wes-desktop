using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Wes.Desktop.Windows.Controls
{
    public class DataGrid : System.Windows.Controls.DataGrid
    {
        /// <summary>
        /// 是否啟用Copy命令
        /// </summary>
        public Boolean CopyCommandEnabled
        {
            get { return (Boolean)GetValue(CopyCommandEnabledProperty); }
            set { SetValue(CopyCommandEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CopyCommandEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CopyCommandEnabledProperty =
            DependencyProperty.Register("CopyCommandEnabled", typeof(Boolean), typeof(DataGrid), new PropertyMetadata(false, (obj, e) =>
            {
                (obj as DataGrid).BindingCopyCommand((Boolean)e.NewValue);
            }));

        public DataGrid()
        {
            this.Loaded += DataGrid_Loaded;
# if DEBUG
            BindingCopyCommand(true);
#else
            BindingCopyCommand(false);
#endif
            DataGridRowHelper.SetShowRowIndexProperty(this, true);
        }

        /// <summary>
        /// DataGrid 複製功能取消和啟用
        /// </summary>
        /// <param name="isEnabled"></param>
        public void BindingCopyCommand(bool isEnabled)
        {
            if (isEnabled)
                this.CommandBindings.Remove(new CommandBinding(ApplicationCommands.Copy));
            else
                this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, null, (o1, e1) => { e1.Handled = true; }));
        }

        private void DataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Loaded -= DataGrid_Loaded;
            var query = this.Columns.Where(c => c.GetType() == typeof(DataGridOrderColumn));
            if (query.Count() > 0)
            {
                DataGridOrderColumn column = query.FirstOrDefault() as DataGridOrderColumn;
                Binding binding = new Binding("Header");
                binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1);
                column.Binding = binding;
            }
        }


    }
}
