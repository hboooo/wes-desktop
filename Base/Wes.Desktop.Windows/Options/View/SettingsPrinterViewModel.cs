using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Desktop.Windows.Options.View
{
    public class SettingsPrinterViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<KeyValuePair<string, string>> _printer = new ObservableCollection<KeyValuePair<string, string>>();
        private ObservableCollection<KeyValuePair<string, string>> _consigneeItems = new ObservableCollection<KeyValuePair<string, string>>();
        private ObservableCollection<KeyValuePair<string, string>> _labelTypeItems = new ObservableCollection<KeyValuePair<string, string>>();
        private ObservableCollection<MarkLabelItem> _labelItems = new ObservableCollection<MarkLabelItem>();

        public ObservableCollection<KeyValuePair<string, string>> Printer
        {
            get { return _printer; }
        }

        public ObservableCollection<KeyValuePair<string, string>> ConsigneeItems
        {
            get { return _consigneeItems; }
        }

        public ObservableCollection<KeyValuePair<string, string>> LabelTypeItems
        {
            get { return _labelTypeItems; }
        }

        public ObservableCollection<MarkLabelItem> LabelItems
        {
            get { return _labelItems; }
        }

        private Dictionary<string, dynamic> _printerConfig = new Dictionary<string, dynamic>();

        private string _currentCustomer;

        public string CurrentCustomer
        {
            get { return _currentCustomer; }
            set
            {
                if (this._currentCustomer != value)
                {
                    this._currentCustomer = value;
                    OnPropertyChanged(nameof(CurrentCustomer));
                }
            }
        }

        private string _currentConsigneeItem;

        public string CurrentConsigneeItem
        {
            get { return _currentConsigneeItem; }
            set
            {
                if (this._currentConsigneeItem != value)
                {
                    this._currentConsigneeItem = value;
                    this.SearchTooltip = "";
                    OnPropertyChanged(nameof(CurrentConsigneeItem));
                }
            }
        }

        private string _currentLabelTypeItem;

        public string CurrentLabelTypeItem
        {
            get { return _currentLabelTypeItem; }
            set
            {
                if (this._currentLabelTypeItem != value)
                {
                    this._currentLabelTypeItem = value;
                    this.SearchTooltip = "";
                    OnPropertyChanged(nameof(CurrentLabelTypeItem));
                }
            }
        }

        private string _searchTooltip;

        public string SearchTooltip
        {
            get { return _searchTooltip; }
            set
            {
                if (this._searchTooltip != value)
                {
                    this._searchTooltip = value;
                    OnPropertyChanged(nameof(SearchTooltip));
                }
            }
        }

        public string PrinterKey
        {
            get
            {
                if (!string.IsNullOrEmpty(this.CurrentConsigneeItem) && !string.IsNullOrEmpty(this.CurrentLabelTypeItem))
                {
                    return this.CurrentConsigneeItem + ";" + this.CurrentLabelTypeItem;
                }
                return null;
            }
        }

        public string PrinterConfig
        {
            get
            {
                if (this.LabelItems != null && this.LabelItems.Count > 0)
                {
                    List<object> objList = new List<object>();
                    foreach (var item in this.LabelItems)
                    {
                        if (!string.IsNullOrEmpty(item.LabelName) && !string.IsNullOrEmpty(item.SelectedPrinter))
                        {
                            objList.Add(new
                            {
                                labelId = item.LabelID,
                                labelName = item.LabelName,
                                printer = item.SelectedPrinter,
                                isCheck = item.IsCheckLabel,
                                timespan = item.CheckTimespan
                            });
                        }
                    }
                    return DynamicJson.SerializeObject(objList);
                }
                return null;
            }
        }

        public string SelectedLabelPrintConfig
        {
            get
            {
                UpdatePrinterConfig();
                return DynamicJson.SerializeObject(_printerConfig);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _printerConfig = DynamicJson.DeserializeObject<Dictionary<string, dynamic>>(value);
                }
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new WindowRelayCommand(this, btnSearchCommand);
            }
        }

        public void UpdatePrinterConfig()
        {
            if (!string.IsNullOrEmpty(PrinterKey)
                && !string.IsNullOrEmpty(PrinterConfig))
            {
                if (_printerConfig != null) _printerConfig[PrinterKey] = PrinterConfig;
            }
        }

        public SettingsPrinterViewModel()
        {
            this.CurrentCustomer = "CurrentCustomer".GetLanguage() + "  " + WesDesktop.Instance.AddIn.EndCustomerName;
            InitPrinter();
            InitConsignee(() =>
            {
                InitLabelType();
            });
        }

        private void InitPrinter()
        {
            _printer.Clear();
            var printers = Wes.Print.WesPrinter.GetPrinters();
            if (printers != null)
            {
                foreach (var item in printers)
                {
                    _printer.Add(new KeyValuePair<string, string>(item.Key, item.Key));
                }
            }
        }

        private void InitConsignee(Action action = null)
        {
            if (String.IsNullOrEmpty(WesDesktop.Instance.AddIn.EndCustomer)) return;

            RestApi.NewInstance(Method.POST)
            .AddUriParam(RestUrlType.WmsServer, ScriptID.GET_CONSIGNEE_BY_ENDCUSTOMER, false)
            .AddJsonBody("endCustomer", WesDesktop.Instance.AddIn.EndCustomer)
            .ExecuteAsync((res, exp, restApi) =>
            {
                if (restApi != null)
                {
                    dynamic consigness = restApi.To<object>();
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.ConsigneeItems.Clear();
                        if (consigness != null)
                        {
                            foreach (var item in consigness)
                            {
                                this.ConsigneeItems.Add(new KeyValuePair<string, string>(item.code.ToString(), item.abbrName.ToString()));
                            }
                        }
                        if (this.ConsigneeItems.Count > 0)
                            CurrentConsigneeItem = ConsigneeItems.ElementAt(0).Key;
                    }));
                }
                action?.Invoke();
            });
        }

        private void InitLabelType()
        {
            RestApi.NewInstance(Method.POST)
            .AddUriParam(RestUrlType.WmsServer, ScriptID.GET_LABEL_TYPE, false)
            .ExecuteAsync((res, exp, restApi) =>
            {
                if (restApi != null)
                {
                    dynamic labelTypes = restApi.To<object>();
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.LabelTypeItems.Clear();
                        if (labelTypes != null)
                        {
                            foreach (var item in labelTypes)
                            {
                                LabelTypeItems.Add(new KeyValuePair<string, string>(item.LabelType.ToString(), item.LabelType.ToString()));
                            }
                        }
                        if (this.LabelTypeItems.Count > 0)
                            CurrentLabelTypeItem = this.LabelTypeItems.ElementAt(0).Key;
                    }));
                }
            });
        }

        private void btnSearchCommand()
        {
            if (String.IsNullOrEmpty(WesDesktop.Instance.AddIn.EndCustomer)) return;

            LabelItems.Clear();
            dynamic res = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, ScriptID.GET_LABEL_BY_ENDCUSTOMER_AND_CONSIGNEE, false)
                .AddJsonBody("endCustomer", WesDesktop.Instance.AddIn.EndCustomer)
                .AddJsonBody("consignee", CurrentConsigneeItem)
                .AddJsonBody("labelType", CurrentLabelTypeItem)
                .Execute()
                .To<object>();
            if (res != null)
            {
                foreach (var item in res)
                {
                    var labelItem = new MarkLabelItem(this.Printer)
                    {
                        LabelID = item.RowID.ToString(),
                        LabelName = item.labelName.ToString(),
                    };
                    LabelItems.Add(labelItem);

                    dynamic printerObjConfigStr = null;
                    _printerConfig.TryGetValue(this.PrinterKey, out printerObjConfigStr);
                    if (printerObjConfigStr != null)
                    {
                        dynamic printerObjConfig = DynamicJson.DeserializeObject<object>(printerObjConfigStr);
                        foreach (var config in printerObjConfig)
                        {
                            if (config.labelId == labelItem.LabelID)
                            {
                                labelItem.SelectedPrinter = config.printer;
                                labelItem.IsCheckLabel = config.isCheck;
                                labelItem.CheckTimespan = (int)config.timespan;
                                break;
                            }
                        }
                    }
                }
            }
            this.SearchTooltip = string.Format("共找到{0}個標籤", LabelItems.Count);
        }
    }

    public class MarkLabelItem : NotifyPropertyChanged
    {
        public MarkLabelItem(ObservableCollection<KeyValuePair<string, string>> items)
        {
            this.PrinterItems = items;
        }

        public string LabelName { get; set; }

        public string LabelID { get; set; }

        public ObservableCollection<KeyValuePair<string, string>> PrinterItems { get; set; }

        private string _selectedPrinter;
        public string SelectedPrinter
        {
            get { return _selectedPrinter; }
            set
            {
                if (_selectedPrinter != value)
                {
                    _selectedPrinter = value;
                    OnPropertyChanged(nameof(SelectedPrinter));
                }
            }
        }

        private bool _isCheckLabel;

        public bool IsCheckLabel
        {
            get { return _isCheckLabel; }
            set
            {
                if (_isCheckLabel != value)
                {
                    _isCheckLabel = value;
                    OnPropertyChanged(nameof(IsCheckLabel));
                }
            }
        }

        private int _timespan;

        public int CheckTimespan
        {
            get { return _timespan; }
            set
            {
                if (_timespan != value)
                {
                    _timespan = value;
                    OnPropertyChanged(nameof(CheckTimespan));
                }
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new WindowRelayCommand(this, () =>
                {
                    CheckTimespan = 0;
                    IsCheckLabel = false;
                    SelectedPrinter = null;
                });
            }
        }
    }

}
