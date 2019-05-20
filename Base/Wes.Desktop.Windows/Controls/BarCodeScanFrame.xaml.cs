using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wes.Desktop.Windows.DebugCommand;
using Wes.Desktop.Windows.ViewModel;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Languages;
using Wes.Utilities.Xml;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace Wes.Desktop.Windows.Controls
{
    /// <summary>
    /// BarCodeScanFrame.xaml 的交互逻辑
    /// </summary>
    public partial class BarCodeScanFrame : UserControl
    {
        private static readonly string _cache = System.IO.Path.Combine(AppPath.DataPath, "scan.db");
        private int _keyInIndex = 0;
        private bool _isFirstKeyDown = true;
        private int _maxCahceItem = 500; //緩存記錄個數

        #region Properties


        public int MinmumPrefixLength
        {
            get { return (int)GetValue(MinmumPrefixLengthProperty); }
            set { SetValue(MinmumPrefixLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinmumPrefixLenght.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinmumPrefixLengthProperty =
            DependencyProperty.Register("MinmumPrefixLength", typeof(int), typeof(BarCodeScanFrame), new PropertyMetadata(3));


        public Visibility ClearButtonVisibility
        {
            get { return (Visibility)GetValue(ClearButtonVisibilityProperty); }
            set { SetValue(ClearButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClearButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClearButtonVisibilityProperty =
            DependencyProperty.Register("ClearButtonVisibility", typeof(Visibility), typeof(BarCodeScanFrame), new PropertyMetadata(Visibility.Visible));


        public Visibility SupportButtonVisibility
        {
            get { return (Visibility)GetValue(SupportButtonVisibilityProperty); }
            set { SetValue(SupportButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SupportButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SupportButtonVisibilityProperty =
            DependencyProperty.Register("SupportButtonVisibility", typeof(Visibility), typeof(BarCodeScanFrame), new PropertyMetadata(Visibility.Visible));

        public Visibility HandPrintButtonVisibility
        {
            get { return (Visibility)GetValue(HandPrintButtonVisibilityProperty); }
            set { SetValue(HandPrintButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SupportButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HandPrintButtonVisibilityProperty =
            DependencyProperty.Register("HandPrintButtonVisibility", typeof(Visibility), typeof(BarCodeScanFrame), new PropertyMetadata(Visibility.Collapsed));



        public string ScanBoxValue
        {
            get { return (string)GetValue(ScanBoxValueProperty); }
            set { SetValue(ScanBoxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScanBoxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScanBoxValueProperty =
            DependencyProperty.Register("ScanBoxValue", typeof(string), typeof(BarCodeScanFrame), new PropertyMetadata(""));


        public string ScanTooltip
        {
            get { return (string)GetValue(ScanTooltipProperty); }
            set { SetValue(ScanTooltipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScanTooltip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScanTooltipProperty =
            DependencyProperty.Register("ScanTooltip", typeof(string), typeof(BarCodeScanFrame), new PropertyMetadata(null));


        public Visibility ScanTooltipVisibility
        {
            get { return (Visibility)GetValue(ScanTooltipVisibilityProperty); }
            set { SetValue(ScanTooltipVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScanTooltipVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScanTooltipVisibilityProperty =
            DependencyProperty.Register("ScanTooltipVisibility", typeof(Visibility), typeof(BarCodeScanFrame), new PropertyMetadata(Visibility.Visible));


        public ICommand CodeBoxKeyDown
        {
            get { return (ICommand)GetValue(CodeBoxKeyDownProperty); }
            set { SetValue(CodeBoxKeyDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CodeBoxKeyDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeBoxKeyDownProperty =
            DependencyProperty.Register("CodeBoxKeyDown", typeof(ICommand), typeof(BarCodeScanFrame), new PropertyMetadata(null));


        public ICommand CodeBoxClear
        {
            get { return (ICommand)GetValue(CodeBoxClearProperty); }
            set { SetValue(CodeBoxClearProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CodeBoxClear.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeBoxClearProperty =
            DependencyProperty.Register("CodeBoxClear", typeof(ICommand), typeof(BarCodeScanFrame), new PropertyMetadata(null));


        public ICommand CodeBoxSupport
        {
            get { return (ICommand)GetValue(CodeBoxSupportProperty); }
            set { SetValue(CodeBoxSupportProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CodeBoxSupport.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeBoxSupportProperty =
            DependencyProperty.Register("CodeBoxSupport", typeof(ICommand), typeof(BarCodeScanFrame), new PropertyMetadata(null));


        public ICommand CodeBoxHandPrint
        {
            get { return (ICommand)GetValue(CodeBoxHandPrintProperty); }
            set { SetValue(CodeBoxHandPrintProperty, value); }
        }

        public static readonly DependencyProperty CodeBoxHandPrintProperty =
            DependencyProperty.Register("CodeBoxHandPrint", typeof(ICommand), typeof(BarCodeScanFrame), new PropertyMetadata(null));

        public string Btn1Content
        {
            get { return (string)GetValue(Btn1ContentProperty); }
            set { SetValue(Btn1ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Btn1Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Btn1ContentProperty =
            DependencyProperty.Register("Btn1Content", typeof(string), typeof(BarCodeScanFrame), new PropertyMetadata("Clear".GetLanguage()));


        public string Btn2Content
        {
            get { return (string)GetValue(Btn2ContentProperty); }
            set { SetValue(Btn2ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Btn2Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Btn2ContentProperty =
            DependencyProperty.Register("Btn2Content", typeof(string), typeof(BarCodeScanFrame), new PropertyMetadata("Support".GetLanguage()));

        public string Btn3Content
        {
            get { return (string)GetValue(Btn3ContentProperty); }
            set { SetValue(Btn3ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Btn2Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Btn3ContentProperty =
            DependencyProperty.Register("Btn3Content", typeof(string), typeof(BarCodeScanFrame), new PropertyMetadata("Print".GetLanguage()));

        public bool UseIntelligent
        {
            get { return (bool)GetValue(UseIntelligentProperty); }
            set { SetValue(UseIntelligentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UseIntelligent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseIntelligentProperty =
            DependencyProperty.Register("UseIntelligent", typeof(bool), typeof(BarCodeScanFrame), new PropertyMetadata(false, (obj, e) =>
            {
                BarCodeScanFrame scanFrame = obj as BarCodeScanFrame;
                if ((bool)e.NewValue == true)
                {
                    scanFrame.TextScan.MinimumPrefixLength = scanFrame.MinmumPrefixLength;
                }
                else
                {
                    scanFrame.TextScan.MinimumPrefixLength = 100;
                }
            }));

        public ObservableCollection<BarCodeScanModel> IntelligentItems
        {
            get { return (ObservableCollection<BarCodeScanModel>)GetValue(IntelligentItemsProperty); }
            set { SetValue(IntelligentItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IntelligentItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntelligentItemsProperty =
            DependencyProperty.Register("IntelligentItems", typeof(ObservableCollection<BarCodeScanModel>), typeof(BarCodeScanFrame), new PropertyMetadata(null, (obj, e) =>
            {
                BarCodeScanFrame scanFrame = obj as BarCodeScanFrame;
                if (scanFrame != null && scanFrame.IntelligentItems != null && scanFrame.IntelligentItems.Count > 0)
                {
                    scanFrame.TextScan.ItemsSource = scanFrame.IntelligentItems;
                    scanFrame.TextScan.FilterMode = AutoCompleteFilterMode.StartsWith;   //设置模式匹配
                    scanFrame.TextScan.MinimumPopulateDelay = 100;                       //下拉框彈出延遲時間
                    scanFrame.TextScan.MinimumPrefixLength = scanFrame.MinmumPrefixLength;                          //輸入大於3個字符時查詢
                    scanFrame.TextScan.PopulateComplete();
                }
            }));


        #endregion

        public BarCodeScanFrame()
        {
            InitializeComponent();

            TextScan.PreviewMouseDown += TextScan_PreviewMouseDown;
            TextScan.GotFocus += TextScan_GotFocus;
            TextScan.LostFocus += TextScan_LostFocus;
        }

        private void TextScan_LostFocus(object sender, RoutedEventArgs e)
        {
            TextScan.PreviewMouseDown += TextScan_PreviewMouseDown;
        }

        private void TextScan_GotFocus(object sender, RoutedEventArgs e)
        {
            TextScan.SelectAll();
            TextScan.PreviewMouseDown -= TextScan_PreviewMouseDown;
        }

        private void TextScan_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextScan.Focus();
            e.Handled = true;
        }

        public new bool Focus()
        {
            return TextScan.Focus();
        }

        public void SelectAll()
        {
            TextScan.SelectAll();
        }

        private void TextScan_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (TextScan.IsDropDownOpen == true) return;

            switch (e.Key)
            {
                case Key.Enter:
                    KeyEnterHandler(sender, e);
                    break;
                case Key.Up:
                    KeyUpHandler(sender, e);
                    break;
                case Key.Down:
                    KeyDownHandler(sender, e);
                    break;
                default:
                    KeyAnyHandler(sender, e);
                    break;
            }
        }

        private void KeyEnterHandler(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ScanBoxValue))
            {
                return;
            }

            if (!string.IsNullOrEmpty(ScanBoxValue) && DebugCommandService.Debug(ScanBoxValue, TextScan))
            {
                ScanBoxValue = "";
                e.Handled = true;
                return;
            }
            LoggingService.Debug($"Barcode frame scan value:{ScanBoxValue}");

            XmlDataViewModel xmlDataViewModel = new XmlDataViewModel();
            string scanValue = ScanBoxValue.Trim();
            List<string> keyIns = EnvironmetService.GetValues("scan", _cache);
            if (keyIns != null && keyIns.Count >= _maxCahceItem)
            {
                EnvironmetService.RemoveFirstValue("scan", "value", _cache);
            }
            //判断主窗口还是流程窗口 region
            Window window = Window.GetWindow(this);
            if (window.GetType().Name == "MainWindow")
            {
                xmlDataViewModel.FlowID = "";
            }
            else if (window.GetType().Name == "WesFlowWindow")
            {
                Type windowType = window.GetType();
                PropertyInfo propertyInfo = windowType.GetProperty("FlowID");
                if (propertyInfo != null)
                {
                    object value = propertyInfo.GetValue(window, null);
                    xmlDataViewModel.FlowID = value == null ? "" : ((WesFlowID)Convert.ToInt32(value)).ToString();
                }
            }
            //endregion

            xmlDataViewModel.Value = scanValue;
            EnvironmetService.AppendValue("scan", "value", xmlDataViewModel, _cache);
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            List<string> keyIns = EnvironmetService.GetValues("scan", _cache);
            if (keyIns != null && keyIns.Count > 0)
            {
                _keyInIndex--;
                if (_keyInIndex < 0) _keyInIndex = keyIns.Count - 1;

                if (TextScan.IsFocused == false) this.Focus();
                ScanBoxValue = keyIns[_keyInIndex % keyIns.Count];
                TextScan.TextBox.SelectionStart = TextScan.TextBox.Text.Length;
            }
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            List<string> keyIns = EnvironmetService.GetValues("scan", _cache);
            if (keyIns != null && keyIns.Count > 0)
            {
                if (_isFirstKeyDown)
                {
                    _isFirstKeyDown = false;
                    _keyInIndex--;
                }

                _keyInIndex++;
                if (TextScan.IsFocused == false) this.Focus();
                ScanBoxValue = keyIns[_keyInIndex % keyIns.Count];
                TextScan.TextBox.SelectionStart = TextScan.TextBox.Text.Length;
            }
        }

        private void KeyAnyHandler(object sender, KeyEventArgs e)
        {
            _keyInIndex = 0;
            _isFirstKeyDown = true;
        }
    }
}
