using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Wes.Desktop.Windows.ViewModel;
using Wes.Utilities;
using Wes.Utilities.Xml;

namespace Wes.Desktop.Windows.View
{
    /// <summary>
    /// ScanHistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScanHistoryWindow : BaseWindow
    {
        public ScanHistoryWindow()
        {
            InitializeComponent();
        }

        public Action<string> ClickAction;

        private void ButtonDelivery(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string value = button.Tag.ToString();
            if (MasterAuthorService.Authorization(VerificationType.Print))
            {
                Clipboard.SetText(value);
            }
        }
        private void datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
                {
                    var model = (XmlDataViewModel)grid.SelectedItems[0];
                    string value = model.Value;
                    ClickAction?.Invoke(value);
                    this.Close();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.F5)
            {
                string file = System.IO.Path.Combine(AppPath.DataPath, "scan.db");
                List<XmlDataViewModel> list = EnvironmetService.GetEntityValues("scan", file);
                this.dataGrid.ItemsSource = list;

                ICollectionView v = CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
                v.SortDescriptions.Clear();
                v.SortDescriptions.Add(new SortDescription("Time", System.ComponentModel.ListSortDirection.Descending));
                v.Refresh();
            }
        }
    }
}
