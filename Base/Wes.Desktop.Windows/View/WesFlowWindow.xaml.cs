using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Wes.Addins.ICommand;
using Wes.Desktop.Windows.Controls;
using Wes.Flow;
using Wes.Utilities;

namespace Wes.Desktop.Windows.View
{
    /// <summary>
    /// WesFlowWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WesFlowWindow : BaseWindow
    {

        public int FlowID
        {
            get { return (int)GetValue(FlowIDProperty); }
            set { SetValue(FlowIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowIDProperty =
            DependencyProperty.Register("FlowID", typeof(int), typeof(WesFlowWindow), new PropertyMetadata(0));

        public int ChildFlowID
        {
            get
            {
                if (wesFlowTab.Items != null)
                {
                    foreach (WesTabItem item in wesFlowTab.Items)
                    {
                        if (item.IsSelected) return item.FlowID;
                    }
                }
                return 0;
            }
            set
            {
                foreach (WesTabItem item in wesFlowTab.Items)
                {
                    if (item.FlowID == value)
                    {
                        item.IsSelected = true;
                        spFlow.DataContext = (item.Content as UserControl).DataContext;
                        break;
                    }
                }
            }
        }

        public object ActionViewModel
        {
            get
            {
                foreach (WesTabItem item in wesFlowTab.Items)
                {
                    if (item.IsSelected == true)
                    {
                        return (item.Content as UserControl).DataContext;
                    }
                }
                return null;
            }
        }

        public WesFlowWindow()
        {
            InitializeComponent();
            wesFlowTab.SelectionChanged += WesFlowTab_SelectionChanged;
        }

        private void WesFlowTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                WesTabItem tabItem = e.AddedItems[0] as WesTabItem;
                if (tabItem != null)
                {
                    spFlow.DataContext = (tabItem.Content as UserControl).DataContext;
                }
            }
        }

        public void InitFlows(List<KeyValuePair<ICommandMetaData, object>> items)
        {
            if (items != null && items.Count > 0)
            {
                var ascItems = items.OrderBy(c => c.Key.CommandIndex);
                foreach (var item in ascItems)
                {
                    var tabItem = new WesTabItem()
                    {
                        FlowID = WesFlow.Instance.GetFlowID(item.Key.CommandName),
                        FlowKey = item.Key.CommandName,
                        Content = item.Value
                    };
                    Binding binding = new Binding("Value");
                    binding.Source = new Wes.Utilities.Languages.LanguageExtension(item.Key.CommandName);
                    BindingOperations.SetBinding(tabItem, WesTabItem.HeaderProperty, binding);
                    wesFlowTab.Items.Add(tabItem);
                }
            }
        }

        private void InitWorkNo(string workNo, string qrCode = null)
        {
            if (string.IsNullOrEmpty(workNo)) return;

            try
            {
                HashSet<WesFlowID> workItems = WorkNoFlowService.GetWorkNoFlow(workNo);
                var tabItems = wesFlowTab.Items;
                if (tabItems != null)
                {
                    foreach (WesTabItem item in tabItems)
                    {
                        if (workItems.Contains((WesFlowID)item.FlowID))
                        {
                            if (!string.IsNullOrEmpty(qrCode) && workNo.StartsWith("TXT") && (WesFlowID)item.FlowID == WesFlowID.FLOW_OUT_BOARDING)
                                SetValueToTextBox(item.Content as Control, qrCode);
                            else
                                SetValueToTextBox(item.Content as Control, workNo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        /// <summary>
        /// 作业流程自动填充作业单号
        /// 1.qrCode 自动填充解析出的pxt,sxt,txt,rxt到相应的流程
        /// 2.sxt，txt 同时存在时，支持sxt或txt作业的填充qrCode值。例如：组板同时支持sxt和txt组板
        /// 3.普通的pxt,sxt,txt,rxt 直接填充到相应的流程
        /// </summary>
        /// <param name="workNos"></param>
        public void InitializeWorkNo(List<string> workNos)
        {
            if (workNos == null) return;
            var querySxt = workNos.Where(s => s.StartsWith("SXT"));
            string qrCode = string.Join(",", workNos);

            foreach (var item in workNos)
            {
                if (item.StartsWith("TXT") && querySxt.Count() > 0)
                    InitWorkNo(item, qrCode);
                else
                    InitWorkNo(item);
            }
        }

        private void SetValueToTextBox(Control viewControl, string workNo)
        {
            Control control = Utilities.VisualTreeHelper.GetChildObject<BarCodeScanFrame>(viewControl, null) as Control;
            if (control == null)
            {
                control = Utilities.VisualTreeHelper.GetLogicalTreeObject<BarCodeScanFrame>(viewControl) as Control;
            }
            if (control != null)
            {
                Control textBox = control.FindName("TextScan") as Control;
                Type type = textBox.GetType();
                if (type != null)
                {
                    PropertyInfo propertyInfo = type.GetProperty("Text");
                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(textBox, workNo, null);
                    }

                    MethodInfo methodInfo = type.GetMethod("SelectAll");
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(textBox, null);
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            try
            {
                foreach (var item in this.wesFlowTab.Items)
                {
                    WesTabItem tabItem = item as WesTabItem;
                    tabItem.Content = null;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("dispose tabItem failed.", ex);
            }
        }
    }

}
