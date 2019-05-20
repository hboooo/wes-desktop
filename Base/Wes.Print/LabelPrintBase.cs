using System;
using System.Collections.Generic;
using System.IO;
using Wes.Print.Model;
using Wes.Utilities;

namespace Wes.Print
{
    /// <summary>
    /// 打印模块
    /// </summary>
    public class LabelPrintBase
    {
        /// <summary>
        /// 模态窗显示相关设置
        /// </summary>
        public PrintParam PrintParam { get; private set; }

        /// <summary>
        /// 打印模板
        /// </summary>
        private List<PrintTemplateModel> PrintTemplates { get; set; }

        #region constructor

        public LabelPrintBase()
        {
            this.PrintParam = new PrintParam("請取標！再掃描");
        }

        public LabelPrintBase(PrintTemplateModel templateModel) : this()
        {
            this.PrintTemplates = new List<PrintTemplateModel>() { templateModel };
        }


        public LabelPrintBase(PrintTemplateModel templateModel, PrintCheckType checkType) : this(templateModel)
        {
            this.PrintParam.RiseCheckLabel = checkType;
        }

        public LabelPrintBase(PrintTemplateModel templateModel, string message) : this(templateModel)
        {
            this.PrintParam.Message = message;
        }

        public LabelPrintBase(PrintTemplateModel templateModel, bool defaulCheck) : this(templateModel)
        {
            this.PrintParam.DefaultCheck = defaulCheck;
        }

        public LabelPrintBase(PrintTemplateModel templateModel, string message, bool defaulCheck) : this(templateModel, message)
        {
            this.PrintParam.DefaultCheck = defaulCheck;
        }

        public LabelPrintBase(List<PrintTemplateModel> printTemplates) : this()
        {
            this.PrintTemplates = printTemplates;
        }

        public LabelPrintBase(List<PrintTemplateModel> printTemplates, PrintCheckType checkType) : this(printTemplates)
        {
            this.PrintParam.RiseCheckLabel = checkType;
        }

        public LabelPrintBase(List<PrintTemplateModel> printTemplates, string message) : this(printTemplates)
        {
            this.PrintParam.Message = message;
        }

        public LabelPrintBase(List<PrintTemplateModel> printTemplates, bool defaulCheck) : this(printTemplates)
        {
            this.PrintParam.DefaultCheck = defaulCheck;
        }

        public LabelPrintBase(List<PrintTemplateModel> printTemplates, string message, bool defaulCheck) : this(printTemplates, message)
        {
            this.PrintParam.DefaultCheck = defaulCheck;
        }

        public LabelPrintBase(List<PrintTemplateModel> printTemplates, PrintParam param) : this(printTemplates)
        {
            this.PrintParam = param;
        }

        #endregion

        #region Print


        public ErrorCode Print()
        {
            if (PrintTemplates == null || PrintTemplates.Count == 0)
                return ErrorCode.DataError;

            LoggingService.DebugFormat("共打印{0}张标签", PrintTemplates.Count);

            List<ILabelPrint> labelPrints = CreateInstance(PrintTemplates);
            foreach (var item in labelPrints)
            {
                item.VerifyingData();
                item.PreparePrinter();
            }
            WesPrint.Engine.OnBeforePrePrint(PrintTemplates, PrintParam);
            foreach (var item in labelPrints)
            {
                if (!string.IsNullOrEmpty(item.TemplateModel.Printer))
                {
                    var res = PrintOne(item);
                    if (res != ErrorCode.Success) return res;
                }
                else
                {
                    return ErrorCode.PrinterError;
                }
            }
            WesPrint.Engine.OnPrintComplate(PrintTemplates, PrintParam);
            if (((PrintParam.RiseCheckLabel & Model.PrintCheckType.PrintComplated) == Model.PrintCheckType.PrintComplated))
            {
                WesPrint.Engine.OnLabelChecked(PrintParam.LabelCheckDatas, PrintParam.RiseCheckLabel);
            }
            return ErrorCode.Success;
        }

        private List<ILabelPrint> CreateInstance(List<PrintTemplateModel> templates)
        {
            List<ILabelPrint> labelPrints = new List<ILabelPrint>();
            foreach (var item in templates)
            {
                ILabelPrint labelPrint = LabelPrintFactory.CreateLabelPrint(item, PrintParam);
                if (labelPrint != null) labelPrints.Add(labelPrint);
            }
            return labelPrints;
        }

        private ErrorCode PrintOne(ILabelPrint labelPrint)
        {
            try
            {
                ILabelPrintTemplateDownload templateDownload = new FtpDownload();
                string downloadFile = null;

                templateDownload.DownloadFile(labelPrint.TemplateModel.TemplateFileName, ref downloadFile);
                labelPrint.TemplateModel.TemplateFile = downloadFile;

                if (!File.Exists(downloadFile))
                    return ErrorCode.DownloadTemplateFailure;
                else
                {
                    bool res = labelPrint.Print();
                    labelPrint.Dispose();
                    if (res) return ErrorCode.Success;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return ErrorCode.InternalError;
        }

        #endregion
    }
}
