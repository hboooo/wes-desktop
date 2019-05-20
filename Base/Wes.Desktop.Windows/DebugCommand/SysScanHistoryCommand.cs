using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Wes.Desktop.Windows.View;
using Wes.Desktop.Windows.ViewModel;
using Wes.Utilities;
using Wes.Utilities.Xml;
using Point = System.Windows.Point;

namespace Wes.Desktop.Windows.DebugCommand
{
    /// <summary>
    /// 系統調試命令符
    /// 支持自定義
    /// 新建Command繼承BaseDebugCommand
    /// 增加ExportDebugCommand特性  DebugID為命令符定義
    /// 命令定義規則:spread-xxx
    /// </summary>
    [ExportDebugCommand(DebugID = "spread-scan", Description = "show scan history")]
    public class SysScanHistoryCommand : BaseDebugCommand
    {
        public override void Execute(object parameter)
        {
            dynamic param = parameter as dynamic;
            AutoCompleteBox control = param.control as AutoCompleteBox;

            var wesHistoryShow = WindowHelper.GetOpenedWindow<ScanHistoryWindow>();
            if (wesHistoryShow == null)
            {
                string file = System.IO.Path.Combine(AppPath.DataPath, "scan.db");
                //解析xml轉對象
                List<XmlDataViewModel> list = EnvironmetService.GetEntityValues("scan", file);
                ScanHistoryWindow uploadWindow = new ScanHistoryWindow();
                //動態數據綁定
                uploadWindow.ClickAction = (value) =>
                {
                    if (control != null) control.Text = value;
                };

                uploadWindow.dataGrid.ItemsSource = list;
                //設置數據實現倒序排序
                ICollectionView v = CollectionViewSource.GetDefaultView(uploadWindow.dataGrid.ItemsSource);
                v.SortDescriptions.Clear();
                v.SortDescriptions.Add(new SortDescription("Time", System.ComponentModel.ListSortDirection.Descending));
                v.Refresh();
                //设置窗体弹框位置

                if (control != null)
                {
                    Window window = Window.GetWindow(control);
                    Point point = control.TransformToAncestor(window).Transform(new Point(0, 0));
                    uploadWindow.Left = point.X + window.Left;
                    uploadWindow.Top = point.Y + window.Top + control.ActualHeight;
                }

                uploadWindow.Show();
            }
            else
            {
                wesHistoryShow.Activate();
            }
        }
    }
}
