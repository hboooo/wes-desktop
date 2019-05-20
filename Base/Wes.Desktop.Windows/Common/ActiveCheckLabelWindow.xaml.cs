using FirstFloor.ModernUI.Presentation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wes.Wrapper;

namespace Wes.Desktop.Windows
{
    /// <summary>
    /// ActiveCheckLabelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ActiveCheckLabelWindow : BaseWindow
    {
        #region Properties

        public ObservableCollection<CheckLabelItem> SubStringCheckItems
        {
            get { return (ObservableCollection<CheckLabelItem>)GetValue(SubStringCheckItemsProperty); }
            set { SetValue(SubStringCheckItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubStringCheckItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubStringCheckItemsProperty =
            DependencyProperty.Register("SubStringCheckItems", typeof(ObservableCollection<CheckLabelItem>), typeof(ActiveCheckLabelWindow), new PropertyMetadata(null));


        public CheckLabelItem SelectedRow
        {
            get { return (CheckLabelItem)GetValue(SelectedRowProperty); }
            set { SetValue(SelectedRowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedRow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedRowProperty =
            DependencyProperty.Register("SelectedRow", typeof(CheckLabelItem), typeof(ActiveCheckLabelWindow), new PropertyMetadata(null));

        public string ScanBoxValue
        {
            get { return (string)GetValue(ScanBoxValueProperty); }
            set { SetValue(ScanBoxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScanBoxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScanBoxValueProperty =
            DependencyProperty.Register("ScanBoxValue", typeof(string), typeof(ActiveCheckLabelWindow), new PropertyMetadata(null));

        public ICommand ScanCommand
        {
            get { return (ICommand)GetValue(ScanCommandProperty); }
            set { SetValue(ScanCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScanCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScanCommandProperty =
            DependencyProperty.Register("ScanCommand", typeof(ICommand), typeof(ActiveCheckLabelWindow), new PropertyMetadata(null));


        public string ScanTooltip
        {
            get { return (string)GetValue(ScanTooltipProperty); }
            set { SetValue(ScanTooltipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScanTooltip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScanTooltipProperty =
            DependencyProperty.Register("ScanTooltip", typeof(string), typeof(ActiveCheckLabelWindow), new PropertyMetadata("請掃描標籤,確認打印是否正常"));

        #endregion

        public ActiveCheckLabelWindow()
        {
            InitializeComponent();
            ScanCommand = new RoutedUICommand("ScanCommand", "ScanCommand", typeof(ActiveCheckLabelWindow));
            this.CommandBindings.Add(new CommandBinding(ScanCommand, new ExecutedRoutedEventHandler(ScanLabelCommand)));
            this.SubStringCheckItems = new ObservableCollection<CheckLabelItem>();
            this.Loaded += ActiveCheckLabelWindow_Loaded;
        }

        private void ActiveCheckLabelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.barCodeFrame.Focus();
        }

        public void AddItem(Dictionary<string, string> values)
        {
            if (values != null)
            {
                foreach (var item in values)
                {
                    this.SubStringCheckItems.Add(new CheckLabelItem()
                    {
                        SubStringName = item.Key,
                        SubStringValue = item.Value,
                        IsMaster = WesDesktop.Instance.Authority.IsPrint == 1 ? true : false   //當前用戶是主管帳戶
                    });
                }
            }
        }

        private void btnLock_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SubStringCheckItems)
            {
                item.IsMaster = false;
            }
        }

        private void btnMaster_Click(object sender, RoutedEventArgs e)
        {
            if (MasterAuthorService.Authorization())
            {
                foreach (var item in SubStringCheckItems)
                {
                    item.IsMaster = true;
                }
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRow == null) return;
            SelectedRow.IsValid = true;
            SelectedRow.DisplayStringValue = SelectedRow.SubStringValue;
            if (Confirm())
            {
                this.DialogResult = true;
            }
        }

        private void checkLabelWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var query = this.SubStringCheckItems.Where(c => c.IsValid == false);
            if (query.Count() > 0)
            {
                WesModernDialog.ShowWesMessage(string.Format("還有{0}個標籤未確認,請掃碼確認", query.Count()));
                e.Cancel = true;
            }
        }

        private void ScanLabelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter == null) return;
            KeyEventArgs keyEventArgs = e.Parameter as KeyEventArgs;
            if (keyEventArgs == null) return;

            if (string.IsNullOrWhiteSpace(this.barCodeFrame.ScanBoxValue)) return;

            if (e.OriginalSource is TextBox && keyEventArgs.Key == Key.Enter)
            {
                this.barCodeFrame.SelectAll();
                var query = this.SubStringCheckItems.Where(c => this.barCodeFrame.ScanBoxValue.ToLower().Contains(c.SubStringValue.ToLower()));
                if (query.Count() > 1)
                {
                    var order = query.OrderByDescending(v => v.SubStringValue.Length);
                    var item = order.First();
                    item.IsValid = true;
                    item.DisplayStringValue = item.SubStringValue;
                }
                else if (query.Count() == 1)
                {
                    var item = query.First();
                    item.IsValid = true;
                    item.DisplayStringValue = item.SubStringValue;
                }
                else
                    ScanTooltip = "掃描錯誤,請掃描最後打印標籤";

                if (Confirm())
                {
                    this.DialogResult = true;
                }
            }
        }

        private bool Confirm()
        {
            var query = this.SubStringCheckItems.Where(c => c.IsValid == false);
            if (query.Count() > 0)
            {
                return false;
            }
            return true;
        }
    }

    public class CheckLabelItem : NotifyPropertyChanged
    {
        public string SubStringName { get; set; }

        public string SubStringValue { get; set; }

        private string _displayStringValue = "***";
        public string DisplayStringValue
        {
            get { return _displayStringValue; }
            set
            {
                if (_displayStringValue != value)
                {
                    _displayStringValue = value;
                    OnPropertyChanged(nameof(DisplayStringValue));
                }
            }
        }

        public string CheckStringValue
        {
            get
            {
                if (IsMaster)
                    return SubStringValue;
                else
                    return "";
            }
        }

        private bool _isMaster = false;

        public bool IsMaster
        {
            get { return _isMaster; }
            set
            {
                if (_isMaster != value)
                {
                    _isMaster = value;
                    OnPropertyChanged(nameof(IsMaster));
                    OnPropertyChanged(nameof(IsBtnEnabled));
                    OnPropertyChanged(nameof(CheckStringValue));
                }
            }
        }

        private bool _isValid = false;

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                    OnPropertyChanged(nameof(ValidState));
                    OnPropertyChanged(nameof(IsBtnEnabled));
                }
            }
        }

        public bool _isEnabled = false;

        public bool IsBtnEnabled
        {
            get
            {
                return !IsValid && IsMaster;
            }
        }

        public string ValidState
        {
            get
            {
                return IsValid ? Application.Current.FindResource("Wes_OK").ToString() : Application.Current.FindResource("Wes_Invalid").ToString();
            }
        }
    }
}
