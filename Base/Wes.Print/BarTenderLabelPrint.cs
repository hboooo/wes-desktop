using BarTender;
using Seagull.BarTender.Print;
using System;
using System.Collections.Generic;
using System.IO;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Messages = Seagull.BarTender.Print.Messages;

namespace Wes.Print
{
    /// <summary>
    /// BarTender打印模块
    /// </summary>
    public class BarTenderLabelPrint : ILabelPrint
    {
        public PrintTemplateModel TemplateModel { get; set; }

        public PrintParam Param { get; set; }

        private LabelFormatDocument OpenDocument(string path, string md5)
        {
            LabelFormatDocument labelDocument = null;
            try
            {
                string name = System.IO.Path.GetExtension(path);
                if (string.Compare(name, ".btw", true) == 0)
                {
                    if (!WesPrint.Engine.TryGetDoc(md5, out labelDocument))
                        labelDocument = WesPrint.Engine.BarTenderPrint.Documents.Open(path);
                }
                else
                {
                    LoggingService.Error("Print Error:The template suffix name of BarTender must be .btw ");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return labelDocument;
        }

        private void PrepareDocumentData(PrintTemplateModel templateModel, LabelFormatDocument labelDocument)
        {
            if (templateModel.PrintDataValues == null)
            {
                try
                {
                    templateModel.PrintDataValues = DynamicJson.DeserializeObject<Dictionary<string, object>>(DynamicJson.SerializeObject(templateModel.PrintData));
                }
                catch (Exception ex)
                {
                    LoggingService.Error("Printer convert json failed", ex);
                }
            }
            LoggingService.InfoFormat("Print data:{0}", DynamicJson.SerializeObject(templateModel.PrintDataValues));
            foreach (var item in labelDocument.SubStrings)
            {
                object value = null;
                templateModel.PrintDataValues.TryGetValue(item.Name, out value);
                if (!string.IsNullOrEmpty(value == null ? "" : value.ToString()))
                    item.Value = value.ToString();
                else
                    item.Value = " ";
            }
        }

        public bool Print()
        {
            return PrintOne(TemplateModel, Param);
        }

        private void ExportLabelImage(LabelFormatDocument labelDocument, string md5)
        {
            try
            {
                Utils.CheckCreateDirectoroy(AppPath.LabelTemplateImagePath, true);
                string path = Path.Combine(AppPath.LabelTemplateImagePath, md5 + ".png");
                labelDocument.ExportImageToFile(path, ImageType.PNG, ColorDepth.Mono, new Resolution(ImageResolution.Screen), OverwriteOptions.Overwrite);
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private bool PrintOne(PrintTemplateModel templateModel, PrintParam param)
        {
            try
            {
                string md5Str = MD5HashProvider.GetMD5HashFromFile(templateModel.TemplateFile);
                LabelFormatDocument labelDocument = OpenDocument(templateModel.TemplateFile, md5Str);
                if (labelDocument == null || string.IsNullOrEmpty(md5Str)) return false;

                WesPrint.Engine.AddDocument(md5Str, labelDocument);
                LoggingService.Info("Print template file:" + templateModel.TemplateFile);
                PrepareDocumentData(templateModel, labelDocument);

                labelDocument.PrintSetup.PrinterName = templateModel.Printer;
                LoggingService.InfoFormat("Printer name:{0}", labelDocument.PrintSetup.PrinterName);


                if (LabelPrintConfigure.GetDisplayLabelImage())
                {
                    string nameMD5 = templateModel.TemplateFileName.GetStringMd5String();
                    using (DebugTimer.Time($"Export bartender lable image {nameMD5}"))
                    {
                        ExportLabelImage(labelDocument, nameMD5);
                    }
                }

                if (WesPrint.Engine.IsPrintEvented)
                {
                    WesPrint.Engine.OnBeforePrint(templateModel, param);
                }

                LoggingService.Info("Begin print...");
                Messages messages = null;
                Result res = labelDocument.Print(nameof(BarTenderLabelPrint), out messages);
                LoggingService.InfoFormat("End print,result:{0}", res.ToString());

                string messageString = OutputMessage(messages);
                if (res == Result.Failure)
                {
                    this.Clear();
                    LoggingService.Info("Print Failed:" + messageString);
                }
                else
                {
                    if (WesPrint.Engine.IsPrintEvented)
                    {
                        WesPrint.Engine.OnPrinted(templateModel, param);
                    }
                    LabelContentCheck(templateModel, md5Str, labelDocument);
                    this.Clear();
                    return true;
                }
            }
            catch (PrintEngineException exception)
            {
                LoggingService.Error("打印標籤異常", exception);
                WesPrint.Engine.Dispose();
            }
            return false;
        }

        public void PreparePrinter()
        {
            string printerName = TemplateModel.Printer;
            if (String.IsNullOrWhiteSpace(printerName))
            {
                printerName = WesPrint.Engine.GetPrinterName(TemplateModel.TemplateFileName);
                TemplateModel.Printer = printerName;
            }
        }

        private void LabelContentCheck(PrintTemplateModel templateModel, string md5Str, LabelFormatDocument labelDocument)
        {
            try
            {
                if (Param.IsCheck == false) { return; }

                string key = templateModel.Printer.GetStringMd5String();
                WesPrint.Engine.LabelCheckTimes.TryGetValue(key, out int printTimes);

                if ((Param.RiseCheckLabel & Model.PrintCheckType.PrintOne) == Model.PrintCheckType.PrintOne)
                {
                    Dictionary<string, string> checkData = Param.LabelCheckDatas;
                    if (checkData == null || checkData.Count == 0)
                    {
                        labelDocument.SaveAs(MakeTempFile(templateModel.TemplateFileName), true);
                        checkData = GetLabelCode(MakeTempFile(templateModel.TemplateFileName));
                    }
                    WesPrint.Engine.OnLabelChecked(checkData, Param.RiseCheckLabel);
                }
                else
                {
                    if (WesPrint.Engine.IsRequireCheck(md5Str, templateModel.TemplateFileName, Param.DefaultCheck, out bool isRequire)
                        || printTimes < WesPrint.Engine.LABEL_CHECK_TIMES_MAX)
                    {
                        labelDocument.SaveAs(MakeTempFile(templateModel.TemplateFileName), true);
                        var checkDatas = GetLabelCode(MakeTempFile(templateModel.TemplateFileName));
                        WesPrint.Engine.OnLabelChecked(checkDatas);
                        WesPrint.Engine.RemoveLabelPrint(md5Str);
                    }
                    if (isRequire)
                    {
                        WesPrint.Engine.AddLabelPrint(md5Str);
                    }
                }

                WesPrint.Engine.LabelCheckTimes[key] = ++printTimes;
                LoggingService.Debug($"本次啟動[{templateModel.Printer}]已打印{printTimes}次標籤");
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
        }

        private string OutputMessage(Messages messages)
        {
            string messageString = "\n\nMessages:";
            foreach (Seagull.BarTender.Print.Message message in messages)
            {
                messageString += "\n\n" + message.Text;
            }
            return messageString;
        }

        private string MakeTempFile(string name)
        {
            return Path.Combine(AppPath.BasePath, "LabelTemplates", "temp_" + name);
        }

        public Dictionary<string, string> GetLabelCode(string localFileName)
        {
            Dictionary<string, string> labelCodeList = new Dictionary<string, string>();
            try
            {
                BarTender.Application barTenderApp;
                BarTender.Format barTenderFormat;
                if (!File.Exists(localFileName)) return labelCodeList;

                barTenderApp = new BarTender.Application();
                barTenderFormat = barTenderApp.Formats.Open(localFileName, false);
                foreach (DesignObject designObject in barTenderFormat.Objects)
                {
                    if (designObject.Type == BtObjectType.btObjectBarcode)
                    {
                        labelCodeList[designObject.Name] = designObject.Value;
                    }
                }

                barTenderFormat.Close();
                barTenderApp.Quit();
                Utils.DeleteFile(localFileName);

            }
            catch (Exception ex)
            {
                LoggingService.Error("獲取標籤數據異常", ex);
            }
            return labelCodeList;
        }

        private void Clear()
        {
            if (TemplateModel != null)
            {
                TemplateModel.PrintDataValues = null;
                TemplateModel.Printer = "";
            }
            TemplateModel = null;
            Param = null;
        }

        public void Dispose()
        {
            Clear();
        }

        public void VerifyingData()
        {
            if (TemplateModel == null)
            {
                throw new WesException("BarTenderLabelPrint Property[PrintTemplate] can not be null");
            }
            else if ((TemplateModel.PrintDataValues == null && TemplateModel.PrintData == null) || string.IsNullOrWhiteSpace(TemplateModel.TemplateFileName))
            {
                throw new WesException("BarTenderLabelPrint PrintTemplate Property[PrintDataValues,PrintData,TemplateFileName] can not be null");
            }
        }
    }
}
