using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Wes.Desktop.Windows.Printer;
using Wes.Utilities;
using Wes.Utilities.Languages;

namespace Wes.Desktop.Windows.Options.View
{
    public class SettingsSerialPort4PrintModel : NotifyPropertyChanged
    {
        private List<KeyValuePair<string, string>> _printerItems = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<string, string>> _serialNames = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<string, int>> _baudRates = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> _dataBits = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, string>> _paritys = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<string, string>> _stopBits = new List<KeyValuePair<string, string>>();

        public List<KeyValuePair<string, string>> PrinterItems
        {
            get { return _printerItems; }
        }

        public List<KeyValuePair<string, string>> SerialItems
        {
            get { return _serialNames; }
        }
        public List<KeyValuePair<string, int>> BaudRateItems
        {
            get { return _baudRates; }
        }
        public List<KeyValuePair<string, int>> DataBitsItems
        {
            get { return _dataBits; }
        }
        public List<KeyValuePair<string, string>> ParityItems
        {
            get { return _paritys; }
        }
        public List<KeyValuePair<string, string>> StopBitsItems
        {
            get { return _stopBits; }
        }

        private string _selectedSerialItem;
        public string SelectedSerialItem
        {
            get { return this._selectedSerialItem; }
            set
            {
                if (this._selectedSerialItem != value)
                {
                    this._selectedSerialItem = value;
                    OnPropertyChanged(nameof(SelectedSerialItem));
                }
            }
        }

        private string _selectedDataBitsItem = "8";
        public string SelectedDataBitsItem
        {
            get { return this._selectedDataBitsItem; }
            set
            {
                if (this._selectedDataBitsItem != value)
                {
                    this._selectedDataBitsItem = value;
                    OnPropertyChanged(nameof(SelectedDataBitsItem));
                }
            }
        }


        private string _selectedParityItem = "0";
        public string SelectedParityItem
        {
            get { return this._selectedParityItem; }
            set
            {
                if (this._selectedParityItem != value)
                {
                    this._selectedParityItem = value;
                    OnPropertyChanged(nameof(SelectedParityItem));
                }
            }
        }


        private string _selectedStopBitsItem = "1";
        public string SelectedStopBitsItem
        {
            get { return this._selectedStopBitsItem; }
            set
            {
                if (this._selectedStopBitsItem != value)
                {
                    this._selectedStopBitsItem = value;
                    OnPropertyChanged(nameof(SelectedStopBitsItem));
                }
            }
        }


        private string _selectedBaudRateItem = "9600";
        public string SelectedBaudRateItem
        {
            get { return this._selectedBaudRateItem; }
            set
            {
                if (this._selectedBaudRateItem != value)
                {
                    this._selectedBaudRateItem = value;
                    OnPropertyChanged(nameof(SelectedBaudRateItem));
                }
            }
        }

        private string _selectedPrinter;
        public string SelectedPrinter
        {
            get { return this._selectedPrinter; }
            set
            {
                if (this._selectedPrinter != value)
                {
                    this._selectedPrinter = value;
                    OnPropertyChanged(nameof(SelectedPrinter));
                }
            }
        }

        public ICommand CheckComCommand
        {
            get
            {
                return new WindowRelayCommand(this, OpenSerialPorts);
            }
        }

        public SettingsSerialPort4PrintModel()
        {
            InitBaseData();
        }

        private void OpenSerialPorts()
        {
            foreach (var item in _serialNames)
            {
                SerialPortManager.OpenSerialPortByCom(item.Key,
                (res, comName) =>
                {
                    LoggingService.InfoFormat("Open serialPort [{0}] result:[{1}]", comName, res);
                },
                (comName) =>
                {
                    LoggingService.InfoFormat("{0} serialport receive data", comName);
                    var serialItem = SerialPortManager.GetDefaultPortItem(comName);
                    WesApp.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.SelectedSerialItem = serialItem.Name;
                        this.SelectedBaudRateItem = serialItem.BaudRate.ToString();
                        this.SelectedDataBitsItem = serialItem.DataBits.ToString();
                        this.SelectedParityItem = serialItem.Parity.ToString();
                        this.SelectedStopBitsItem = serialItem.StopBits.ToString();
                        WesModernDialog.ShowWesMessage(string.Format("配置成功,打印机[{0}]对应红外线设备串口[{1}]",
                            _printerItems.Where(kvp => kvp.Key == SelectedPrinter).FirstOrDefault().Value, serialItem.Name));
                    }));
                });
            }
        }

        private void InitBaseData()
        {
            var printers = Wes.Print.WesPrinter.GetPrinters();
            if (printers == null || printers.Count == 0)
            {
                WesModernDialog.ShowWesMessageAsyc("獲取系統打印機列表失敗，請檢查Print Spooler服務是否開啟，或聯繫管理員");
                return;
            }
            foreach (var item in printers)
            {
                _printerItems.Add(new KeyValuePair<string, string>(item.Key.GetStringMd5String(), item.Key));
            }
            if (_printerItems.Count > 0)
                SelectedPrinter = _printerItems.ElementAt(0).Key;

            //串口号
            string[] ports = SerialPort.GetPortNames();
            if (ports != null && ports.Length > 0)
            {
                foreach (var item in ports)
                {
                    _serialNames.Add(new KeyValuePair<string, string>(item, item));
                }
            }

            //波特率
            _baudRates.Add(new KeyValuePair<string, int>("9600", 9600));
            _baudRates.Add(new KeyValuePair<string, int>("19200", 19200));
            _baudRates.Add(new KeyValuePair<string, int>("38400", 38400));
            _baudRates.Add(new KeyValuePair<string, int>("57600", 57600));
            _baudRates.Add(new KeyValuePair<string, int>("115200", 115200));

            //数据位
            _dataBits.Add(new KeyValuePair<string, int>("5", 5));
            _dataBits.Add(new KeyValuePair<string, int>("6", 6));
            _dataBits.Add(new KeyValuePair<string, int>("7", 7));
            _dataBits.Add(new KeyValuePair<string, int>("8", 8));

            //校验位
            foreach (var item in Enum.GetValues(typeof(Parity)))
            {
                _paritys.Add(new KeyValuePair<string, string>(((int)item).ToString(), ((Parity)item).GetLanguage()));
            }

            //停止位
            foreach (var item in Enum.GetValues(typeof(StopBits)))
            {
                _stopBits.Add(new KeyValuePair<string, string>(((int)item).ToString(), ((StopBits)item).GetLanguage()));
            }
        }
        public ICommand CheckEmptyCommand
        {
            get
            {
                return new WindowRelayCommand(this, EmptySerialPort);
            }
        }
        private void EmptySerialPort()
        {
            var result = WesModernDialog.ShowWesMessage("清除后,防呆功能將失效,確認要清除嗎?", "WES_Message".GetLanguage(),
               MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                this.SelectedSerialItem = null;
                this.SelectedBaudRateItem = null;
                this.SelectedDataBitsItem = null;
                this.SelectedParityItem = null;
                this.SelectedStopBitsItem = null;

                OptionConfigureService.DeleteElement("SerialPort" + this.SelectedPrinter);
            }
        }
    }

}
