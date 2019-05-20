using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows.Controls
{
    //[ContentProperty("ItemsSource"), StyleTypedProperty(Property = "TextBoxStyle", StyleTargetType = typeof(TextBox)), StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListBox)), TemplatePart(Name = "Popup", Type = typeof(Popup)), TemplatePart(Name = "SelectionAdapter", Type = typeof(ISelectionAdapter)), TemplatePart(Name = "Text", Type = typeof(TextBox)), TemplatePart(Name = "Selector", Type = typeof(Selector)), TemplateVisualState(Name = "Disabled", GroupName = "CommonStates"), TemplateVisualState(Name = "Normal", GroupName = "CommonStates"), TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates"), TemplateVisualState(Name = "Pressed", GroupName = "CommonStates"), TemplateVisualState(Name = "Focused", GroupName = "FocusStates"), TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates"), TemplateVisualState(Name = "PopupClosed", GroupName = "PopupStates"), TemplateVisualState(Name = "PopupOpened", GroupName = "PopupStates"), TemplateVisualState(Name = "Valid", GroupName = "ValidationStates"), TemplateVisualState(Name = "InvalidFocused", GroupName = "ValidationStates"), TemplateVisualState(Name = "InvalidUnfocused", GroupName = "ValidationStates")]
    public class AutoCompleteBox : Control, IUpdateVisualState
    {
        private const string ElementSelectionAdapter = "SelectionAdapter";
        private const string ElementSelector = "Selector";
        private const string ElementPopup = "Popup";
        private const string ElementTextBox = "Text";
        private const string ElementTextBoxStyle = "TextBoxStyle";
        private const string ElementItemContainerStyle = "ItemContainerStyle";
        private List<object> _items;
        private ObservableCollection<object> _view;
        private int _ignoreTextPropertyChange;
        private bool _ignorePropertyChange;
        private bool _ignoreTextSelectionChange;
        private bool _skipSelectedItemTextUpdate;
        private int _textSelectionStart;
        private bool _userCalledPopulate;
        private bool _popupHasOpened;
        private DispatcherTimer _delayTimer;
        private bool _allowWrite;
        private BindingEvaluator<string> _valueBindingEvaluator;
        private WeakEventListener<AutoCompleteBox, object, NotifyCollectionChangedEventArgs> _collectionChangedWeakEventListener;
        public static readonly DependencyProperty MinimumPrefixLengthProperty;
        public static readonly DependencyProperty MinimumPopulateDelayProperty;
        public static readonly DependencyProperty IsTextCompletionEnabledProperty;
        public static readonly DependencyProperty ItemTemplateProperty;
        public static readonly DependencyProperty ItemContainerStyleProperty;
        public static readonly DependencyProperty TextBoxStyleProperty;
        public static readonly DependencyProperty MaxDropDownHeightProperty;
        public static readonly DependencyProperty IsDropDownOpenProperty;
        public static readonly DependencyProperty ItemsSourceProperty;
        public static readonly DependencyProperty SelectedItemProperty;
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty SearchTextProperty;
        public static readonly DependencyProperty FilterModeProperty;
        public static readonly DependencyProperty ItemFilterProperty;
        public static readonly DependencyProperty TextFilterProperty;
        public static readonly DependencyProperty CornerRadiusProperty;

        private TextBox _text;
        private ListBox _listBox;
        private ISelectionAdapter _adapter;
        public static readonly RoutedEvent TextChangedEvent;
        public static readonly RoutedEvent PopulatingEvent;
        public static readonly RoutedEvent PopulatedEvent;
        public static readonly RoutedEvent DropDownOpeningEvent;
        public static readonly RoutedEvent DropDownOpenedEvent;
        public static readonly RoutedEvent DropDownClosingEvent;
        public static readonly RoutedEvent DropDownClosedEvent;
        public static readonly RoutedEvent SelectionChangedEvent;
        public event RoutedEventHandler TextChanged
        {
            add
            {
                base.AddHandler(AutoCompleteBox.TextChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(AutoCompleteBox.TextChangedEvent, value);
            }
        }
        public event PopulatingEventHandler Populating
        {
            add
            {
                base.AddHandler(AutoCompleteBox.PopulatingEvent, value);
            }
            remove
            {
                base.RemoveHandler(AutoCompleteBox.PopulatingEvent, value);
            }
        }
        public event PopulatedEventHandler Populated
        {
            add
            {
                base.AddHandler(AutoCompleteBox.PopulatedEvent, value);
            }
            remove
            {
                base.RemoveHandler(AutoCompleteBox.PopulatedEvent, value);
            }
        }
        public event RoutedPropertyChangingEventHandler<bool> DropDownOpening
        {
            add
            {
                base.AddHandler(AutoCompleteBox.PopulatedEvent, value);
            }
            remove
            {
                base.RemoveHandler(AutoCompleteBox.PopulatedEvent, value);
            }
        }
        public event RoutedPropertyChangedEventHandler<bool> DropDownOpened
        {
            add
            {
                base.AddHandler(AutoCompleteBox.DropDownOpenedEvent, value);
            }
            remove
            {
                base.RemoveHandler(AutoCompleteBox.DropDownOpenedEvent, value);
            }
        }
        public event RoutedPropertyChangingEventHandler<bool> DropDownClosing
        {
            add
            {
                base.AddHandler(AutoCompleteBox.DropDownClosingEvent, value);
            }
            remove
            {
                base.RemoveHandler(AutoCompleteBox.DropDownClosingEvent, value);
            }
        }
        public event RoutedPropertyChangedEventHandler<bool> DropDownClosed
        {
            add
            {
                base.AddHandler(AutoCompleteBox.DropDownClosedEvent, value);
            }
            remove
            {
                base.RemoveHandler(AutoCompleteBox.DropDownClosedEvent, value);
            }
        }
        public event SelectionChangedEventHandler SelectionChanged
        {
            add
            {
                base.AddHandler(AutoCompleteBox.SelectionChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(AutoCompleteBox.SelectionChangedEvent, value);
            }
        }
        internal InteractionHelper Interaction
        {
            get;
            set;
        }
        public int MinimumPrefixLength
        {
            get
            {
                return (int)base.GetValue(AutoCompleteBox.MinimumPrefixLengthProperty);
            }
            set
            {
                base.SetValue(AutoCompleteBox.MinimumPrefixLengthProperty, value);
            }
        }
        public int MinimumPopulateDelay
        {
            get
            {
                return (int)base.GetValue(AutoCompleteBox.MinimumPopulateDelayProperty);
            }
            set
            {
                base.SetValue(AutoCompleteBox.MinimumPopulateDelayProperty, value);
            }
        }
        public bool IsTextCompletionEnabled
        {
            get
            {
                return (bool)base.GetValue(AutoCompleteBox.IsTextCompletionEnabledProperty);
            }
            set
            {
                base.SetValue(AutoCompleteBox.IsTextCompletionEnabledProperty, value);
            }
        }
        public DataTemplate ItemTemplate
        {
            get
            {
                return base.GetValue(AutoCompleteBox.ItemTemplateProperty) as DataTemplate;
            }
            set
            {
                base.SetValue(AutoCompleteBox.ItemTemplateProperty, value);
            }
        }
        public Style ItemContainerStyle
        {
            get
            {
                return base.GetValue(AutoCompleteBox.ItemContainerStyleProperty) as Style;
            }
            set
            {
                base.SetValue(AutoCompleteBox.ItemContainerStyleProperty, value);
            }
        }
        public Style TextBoxStyle
        {
            get
            {
                return base.GetValue(AutoCompleteBox.TextBoxStyleProperty) as Style;
            }
            set
            {
                base.SetValue(AutoCompleteBox.TextBoxStyleProperty, value);
            }
        }
        public double MaxDropDownHeight
        {
            get
            {
                return (double)base.GetValue(AutoCompleteBox.MaxDropDownHeightProperty);
            }
            set
            {
                base.SetValue(AutoCompleteBox.MaxDropDownHeightProperty, value);
            }
        }
        public bool IsDropDownOpen
        {
            get
            {
                return (bool)base.GetValue(AutoCompleteBox.IsDropDownOpenProperty);
            }
            set
            {
                base.SetValue(AutoCompleteBox.IsDropDownOpenProperty, value);
            }
        }
        public IEnumerable ItemsSource
        {
            get
            {
                return base.GetValue(AutoCompleteBox.ItemsSourceProperty) as IEnumerable;
            }
            set
            {
                base.SetValue(AutoCompleteBox.ItemsSourceProperty, value);
            }
        }
        public CornerRadius CornerRadius
        {
            get
            {
                return (CornerRadius)base.GetValue(AutoCompleteBox.CornerRadiusProperty);
            }
            set
            {
                base.SetValue(AutoCompleteBox.CornerRadiusProperty, value);
            }
        }
        public object SelectedItem
        {
            get
            {
                return base.GetValue(AutoCompleteBox.SelectedItemProperty);
            }
            set
            {
                base.SetValue(AutoCompleteBox.SelectedItemProperty, value);
            }
        }
        public string Text
        {
            get
            {
                return base.GetValue(AutoCompleteBox.TextProperty) as string;
            }
            set
            {
                base.SetValue(AutoCompleteBox.TextProperty, value);
            }
        }
        public string SearchText
        {
            get
            {
                return (string)base.GetValue(AutoCompleteBox.SearchTextProperty);
            }
            private set
            {
                try
                {
                    this._allowWrite = true;
                    base.SetValue(AutoCompleteBox.SearchTextProperty, value);
                }
                finally
                {
                    this._allowWrite = false;
                }
            }
        }
        public AutoCompleteFilterMode FilterMode
        {
            get
            {
                return (AutoCompleteFilterMode)base.GetValue(AutoCompleteBox.FilterModeProperty);
            }
            set
            {
                base.SetValue(AutoCompleteBox.FilterModeProperty, value);
            }
        }
        public AutoCompleteFilterPredicate<object> ItemFilter
        {
            get
            {
                return base.GetValue(AutoCompleteBox.ItemFilterProperty) as AutoCompleteFilterPredicate<object>;
            }
            set
            {
                base.SetValue(AutoCompleteBox.ItemFilterProperty, value);
            }
        }
        public AutoCompleteFilterPredicate<string> TextFilter
        {
            get
            {
                return base.GetValue(AutoCompleteBox.TextFilterProperty) as AutoCompleteFilterPredicate<string>;
            }
            set
            {
                base.SetValue(AutoCompleteBox.TextFilterProperty, value);
            }
        }
        private PopupHelper DropDownPopup
        {
            get;
            set;
        }
        public TextBox TextBox
        {
            get
            {
                return this._text;
            }
            set
            {
                if (this._text != null)
                {
                    this._text.SelectionChanged -= new RoutedEventHandler(this.OnTextBoxSelectionChanged);
                    this._text.TextChanged -= new TextChangedEventHandler(this.OnTextBoxTextChanged);
                }
                this._text = value;
                if (this._text != null)
                {
                    this._text.SelectionChanged += new RoutedEventHandler(this.OnTextBoxSelectionChanged);
                    this._text.TextChanged += new TextChangedEventHandler(this.OnTextBoxTextChanged);
                    if (this.Text != null)
                    {
                        this.UpdateTextValue(this.Text);
                    }
                }
            }
        }
        internal ListBox ListBox
        {
            get
            {
                return this._listBox;
            }
            set
            {
                this._listBox = value;
            }
        }
        protected internal ISelectionAdapter SelectionAdapter
        {
            get
            {
                return this._adapter;
            }
            set
            {
                if (this._adapter != null)
                {
                    this._adapter.SelectionChanged -= new SelectionChangedEventHandler(this.OnAdapterSelectionChanged);
                    this._adapter.Commit -= new RoutedEventHandler(this.OnAdapterSelectionComplete);
                    this._adapter.Cancel -= new RoutedEventHandler(this.OnAdapterSelectionCanceled);
                    this._adapter.Cancel -= new RoutedEventHandler(this.OnAdapterSelectionComplete);
                    this._adapter.ItemsSource = null;
                }
                this._adapter = value;
                if (this._adapter != null)
                {
                    this._adapter.SelectionChanged += new SelectionChangedEventHandler(this.OnAdapterSelectionChanged);
                    this._adapter.Commit += new RoutedEventHandler(this.OnAdapterSelectionComplete);
                    this._adapter.Cancel += new RoutedEventHandler(this.OnAdapterSelectionCanceled);
                    this._adapter.Cancel += new RoutedEventHandler(this.OnAdapterSelectionComplete);
                    this._adapter.ItemsSource = this._view;
                }
            }
        }
        public Binding ValueMemberBinding
        {
            get
            {
                if (this._valueBindingEvaluator == null)
                {
                    return null;
                }
                return this._valueBindingEvaluator.ValueBinding;
            }
            set
            {
                this._valueBindingEvaluator = new BindingEvaluator<string>(value);
            }
        }
        public string ValueMemberPath
        {
            get
            {
                if (this.ValueMemberBinding == null)
                {
                    return null;
                }
                return this.ValueMemberBinding.Path.Path;
            }
            set
            {
                this.ValueMemberBinding = ((value == null) ? null : new Binding(value));
            }
        }
        private static void OnMinimumPrefixLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int num = (int)e.NewValue;
            if (num < 0 && num != -1)
            {
                throw new ArgumentOutOfRangeException("MinimumPrefixLength");
            }
        }
        private static void OnMinimumPopulateDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            if (autoCompleteBox._ignorePropertyChange)
            {
                autoCompleteBox._ignorePropertyChange = false;
                return;
            }
            int num = (int)e.NewValue;
            if (num < 0)
            {
                autoCompleteBox._ignorePropertyChange = true;
                d.SetValue(e.Property, e.OldValue);
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "MinimumPopulateDelayProperty value is invalid"));
            }
            if (autoCompleteBox._delayTimer != null)
            {
                autoCompleteBox._delayTimer.Stop();
                if (num == 0)
                {
                    autoCompleteBox._delayTimer = null;
                }
            }
            if (num > 0 && autoCompleteBox._delayTimer == null)
            {
                autoCompleteBox._delayTimer = new DispatcherTimer();
                autoCompleteBox._delayTimer.Tick += new EventHandler(autoCompleteBox.PopulateDropDown);
            }
            if (num > 0 && autoCompleteBox._delayTimer != null)
            {
                autoCompleteBox._delayTimer.Interval = TimeSpan.FromMilliseconds((double)num);
            }
        }
        private static void OnMaxDropDownHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            if (autoCompleteBox._ignorePropertyChange)
            {
                autoCompleteBox._ignorePropertyChange = false;
                return;
            }
            double num = (double)e.NewValue;
            if (num < 0.0)
            {
                autoCompleteBox._ignorePropertyChange = true;
                autoCompleteBox.SetValue(e.Property, e.OldValue);
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "MaxDropDownHeightProperty value is invalid"));
            }
            autoCompleteBox.OnMaxDropDownHeightChanged(num);
        }
        private static void OnIsDropDownOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            if (autoCompleteBox._ignorePropertyChange)
            {
                autoCompleteBox._ignorePropertyChange = false;
                return;
            }
            bool oldValue = (bool)e.OldValue;
            bool flag = (bool)e.NewValue;
            if (flag)
            {
                autoCompleteBox.TextUpdated(autoCompleteBox.Text, true);
            }
            else
            {
                autoCompleteBox.ClosingDropDown(oldValue);
            }
            autoCompleteBox.UpdateVisualState(true);
        }
        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            autoCompleteBox.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }
        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            if (autoCompleteBox._ignorePropertyChange)
            {
                autoCompleteBox._ignorePropertyChange = false;
                return;
            }
            if (autoCompleteBox._skipSelectedItemTextUpdate)
            {
                autoCompleteBox._skipSelectedItemTextUpdate = false;
            }
            else
            {
                autoCompleteBox.OnSelectedItemChanged(e.NewValue);
            }
            List<object> list = new List<object>();
            if (e.OldValue != null)
            {
                list.Add(e.OldValue);
            }
            List<object> list2 = new List<object>();
            if (e.NewValue != null)
            {
                list2.Add(e.NewValue);
            }
            autoCompleteBox.OnSelectionChanged(new SelectionChangedEventArgs(AutoCompleteBox.SelectionChangedEvent, list, list2));
        }
        private void OnSelectedItemChanged(object newItem)
        {
            string value;
            if (newItem == null)
            {
                value = this.SearchText;
            }
            else
            {
                value = this.FormatValue(newItem, true);
            }
            this.UpdateTextValue(value);
            if (this.TextBox != null && this.Text != null)
            {
                this.TextBox.SelectionStart = this.Text.Length;
            }
        }
        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            autoCompleteBox.TextUpdated((string)e.NewValue, false);
        }
        private static void OnSearchTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            if (autoCompleteBox._ignorePropertyChange)
            {
                autoCompleteBox._ignorePropertyChange = false;
                return;
            }
            if (!autoCompleteBox._allowWrite)
            {
                autoCompleteBox._ignorePropertyChange = true;
                autoCompleteBox.SetValue(e.Property, e.OldValue);
                throw new InvalidOperationException("SearchTextPropertyChanged value is invalid");
            }
        }
        private static void OnFilterModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            AutoCompleteFilterMode autoCompleteFilterMode = (AutoCompleteFilterMode)e.NewValue;
            if (autoCompleteFilterMode != AutoCompleteFilterMode.Contains && autoCompleteFilterMode != AutoCompleteFilterMode.ContainsCaseSensitive && autoCompleteFilterMode != AutoCompleteFilterMode.ContainsOrdinal && autoCompleteFilterMode != AutoCompleteFilterMode.ContainsOrdinalCaseSensitive && autoCompleteFilterMode != AutoCompleteFilterMode.Custom && autoCompleteFilterMode != AutoCompleteFilterMode.Equals && autoCompleteFilterMode != AutoCompleteFilterMode.EqualsCaseSensitive && autoCompleteFilterMode != AutoCompleteFilterMode.EqualsOrdinal && autoCompleteFilterMode != AutoCompleteFilterMode.EqualsOrdinalCaseSensitive && autoCompleteFilterMode != AutoCompleteFilterMode.None && autoCompleteFilterMode != AutoCompleteFilterMode.StartsWith && autoCompleteFilterMode != AutoCompleteFilterMode.StartsWithCaseSensitive && autoCompleteFilterMode != AutoCompleteFilterMode.StartsWithOrdinal && autoCompleteFilterMode != AutoCompleteFilterMode.StartsWithOrdinalCaseSensitive)
            {
                autoCompleteBox.SetValue(e.Property, e.OldValue);
                throw new ArgumentException("OnFilterModeProperty value is invalid");
            }
            AutoCompleteFilterMode filterMode = (AutoCompleteFilterMode)e.NewValue;
            autoCompleteBox.TextFilter = AutoCompleteSearch.GetFilter(filterMode);
        }
        private static void OnItemFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteBox autoCompleteBox = d as AutoCompleteBox;
            if (!(e.NewValue is AutoCompleteFilterPredicate<object>))
            {
                autoCompleteBox.FilterMode = AutoCompleteFilterMode.None;
                return;
            }
            autoCompleteBox.FilterMode = AutoCompleteFilterMode.Custom;
            autoCompleteBox.TextFilter = null;
        }
        static AutoCompleteBox()
        {
            AutoCompleteBox.MinimumPrefixLengthProperty = DependencyProperty.Register("MinimumPrefixLength", typeof(int), typeof(AutoCompleteBox), new PropertyMetadata(1, new PropertyChangedCallback(AutoCompleteBox.OnMinimumPrefixLengthPropertyChanged)));
            AutoCompleteBox.MinimumPopulateDelayProperty = DependencyProperty.Register("MinimumPopulateDelay", typeof(int), typeof(AutoCompleteBox), new PropertyMetadata(new PropertyChangedCallback(AutoCompleteBox.OnMinimumPopulateDelayPropertyChanged)));
            AutoCompleteBox.IsTextCompletionEnabledProperty = DependencyProperty.Register("IsTextCompletionEnabled", typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false, null));
            AutoCompleteBox.ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(AutoCompleteBox), new PropertyMetadata(null));
            AutoCompleteBox.ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(AutoCompleteBox), new PropertyMetadata(null, null));
            AutoCompleteBox.TextBoxStyleProperty = DependencyProperty.Register("TextBoxStyle", typeof(Style), typeof(AutoCompleteBox), new PropertyMetadata(null));
            AutoCompleteBox.MaxDropDownHeightProperty = DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(AutoCompleteBox), new PropertyMetadata(double.PositiveInfinity, new PropertyChangedCallback(AutoCompleteBox.OnMaxDropDownHeightPropertyChanged)));
            AutoCompleteBox.IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false, new PropertyChangedCallback(AutoCompleteBox.OnIsDropDownOpenPropertyChanged)));
            AutoCompleteBox.ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AutoCompleteBox), new PropertyMetadata(new PropertyChangedCallback(AutoCompleteBox.OnItemsSourcePropertyChanged)));
            AutoCompleteBox.CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(AutoCompleteBox), new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
            AutoCompleteBox.SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteBox), new PropertyMetadata(new PropertyChangedCallback(AutoCompleteBox.OnSelectedItemPropertyChanged)));
            AutoCompleteBox.TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(string.Empty, new PropertyChangedCallback(AutoCompleteBox.OnTextPropertyChanged)));
            AutoCompleteBox.SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(string.Empty, new PropertyChangedCallback(AutoCompleteBox.OnSearchTextPropertyChanged)));
            AutoCompleteBox.FilterModeProperty = DependencyProperty.Register("FilterMode", typeof(AutoCompleteFilterMode), typeof(AutoCompleteBox), new PropertyMetadata(AutoCompleteFilterMode.StartsWith, new PropertyChangedCallback(AutoCompleteBox.OnFilterModePropertyChanged)));
            AutoCompleteBox.ItemFilterProperty = DependencyProperty.Register("ItemFilter", typeof(AutoCompleteFilterPredicate<object>), typeof(AutoCompleteBox), new PropertyMetadata(new PropertyChangedCallback(AutoCompleteBox.OnItemFilterPropertyChanged)));
            AutoCompleteBox.TextFilterProperty = DependencyProperty.Register("TextFilter", typeof(AutoCompleteFilterPredicate<string>), typeof(AutoCompleteBox), new PropertyMetadata(AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith)));
            AutoCompleteBox.TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AutoCompleteBox));
            AutoCompleteBox.PopulatingEvent = EventManager.RegisterRoutedEvent("Populating", RoutingStrategy.Bubble, typeof(PopulatingEventHandler), typeof(AutoCompleteBox));
            AutoCompleteBox.PopulatedEvent = EventManager.RegisterRoutedEvent("Populated", RoutingStrategy.Bubble, typeof(PopulatedEventHandler), typeof(AutoCompleteBox));
            AutoCompleteBox.DropDownOpeningEvent = EventManager.RegisterRoutedEvent("DropDownOpening", RoutingStrategy.Bubble, typeof(RoutedPropertyChangingEventHandler<bool>), typeof(AutoCompleteBox));
            AutoCompleteBox.DropDownOpenedEvent = EventManager.RegisterRoutedEvent("DropDownOpened", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(AutoCompleteBox));
            AutoCompleteBox.DropDownClosingEvent = EventManager.RegisterRoutedEvent("DropDownClosing", RoutingStrategy.Bubble, typeof(RoutedPropertyChangingEventHandler<bool>), typeof(AutoCompleteBox));
            AutoCompleteBox.DropDownClosedEvent = EventManager.RegisterRoutedEvent("DropDownClosed", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(AutoCompleteBox));
            AutoCompleteBox.SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(AutoCompleteBox));
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox), new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }
        public AutoCompleteBox()
        {
            base.IsEnabledChanged += new DependencyPropertyChangedEventHandler(this.ControlIsEnabledChanged);
            this.Interaction = new InteractionHelper(this);
            InitializeStyle();
            this.ClearView();
        }
        private void InitializeStyle()
        {
            var Resource = new ResourceDictionary();
            Resource.Source = new Uri(@"pack://application:,,,/AutoCompleteBox;component/AutoCompleteBox.xaml", UriKind.Absolute);
            Style = (Style)Resource["DefaultAutoCompleteBoxStyle"];
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size result = base.ArrangeOverride(finalSize);
            if (this.DropDownPopup != null)
            {
                this.DropDownPopup.Arrange();
            }
            return result;
        }
        public override void OnApplyTemplate()
        {
            if (this.TextBox != null)
            {
                this.TextBox.PreviewKeyDown -= new KeyEventHandler(this.OnTextBoxPreviewKeyDown);
            }
            if (this.DropDownPopup != null)
            {
                this.DropDownPopup.Closed -= new EventHandler(this.DropDownPopup_Closed);
                this.DropDownPopup.FocusChanged -= new EventHandler(this.OnDropDownFocusChanged);
                this.DropDownPopup.UpdateVisualStates -= new EventHandler(this.OnDropDownPopupUpdateVisualStates);
                this.DropDownPopup.BeforeOnApplyTemplate();
                this.DropDownPopup = null;
            }
            base.OnApplyTemplate();
            Popup popup = base.GetTemplateChild("Popup") as Popup;
            if (popup != null)
            {
                this.DropDownPopup = new PopupHelper(this, popup);
                this.DropDownPopup.MaxDropDownHeight = this.MaxDropDownHeight;
                this.DropDownPopup.AfterOnApplyTemplate();
                this.DropDownPopup.Closed += new EventHandler(this.DropDownPopup_Closed);
                this.DropDownPopup.FocusChanged += new EventHandler(this.OnDropDownFocusChanged);
                this.DropDownPopup.UpdateVisualStates += new EventHandler(this.OnDropDownPopupUpdateVisualStates);
            }
            this.SelectionAdapter = this.GetSelectionAdapterPart();
            this.TextBox = (base.GetTemplateChild("Text") as TextBox);
            if (this.TextBox != null)
            {
                this.TextBox.PreviewKeyDown += new KeyEventHandler(this.OnTextBoxPreviewKeyDown);
            }
            this.ListBox = (base.GetTemplateChild("Selector") as ListBox);
            this.Interaction.OnApplyTemplateBase();
            if (this.IsDropDownOpen && this.DropDownPopup != null && !this.DropDownPopup.IsOpen)
            {
                this.OpeningDropDown(false);
            }
        }
        private void OnDropDownPopupUpdateVisualStates(object sender, EventArgs e)
        {
            this.UpdateVisualState(true);
        }
        private void OnDropDownFocusChanged(object sender, EventArgs e)
        {
            this.FocusChanged(this.HasFocus());
        }
        private void ClosingDropDown(bool oldValue)
        {
            bool flag = false;
            if (this.DropDownPopup != null)
            {
                flag = this.DropDownPopup.UsesClosingVisualState;
            }
            RoutedPropertyChangingEventArgs<bool> routedPropertyChangingEventArgs = new RoutedPropertyChangingEventArgs<bool>(AutoCompleteBox.IsDropDownOpenProperty, oldValue, false, true, AutoCompleteBox.DropDownClosingEvent);
            this.OnDropDownClosing(routedPropertyChangingEventArgs);
            if (this._view == null || this._view.Count == 0)
            {
                flag = false;
            }
            if (routedPropertyChangingEventArgs.Cancel)
            {
                this._ignorePropertyChange = true;
                base.SetValue(AutoCompleteBox.IsDropDownOpenProperty, oldValue);
            }
            else
            {
                this.RaiseExpandCollapseAutomationEvent(oldValue, false);
                if (!flag)
                {
                    this.CloseDropDown(oldValue, false);
                }
            }
            this.UpdateVisualState(true);
        }
        private void OpeningDropDown(bool oldValue)
        {
            RoutedPropertyChangingEventArgs<bool> routedPropertyChangingEventArgs = new RoutedPropertyChangingEventArgs<bool>(AutoCompleteBox.IsDropDownOpenProperty, oldValue, true, true, AutoCompleteBox.DropDownOpeningEvent);
            this.OnDropDownOpening(routedPropertyChangingEventArgs);
            if (routedPropertyChangingEventArgs.Cancel)
            {
                this._ignorePropertyChange = true;
                base.SetValue(AutoCompleteBox.IsDropDownOpenProperty, oldValue);
            }
            else
            {
                this.RaiseExpandCollapseAutomationEvent(oldValue, true);
                this.OpenDropDown(oldValue, true);
            }
            this.UpdateVisualState(true);
        }
        private void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
        {
            AutoCompleteBoxAutomationPeer autoCompleteBoxAutomationPeer = UIElementAutomationPeer.FromElement(this) as AutoCompleteBoxAutomationPeer;
            if (autoCompleteBoxAutomationPeer != null)
            {
                autoCompleteBoxAutomationPeer.RaiseExpandCollapseAutomationEvent(oldValue, newValue);
            }
        }
        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }
        private void DropDownPopup_Closed(object sender, EventArgs e)
        {
            if (this.IsDropDownOpen)
            {
                this.IsDropDownOpen = false;
            }
            if (this._popupHasOpened)
            {
                this.OnDropDownClosed(new RoutedPropertyChangedEventArgs<bool>(true, false, AutoCompleteBox.DropDownClosedEvent));
            }
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AutoCompleteBoxAutomationPeer(this);
        }
        private void FocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                if (this.TextBox != null && this.TextBox.SelectionLength == 0)
                {
                    this.TextBox.SelectAll();
                    return;
                }
            }
            else
            {
                this.IsDropDownOpen = false;
                this._userCalledPopulate = false;
                if (this.TextBox != null)
                {
                    this.TextBox.Select(this.TextBox.Text.Length, 0);
                }
            }
        }
        protected bool HasFocus()
        {
            DependencyObject parent;
            for (DependencyObject dependencyObject = base.IsKeyboardFocusWithin ? (Keyboard.FocusedElement as DependencyObject) : (FocusManager.GetFocusedElement(this) as DependencyObject); dependencyObject != null; dependencyObject = parent)
            {
                if (object.ReferenceEquals(dependencyObject, this))
                {
                    return true;
                }
                parent = VisualTreeHelper.GetParent(dependencyObject);
                if (parent == null)
                {
                    FrameworkElement frameworkElement = dependencyObject as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        parent = frameworkElement.Parent;
                    }
                }
            }
            return false;
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            this.FocusChanged(this.HasFocus());
        }
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            this.FocusChanged((bool)e.NewValue);
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            this.FocusChanged(this.HasFocus());
        }
        private void ControlIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                this.IsDropDownOpen = false;
            }
        }
        protected virtual ISelectionAdapter GetSelectionAdapterPart()
        {
            ISelectionAdapter selectionAdapter = null;
            Selector selector = base.GetTemplateChild("Selector") as Selector;
            if (selector != null)
            {
                selectionAdapter = (selector as ISelectionAdapter);
                if (selectionAdapter == null)
                {
                    selectionAdapter = new SelectorSelectionAdapter(selector);
                }
            }
            if (selectionAdapter == null)
            {
                selectionAdapter = (base.GetTemplateChild("SelectionAdapter") as ISelectionAdapter);
            }
            return selectionAdapter;
        }
        private void PopulateDropDown(object sender, EventArgs e)
        {
            if (this._delayTimer != null)
            {
                this._delayTimer.Stop();
            }
            this.SearchText = this.Text;
            PopulatingEventArgs populatingEventArgs = new PopulatingEventArgs(this.SearchText, AutoCompleteBox.PopulatingEvent);
            this.OnPopulating(populatingEventArgs);
            if (!populatingEventArgs.Cancel)
            {
                this.PopulateComplete();
            }
        }
        protected virtual void OnPopulating(PopulatingEventArgs e)
        {
            base.RaiseEvent(e);
        }
        protected virtual void OnPopulated(PopulatedEventArgs e)
        {
            base.RaiseEvent(e);
        }
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.RaiseEvent(e);
        }
        protected virtual void OnDropDownOpening(RoutedPropertyChangingEventArgs<bool> e)
        {
            base.RaiseEvent(e);
        }
        protected virtual void OnDropDownOpened(RoutedPropertyChangedEventArgs<bool> e)
        {
            base.RaiseEvent(e);
        }
        protected virtual void OnDropDownClosing(RoutedPropertyChangingEventArgs<bool> e)
        {
            base.RaiseEvent(e);
        }
        protected virtual void OnDropDownClosed(RoutedPropertyChangedEventArgs<bool> e)
        {
            base.RaiseEvent(e);
        }
        private string FormatValue(object value, bool clearDataContext)
        {
            string result = this.FormatValue(value);
            if (clearDataContext && this._valueBindingEvaluator != null)
            {
                this._valueBindingEvaluator.ClearDataContext();
            }
            return result;
        }
        protected virtual string FormatValue(object value)
        {
            if (this._valueBindingEvaluator != null)
            {
                return this._valueBindingEvaluator.GetDynamicValue(value) ?? string.Empty;
            }
            if (value != null)
            {
                return value.ToString();
            }
            return string.Empty;
        }
        protected virtual void OnTextChanged(RoutedEventArgs e)
        {
            base.RaiseEvent(e);
        }
        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            this.TextUpdated(this._text.Text, true);
        }
        private void OnTextBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this._ignoreTextSelectionChange)
            {
                return;
            }
            this._textSelectionStart = this._text.SelectionStart;
        }
        private void UpdateTextValue(string value)
        {
            this.UpdateTextValue(value, null);
        }
        private void UpdateTextValue(string value, bool? userInitiated)
        {
            if ((!userInitiated.HasValue || userInitiated == true) && this.Text != value)
            {
                this._ignoreTextPropertyChange++;
                this.Text = value;
                this.OnTextChanged(new RoutedEventArgs(AutoCompleteBox.TextChangedEvent));
            }
            if ((!userInitiated.HasValue || userInitiated == false) && this.TextBox != null && this.TextBox.Text != value)
            {
                this._ignoreTextPropertyChange++;
                this.TextBox.Text = (value ?? string.Empty);
                if (this.Text == value || this.Text == null)
                {
                    this.OnTextChanged(new RoutedEventArgs(AutoCompleteBox.TextChangedEvent));
                }
            }
        }
        private void TextUpdated(string newText, bool userInitiated)
        {
            if (this._ignoreTextPropertyChange > 0)
            {
                this._ignoreTextPropertyChange--;
                return;
            }
            if (newText == null)
            {
                newText = string.Empty;
            }
            if (this.IsTextCompletionEnabled && this.TextBox != null && this.TextBox.SelectionLength > 0 && this.TextBox.SelectionStart != this.TextBox.Text.Length)
            {
                return;
            }
            bool flag = newText.Length >= this.MinimumPrefixLength && this.MinimumPrefixLength >= 0;
            this._userCalledPopulate = (flag && userInitiated);
            this.UpdateTextValue(newText, new bool?(userInitiated));
            if (!flag)
            {
                this.SearchText = string.Empty;
                if (this.SelectedItem != null)
                {
                    this._skipSelectedItemTextUpdate = true;
                }
                this.SelectedItem = null;
                if (this.IsDropDownOpen)
                {
                    this.IsDropDownOpen = false;
                }
                return;
            }
            this._ignoreTextSelectionChange = true;
            if (this._delayTimer != null)
            {
                this._delayTimer.Start();
                return;
            }
            this.PopulateDropDown(this, EventArgs.Empty);
        }
        public void PopulateComplete()
        {
            this.RefreshView();
            PopulatedEventArgs e = new PopulatedEventArgs(new ReadOnlyCollection<object>(this._view), AutoCompleteBox.PopulatedEvent);
            this.OnPopulated(e);
            if (this.SelectionAdapter != null && this.SelectionAdapter.ItemsSource != this._view)
            {
                this.SelectionAdapter.ItemsSource = this._view;
            }
            bool flag = this._userCalledPopulate && this._view.Count > 0;
            if (flag != this.IsDropDownOpen)
            {
                this._ignorePropertyChange = true;
                this.IsDropDownOpen = flag;
            }
            if (this.IsDropDownOpen)
            {
                this.OpeningDropDown(false);
                if (this.DropDownPopup != null)
                {
                    this.DropDownPopup.Arrange();
                }
            }
            else
            {
                this.ClosingDropDown(true);
            }
            this.UpdateTextCompletion(this._userCalledPopulate);
        }
        public void SelectAll()
        {
            if (this.TextBox != null)
                this.TextBox.SelectAll();
        }
        public new bool Focus()
        {
            return this.TextBox.Focus();
        }
        private void UpdateTextCompletion(bool userInitiated)
        {
            object obj = null;
            string text = this.Text;
            if (this._view.Count > 0)
            {
                if (this.IsTextCompletionEnabled && this.TextBox != null && userInitiated)
                {
                    int length = this.TextBox.Text.Length;
                    int selectionStart = this.TextBox.SelectionStart;
                    if (selectionStart == text.Length && selectionStart > this._textSelectionStart)
                    {
                        object obj2 = (this.FilterMode == AutoCompleteFilterMode.StartsWith || this.FilterMode == AutoCompleteFilterMode.StartsWithCaseSensitive) ? this._view[0] : this.TryGetMatch(text, this._view, AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.StartsWith));
                        if (obj2 != null)
                        {
                            obj = obj2;
                            string text2 = this.FormatValue(obj2, true);
                            int length2 = Math.Min(text2.Length, this.Text.Length);
                            if (AutoCompleteSearch.Equals(this.Text.Substring(0, length2), text2.Substring(0, length2)))
                            {
                                this.UpdateTextValue(text2);
                                this.TextBox.SelectionStart = length;
                                this.TextBox.SelectionLength = text2.Length - length;
                            }
                        }
                    }
                }
                else
                {
                    obj = this.TryGetMatch(text, this._view, AutoCompleteSearch.GetFilter(AutoCompleteFilterMode.EqualsCaseSensitive));
                }
            }
            if (this.SelectedItem != obj)
            {
                this._skipSelectedItemTextUpdate = true;
            }
            this.SelectedItem = obj;
            if (this._ignoreTextSelectionChange)
            {
                this._ignoreTextSelectionChange = false;
                if (this.TextBox != null)
                {
                    this._textSelectionStart = this.TextBox.SelectionStart;
                }
            }
        }
        private object TryGetMatch(string searchText, ObservableCollection<object> view, AutoCompleteFilterPredicate<string> predicate)
        {
            if (view != null && view.Count > 0)
            {
                foreach (object current in view)
                {
                    if (predicate(searchText, this.FormatValue(current)))
                    {
                        return current;
                    }
                }
            }
            return null;
        }
        private void ClearView()
        {
            if (this._view == null)
            {
                this._view = new ObservableCollection<object>();
                return;
            }
            this._view.Clear();
        }
        private void RefreshView()
        {
            if (this._items == null)
            {
                this.ClearView();
                return;
            }
            string search = this.Text ?? string.Empty;
            bool flag = this.TextFilter != null;
            bool flag2 = this.FilterMode == AutoCompleteFilterMode.Custom && this.TextFilter == null;
            int num = 0;
            int num2 = this._view.Count;
            List<object> items = this._items;
            foreach (object current in items)
            {
                bool flag3 = !flag && !flag2;
                if (!flag3)
                {
                    flag3 = (flag ? this.TextFilter(search, this.FormatValue(current)) : this.ItemFilter(search, current));
                }
                if (num2 > num && flag3 && this._view[num] == current)
                {
                    num++;
                }
                else if (flag3)
                {
                    if (num2 > num && this._view[num] != current)
                    {
                        this._view.RemoveAt(num);
                        this._view.Insert(num, current);
                        num++;
                    }
                    else
                    {
                        if (num == num2)
                        {
                            this._view.Add(current);
                        }
                        else
                        {
                            this._view.Insert(num, current);
                        }
                        num++;
                        num2++;
                    }
                }
                else if (num2 > num && this._view[num] == current)
                {
                    this._view.RemoveAt(num);
                    num2--;
                }
            }
            if (this._valueBindingEvaluator != null)
            {
                this._valueBindingEvaluator.ClearDataContext();
            }
        }
        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            INotifyCollectionChanged notifyCollectionChanged = oldValue as INotifyCollectionChanged;
            if (notifyCollectionChanged != null && this._collectionChangedWeakEventListener != null)
            {
                this._collectionChangedWeakEventListener.Detach();
                this._collectionChangedWeakEventListener = null;
            }
            INotifyCollectionChanged newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (newValueINotifyCollectionChanged != null)
            {
                this._collectionChangedWeakEventListener = new WeakEventListener<AutoCompleteBox, object, NotifyCollectionChangedEventArgs>(this);
                this._collectionChangedWeakEventListener.OnEventAction = delegate (AutoCompleteBox instance, object source, NotifyCollectionChangedEventArgs eventArgs)
                {
                    instance.ItemsSourceCollectionChanged(source, eventArgs);
                };
                this._collectionChangedWeakEventListener.OnDetachAction = delegate (WeakEventListener<AutoCompleteBox, object, NotifyCollectionChangedEventArgs> weakEventListener)
                {
                    newValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(weakEventListener.OnEvent);
                };
                newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(this._collectionChangedWeakEventListener.OnEvent);
            }
            this._items = ((newValue == null) ? null : new List<object>(newValue.Cast<object>().ToList<object>()));
            this.ClearView();
            if (this.SelectionAdapter != null && this.SelectionAdapter.ItemsSource != this._view)
            {
                this.SelectionAdapter.ItemsSource = this._view;
            }
            if (this.IsDropDownOpen)
            {
                this.RefreshView();
            }
        }
        private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    this._items.RemoveAt(e.OldStartingIndex);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && this._items.Count >= e.NewStartingIndex)
            {
                for (int j = 0; j < e.NewItems.Count; j++)
                {
                    this._items.Insert(e.NewStartingIndex + j, e.NewItems[j]);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Replace && e.NewItems != null && e.OldItems != null)
            {
                for (int k = 0; k < e.NewItems.Count; k++)
                {
                    this._items[e.NewStartingIndex] = e.NewItems[k];
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                for (int l = 0; l < e.OldItems.Count; l++)
                {
                    this._view.Remove(e.OldItems[l]);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.ClearView();
                if (this.ItemsSource != null)
                {
                    this._items = new List<object>(this.ItemsSource.Cast<object>().ToList<object>());
                }
            }
            this.RefreshView();
        }
        private void OnAdapterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItem = this._adapter.SelectedItem;
            if (ListBox != null) ListBox.ScrollIntoView(this.SelectedItem);
        }
        private void OnAdapterSelectionComplete(object sender, RoutedEventArgs e)
        {
            this.IsDropDownOpen = false;
            this.UpdateTextCompletion(false);
            if (this.TextBox != null)
            {
                this.TextBox.Select(this.TextBox.Text.Length, 0);
            }
            if (this.TextBox != null)
            {
                Keyboard.Focus(this.TextBox);
                return;
            }
            base.Focus();
        }
        private void OnAdapterSelectionCanceled(object sender, RoutedEventArgs e)
        {
            this.UpdateTextValue(this.SearchText);
            this.UpdateTextCompletion(false);
        }
        private void OnMaxDropDownHeightChanged(double newValue)
        {
            if (this.DropDownPopup != null)
            {
                this.DropDownPopup.MaxDropDownHeight = newValue;
                this.DropDownPopup.Arrange();
            }
            this.UpdateVisualState(true);
        }
        private void OpenDropDown(bool oldValue, bool newValue)
        {
            if (this.DropDownPopup != null)
            {
                this.DropDownPopup.IsOpen = true;
            }
            this._popupHasOpened = true;
            this.OnDropDownOpened(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue, AutoCompleteBox.DropDownOpenedEvent));
        }
        private void CloseDropDown(bool oldValue, bool newValue)
        {
            if (this._popupHasOpened)
            {
                if (this.SelectionAdapter != null)
                {
                    this.SelectionAdapter.SelectedItem = null;
                }
                if (this.DropDownPopup != null)
                {
                    this.DropDownPopup.IsOpen = false;
                }
                this.OnDropDownClosed(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue, AutoCompleteBox.DropDownClosedEvent));
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            base.OnKeyDown(e);
            if (e.Handled || !base.IsEnabled)
            {
                return;
            }
            if (this.IsDropDownOpen)
            {
                if (this.SelectionAdapter != null)
                {
                    this.SelectionAdapter.HandleKeyDown(e);
                    if (e.Handled)
                    {
                        return;
                    }
                }
                if (e.Key == Key.Escape)
                {
                    this.OnAdapterSelectionCanceled(this, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                this.IsDropDownOpen = true;
                e.Handled = true;
            }
            Key key = e.Key;
            if (key == Key.Return)
            {
                this.OnAdapterSelectionComplete(this, new RoutedEventArgs());
                //e.Handled = true;   //TODO:掃描框需要enter
                return;
            }
            if (key != Key.F4)
            {
                return;
            }
            this.IsDropDownOpen = !this.IsDropDownOpen;
            e.Handled = true;
        }
        void IUpdateVisualState.UpdateVisualState(bool useTransitions)
        {
            this.UpdateVisualState(useTransitions);
        }
        internal virtual void UpdateVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, this.IsDropDownOpen ? "PopupOpened" : "PopupClosed", useTransitions);
            this.Interaction.UpdateVisualStateBase(useTransitions);
        }
    }


}
