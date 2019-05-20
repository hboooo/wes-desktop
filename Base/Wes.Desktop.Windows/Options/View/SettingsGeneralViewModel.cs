using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Wes.Print;
using Wes.Utilities;
using Wes.Wrapper;
using static Wes.Utilities.WindowsAPI;

namespace Wes.Desktop.Windows.Options.View
{
    public class SettingsGeneralViewModel : NotifyPropertyChanged
    {
        private SettingsGeneral _settingsGeneral = null;

        public SettingsGeneralViewModel(SettingsGeneral settingsGeneral)
        {
            _settingsGeneral = settingsGeneral;
        }

        private bool _selectedDisplayLabelImage = true;

        public bool SelectedDisplayLabelImage
        {
            get { return this._selectedDisplayLabelImage; }
            set
            {
                if (this._selectedDisplayLabelImage != value)
                {
                    this._selectedDisplayLabelImage = value;
                    OnPropertyChanged(nameof(SelectedDisplayLabelImage));
                }
            }
        }

        private string _port = "12339";
        public string SelectedPort
        {
            get { return this._port; }
            set
            {
                if (this._port != value)
                {
                    this._port = value;
                    OnPropertyChanged(nameof(SelectedPort));
                }
            }
        }

        private string _alarmDeviceIp = "172.16.3.4";
        public string SelectedAlarmDeviceIP
        {
            get { return _alarmDeviceIp; }
            set
            {
                if (this._alarmDeviceIp != value)
                {
                    this._alarmDeviceIp = value;
                    OnPropertyChanged(nameof(SelectedAlarmDeviceIP));
                }
            }
        }

        private string _alarmDevicePort = "80";
        public string SelectedAlarmDevicePort
        {
            get { return _alarmDevicePort; }
            set
            {
                if (this._alarmDevicePort != value)
                {
                    this._alarmDevicePort = value;
                    OnPropertyChanged(nameof(SelectedAlarmDevicePort));
                }
            }
        }

        public ICommand StartServerCommand
        {
            get
            {
                return new WindowRelayCommand(this, StartServer);
            }
        }

        public ICommand StopServerCommand
        {
            get
            {
                return new WindowRelayCommand(this, StopServer);
            }
        }

        public ICommand TestPrintCommand
        {
            get
            {
                return new WindowRelayCommand(this, TestPrint);
            }
        }

        public ICommand TestLocalPrintCommand
        {
            get
            {
                return new WindowRelayCommand(this, TestLocalPrint);
            }
        }

        private void StartServer()
        {
            try
            {
                string server = Path.Combine(AppPath.BasePath, $"{WesApp.WES_SERVER}.exe");
                if (File.Exists(server))
                {
                    _settingsGeneral.SaveOption();
                    Process.Start(server, SelectedPort);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private void StopServer()
        {
            try
            {
                dynamic res = RestApi.NewInstance(Method.GET)
                            .AddCommonUri(RestUrlType.WesServer, "system/shutdown")
                            .Execute().To<object>();
                if (res.result == true)
                {
                    WesModernDialog.ShowWesMessage("发送关闭消息成功");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private void TestPrint()
        {
            try
            {
                dynamic res = RestApi.NewInstance(Method.GET)
                            .AddCommonUri(RestUrlType.WesServer, "system/printtest")
                            .Execute().To<object>();
                if (res[0].result == true)
                {
                    WesModernDialog.ShowWesMessage("發送測試消息成功");
                }
            }
            catch (Exception ex)
            {
                WesModernDialog.ShowWesMessage($"發送測試消息失敗，原因:{ex.Message}");
            }
        }

        private void TestLocalPrint()
        {
            try
            {
                PrintTemplateModel barTenderModel = new PrintTemplateModel();
                var testData = new Dictionary<string, string>();
                testData.Add("InvoiceNo", "这是一个WES测试页");
                barTenderModel.PrintData = testData;
                barTenderModel.TemplateFileName = "PackageId.btw";

                LabelPrintBase labelPrint = new LabelPrintBase(new List<PrintTemplateModel>() { barTenderModel }, false);
                var res = labelPrint.Print();
                if (res == ErrorCode.Success)
                {
                    WesModernDialog.ShowWesMessage("測試本機打印成功");
                }
                else
                {
                    WesModernDialog.ShowWesMessage($"測試本機打印失敗，原因:{res.ToString()}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }
    }
}
