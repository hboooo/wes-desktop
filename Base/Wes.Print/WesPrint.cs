using Seagull.BarTender.Print;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TECIT.TFORMer;
using Wes.Print.Model;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Print
{
    public class WesPrint
    {
        #region BarTender

        private static WesPrint _wesPrintEngine;

        /// <summary>
        /// 保存当前打印过的模板,下次不再打开
        /// </summary>
        private static Dictionary<string, LabelFormatDocument> _documents = new Dictionary<string, LabelFormatDocument>();

        /// <summary>
        /// 最大缓存文件数量
        /// </summary>
        public int MaxCacheDocs = 500;

        #endregion

        #region TFORMer

        public string License = "Spread Logistics Limit Hong Kong";

        public int NoOfLicense = 1;

        public string Key = "F9AFC8929904E21C3AE998C14BC84FB9";

        public int LicenseKind = 5;

        #endregion

        /// <summary>
        /// 默认打印20次检查一次标签
        /// </summary>
        public int CheckTimespan = 20;

        /// <summary>
        /// 是否触发打印事件,默认为true
        /// </summary>
        public bool IsPrintEvented = true;

        private static readonly object _locker = new object();

        /// <summary>
        /// 标签打印次数
        /// </summary>
        private static Dictionary<string, int> _labelPrintTimes = new Dictionary<string, int>();

        /// <summary>
        /// 每台电脑的第一单连续检测次數，檢測3次后，按標籤檢測間隔進行檢測
        /// </summary>
        public Dictionary<string, int> LabelCheckTimes = new Dictionary<string, int>();
        public int LABEL_CHECK_TIMES_MAX = 3;

        public Engine BarTenderPrint;

        private bool _isInitialed = true;
        /// <summary>
        /// 打印机模块是否成功加载
        /// </summary>
        public bool IsInitialed
        {
            get { return _isInitialed; }
        }

        public Dictionary<string, LabelFormatDocument> Document
        {
            get
            {
                return _documents;
            }
        }

        private WesPrint()
        {
            LoggingService.Debug("Initialize printer...");
            try
            {
                using (DebugTimer.Time("Initialize bartender engine"))
                {
                    BarTenderPrint = new Engine(true);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
                _isInitialed = false;
            }
            try
            {
                if (IsInitialed)
                {
                    using (DebugTimer.Time("Initialize TFORMer engine"))
                    {
                        TFORMer.License(License, (TECIT.TFORMer.LicenseKind)LicenseKind, NoOfLicense, Key);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
                _isInitialed = false;
            }
        }

        public static WesPrint Engine
        {
            get
            {
                if (_wesPrintEngine == null)
                {
                    lock (_locker)
                    {
                        if (_wesPrintEngine == null)
                        {
                            _wesPrintEngine = new WesPrint();
                        }
                    }
                }
                return _wesPrintEngine;
            }
        }

        public bool TryGetDoc(string md5, out LabelFormatDocument doc)
        {
            return _documents.TryGetValue(md5, out doc);
        }

        public void AddDocument(string md5, LabelFormatDocument doc)
        {
            if (_documents.Count > MaxCacheDocs)
            {
                _documents.Remove(_documents.LastOrDefault().Key);
            }
            _documents[md5] = doc;
        }

        /// <summary>
        /// 清除bartender缓存的document对象
        /// 清除后会重新加载bartender模板，不建议经常清除，影响性能
        /// </summary>
        public void ClearDocument()
        {
            try
            {
                _documents?.Clear();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        /// <summary>
        /// 记录打印标签次数
        /// </summary>
        /// <param name="md5"></param>
        public void AddLabelPrint(string md5)
        {
            if (_labelPrintTimes.ContainsKey(md5))
                _labelPrintTimes[md5] = ++_labelPrintTimes[md5];
            else
                _labelPrintTimes[md5] = 1;

            LabelPrintConfigure.SavePrintTimes(_labelPrintTimes);
        }

        public void RemoveLabelPrint(string md5)
        {
            if (_labelPrintTimes.ContainsKey(md5))
                _labelPrintTimes.Remove(md5);

            LabelPrintConfigure.SavePrintTimes(_labelPrintTimes);
        }

        /// <summary>
        /// 是否需要检测打印标签内容
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        public bool IsRequireCheck(string md5, string labelName, bool defaultCheck, out bool isRequire)
        {
            isRequire = false;
            int checkTimespan = CheckTimespan;
            var config = GetPrinterConfig(labelName);
            if (config != null && !string.IsNullOrEmpty(config.labelName.ToString()))
            {
                checkTimespan = (int)config.timespan;
                isRequire = (bool)config.isCheck;
            }
            else
            {
                if (defaultCheck)
                {
                    checkTimespan = CheckTimespan;
                    isRequire = true;
                }
            }

            if (isRequire)
            {
                var tempTimes = LabelPrintConfigure.GetPrintTimes();
                if (tempTimes != null)
                    _labelPrintTimes = tempTimes;
                if (_labelPrintTimes.ContainsKey(md5) && _labelPrintTimes[md5] >= checkTimespan)
                {
                    return true;
                }
            }

            return false;
        }

        public String GetPrinterName(string labelName)
        {
            string printerName = string.Empty;
            var config = GetPrinterConfig(labelName);
            if (config != null && config.printer != null && !string.IsNullOrEmpty(config.printer.ToString()))
            {
                printerName = config.printer.ToString();
            }
            else
            {
                Printer printer = WesPrinter.GetDefaultPrinter();
                if (printer != null) printerName = printer.PrinterName;
            }
            return printerName;
        }

        public dynamic GetPrinterConfig(string labelName)
        {
            var config = LabelPrintConfigure.GetPrintConfig();
            if (config != null)
            {
                string name = Path.GetFileNameWithoutExtension(labelName);
                foreach (var item in config)
                {
                    var labels = DynamicJson.DeserializeObject<object>(item.Value);
                    if (labels != null)
                    {
                        foreach (var label in labels)
                        {
                            if (label.labelName != null && string.Compare(label.labelName.ToString(), name) == 0)
                            {
                                return label;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 更新所有Label模板
        /// </summary>
        /// <param name="updateCallback">更新一個模板文件完成的回調</param>
        /// <param name="complateCallback">更新所有模板文件全部完成的回調</param>
        public void UpdateTemplates(Action<int, string> updateCallback = null, Action<List<string>> complateCallback = null)
        {
            List<string> updateFiles = new List<string>();
            string path = System.IO.Path.Combine(AppPath.BasePath, "LabelTemplates");
            if (!Utils.CheckCreateDirectoroy(path))
            {
                complateCallback?.Invoke(updateFiles);
                return;
            }

            string[] templates = Directory.GetFiles(path, "*.*");
            ILabelPrintTemplateDownload templateDownload = new FtpDownload();
            List<string> successList = new List<string>();
            DownloadTemplateStep(templates, templateDownload, 0, (count, file) =>
            {
                successList.Add(file);
                updateCallback?.Invoke(count, file);
            }, () =>
            {
                complateCallback?.Invoke(successList);
            });
        }

        private void DownloadTemplateStep(string[] files, ILabelPrintTemplateDownload templateDownload, int index = 0, Action<int, string> action = null, Action complated = null)
        {
            if (files == null || files.Length == 0 || files.Length <= index)
            {
                complated?.Invoke();
                return;
            }
            string file = files[index];
            templateDownload.DownloadFileAsync(new FileInfo(file).Name, (f, res) =>
            {
                if (res)
                {
                    action?.Invoke(files.Length, f);
                }
                else
                {
                    LoggingService.Error(new WesException($"{f} labelTemplate update failed"));
                }
                DownloadTemplateStep(files, templateDownload, ++index, action, complated);
            });
        }

        /// <summary>
        /// 释放bartender使用资源
        /// </summary>
        public void Dispose()
        {
            if (BarTenderPrint != null)
            {
                _documents.Clear();
                try
                {
                    BarTenderPrint.Stop();
                }
                catch { }
            }
            _isInitialed = true;
            LoggingService.Debug("釋放打印機資源");
        }

        #region Delegate

        public delegate void LabelEventHandler(object sender, LabelEventArgs e);

        public delegate void PrintEventHandler(object sender, PrintEventArgs e);

        #endregion

        #region Events

        private event LabelEventHandler _labelChecked;

        public event LabelEventHandler LabelChecked
        {
            add
            {
                _labelChecked += value;
            }
            remove
            {
                _labelChecked -= value;
            }
        }

        private event PrintEventHandler _beforePrePrint;

        public event PrintEventHandler BeforePrePrint
        {
            add
            {
                _beforePrePrint += value;
            }
            remove
            {
                _beforePrePrint -= value;
            }
        }

        private event PrintEventHandler _beforePrint;

        public event PrintEventHandler BeforePrint
        {
            add
            {
                _beforePrint += value;
            }
            remove
            {
                _beforePrint -= value;
            }
        }

        public event PrintEventHandler _printed;

        public event PrintEventHandler Printed
        {
            add
            {
                _printed += value;
            }
            remove
            {
                _printed -= value;
            }
        }

        public event PrintEventHandler _printComplate;

        public event PrintEventHandler PrintComplate
        {
            add
            {
                _printComplate += value;
            }
            remove
            {
                _printComplate -= value;
            }
        }

        internal void OnLabelChecked(Dictionary<string, string> labelValues, PrintCheckType printCheckType = PrintCheckType.None)
        {
            if (labelValues == null || labelValues.Count == 0)
            {
                LoggingService.Info("LabelValues args is null,Ignore OnLabelChecked");
                return;
            }

            try
            {
                if (_labelChecked != null)
                {
                    LoggingService.DebugFormat("Event {0}, Event Args:{1}", "LabelChecked", DynamicJson.SerializeObject(labelValues));
                    _labelChecked(this, new LabelEventArgs(labelValues, printCheckType));
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("LabelChecked Error", ex);
            }
        }

        internal void OnBeforePrePrint(List<PrintTemplateModel> templates, PrintParam param)
        {
            if (templates == null || templates.Count == 0)
            {
                LoggingService.DebugFormat("Event {0}, Event Args can not be null", "OnBeforePrePrint");
                return;
            }
            try
            {
                if (_beforePrePrint != null)
                {
                    LoggingService.DebugFormat("Event {0}, Event Args:{1}", "OnBeforePrePrint", DynamicJson.SerializeObject(param));
                    _beforePrePrint(this, new PrintEventArgs(templates, param));
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("OnBeforePrePrint Error", ex);
            }
        }

        internal void OnBeforePrint(PrintTemplateModel template, PrintParam param)
        {
            if (template == null)
            {
                LoggingService.DebugFormat("Event {0}, Event Args can not be null", "OnBeforePrint");
                return;
            }
            try
            {
                if (_beforePrint != null)
                {
                    LoggingService.DebugFormat("Event {0}, Event Args:{1}", "OnBeforePrint", DynamicJson.SerializeObject(param));
                    _beforePrint(this, new PrintEventArgs(template, param));
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("OnBeforePrint MutiPrint Error", ex);
            }
        }

        internal void OnPrinted(PrintTemplateModel template, PrintParam param)
        {
            if (template == null)
            {
                LoggingService.DebugFormat("Event {0}, Event Args can not be null", "OnPrinted");
                return;
            }
            try
            {
                if (_printed != null)
                {
                    LoggingService.DebugFormat("Event {0}, Event Args:{1}", "OnPrinted", DynamicJson.SerializeObject(param));
                    _printed(this, new PrintEventArgs(template, param));
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("OnPrinted Error", ex);
            }
        }

        internal void OnPrintComplate(List<PrintTemplateModel> templates, PrintParam param)
        {
            if (templates == null || templates.Count == 0)
            {
                LoggingService.DebugFormat("Event {0}, Event Args can not be null", "OnPrintComplate");
                return;
            }

            try
            {
                if (_printComplate != null)
                {
                    LoggingService.DebugFormat("Event {0}, Event Args:{1}", "OnPrintComplate", DynamicJson.SerializeObject(param));
                    _printComplate(this, new PrintEventArgs(templates, param));
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("OnPrintComplate Error", ex);
            }
        }
        #endregion
    }
}
