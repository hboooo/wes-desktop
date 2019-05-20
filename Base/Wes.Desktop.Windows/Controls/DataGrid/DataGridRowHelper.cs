using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Wes.Desktop.Windows.Controls
{
    /// <summary>
    /// DataGrid序號
    /// </summary>
    public static class DataGridRowHelper
    {
        #region RowIndex

        public static readonly DependencyProperty RowIndexProperty = DependencyProperty.RegisterAttached("RowIndex", 
            typeof(bool), typeof(DataGridRowHelper), new PropertyMetadata(false, new PropertyChangedCallback(OnRowIndexPropertyChanged)));

        private static void OnRowIndexPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is DataGrid)
            {
                DataGrid dataGrid = sender as DataGrid;
                dataGrid.LoadingRow -= dataGrid_LoadingRow;
                dataGrid.UnloadingRow -= dataGrid_UnloadingRow;
                bool newValue = (bool)e.NewValue;
                if (newValue)
                {
                    dataGrid.LoadingRow += dataGrid_LoadingRow;
                    dataGrid.UnloadingRow += dataGrid_UnloadingRow;
                }
            }
        }

        static void dataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            List<DataGridRow> rows = GetRowsProperty(sender as DataGrid);
            if (rows.Contains(e.Row))
                rows.Remove(e.Row);
            foreach (DataGridRow row in rows)
                row.Header = row.GetIndex() + 1;
        }

        static void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            List<DataGridRow> rows = GetRowsProperty(sender as DataGrid);
            if (!rows.Contains(e.Row))
                rows.Add(e.Row);
            foreach (DataGridRow row in rows)
                row.Header = row.GetIndex() + 1;
        }

        public static bool GetShowRowIndexProperty(DependencyObject obj)
        {
            return (bool)obj.GetValue(RowIndexProperty);
        }

        public static void SetShowRowIndexProperty(DependencyObject obj, bool value)
        {
            obj.SetValue(RowIndexProperty, value);
        }

        #endregion

        #region Rows

        public static readonly DependencyProperty RowsProperty = DependencyProperty.RegisterAttached("Rows", 
            typeof(List<DataGridRow>), typeof(DataGridRowHelper), new PropertyMetadata(new List<DataGridRow>()));

        private static List<DataGridRow> GetRowsProperty(DependencyObject obj)
        {
            return (List<DataGridRow>)obj.GetValue(RowsProperty);
        }

        #endregion
    }
}
