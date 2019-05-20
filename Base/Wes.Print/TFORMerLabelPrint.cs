using System;
using System.Collections.Generic;
using System.IO;
using TECIT.TFORMer;
using Wes.Utilities;
using Wes.Utilities.Exception;

namespace Wes.Print
{
    /// <summary>
    /// TFORMer打印模块
    /// </summary>
    public class TFORMerLabelPrint : ILabelPrint
    {
        public PrintTemplateModel TemplateModel { get; set; }

        public PrintParam Param { get; set; }

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

        public bool Print()
        {
            return PrintOne(TemplateModel, Param);
        }

        private void ExportLabelImage(JobDataRecordSet recordSet, PrintTemplateModel templateModel, string md5)
        {
            Job printJob = new Job();
            try
            {
                Utils.CheckCreateDirectoroy(AppPath.LabelTemplateImagePath, true);
                string path = Path.Combine(AppPath.LabelTemplateImagePath, md5 + ".png");
                printJob.RepositoryName = templateModel.TemplateFile;
                printJob.JobData = recordSet;
                printJob.OutputName = path;
                printJob.PrinterType = PrinterType.ePrinterType_ImagePng;
                printJob.Print();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            finally
            {
                if (printJob != null) printJob.Dispose();
            }
        }

        private bool PrintOne(PrintTemplateModel templateModel, PrintParam param, bool isSingle = true)
        {
            LoggingService.Info("Print template file:" + templateModel.TemplateFile);
            JobDataRecordSet recordSet = PrepareDocumentData(templateModel);
            if (recordSet == null) return false;

            try
            {
                string name = System.IO.Path.GetExtension(templateModel.TemplateFile);
                if (string.Compare(name, ".tff", true) != 0)
                {
                    LoggingService.Error("Print Error:The template suffix name of TFORMer must be .tff ");
                    return false;
                }

                if (LabelPrintConfigure.GetDisplayLabelImage())
                {
                    string nameMD5 = templateModel.TemplateFileName.GetStringMd5String();
                    using (DebugTimer.Time($"Export TFORMer lable image {nameMD5}"))
                    {
                        ExportLabelImage(recordSet, templateModel, nameMD5);
                    }
                }

                if (WesPrint.Engine.IsPrintEvented)
                {
                    WesPrint.Engine.OnBeforePrint(templateModel, param);
                }

                Job printJob = new Job();
                try
                {
                    printJob.RepositoryName = templateModel.TemplateFile;
                    printJob.PrinterName = templateModel.Printer;
                    printJob.JobData = recordSet;
                    printJob.Print();
                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                }
                finally
                {
                    if (printJob != null) printJob.Dispose();
                }

                if (WesPrint.Engine.IsPrintEvented)
                {
                    WesPrint.Engine.OnPrinted(templateModel, param);
                }
                LabelContentCheck(templateModel);
                this.Clear();
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.Error("TFORMer print error", ex);
            }
            return false;
        }

        private void LabelContentCheck(PrintTemplateModel templateModel)
        {
            try
            {
                if (Param.IsCheck == false) { return; }

                string key = templateModel.Printer.GetStringMd5String();
                WesPrint.Engine.LabelCheckTimes.TryGetValue(key, out int printTimes);

                if (((Param.RiseCheckLabel & Model.PrintCheckType.PrintOne) == Model.PrintCheckType.PrintOne))
                {
                    WesPrint.Engine.OnLabelChecked(Param.LabelCheckDatas, Param.RiseCheckLabel);
                }
                else
                {
                    string md5Str = templateModel.TemplateFile.GetStringMd5String();
                    if (WesPrint.Engine.IsRequireCheck(md5Str, templateModel.TemplateFileName, Param.DefaultCheck, out bool isRequire) 
                        || printTimes < WesPrint.Engine.LABEL_CHECK_TIMES_MAX)
                    {
                        Dictionary<string, string> checkDatas = null;
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

        private JobDataRecordSet PrepareDocumentData(PrintTemplateModel templateModel)
        {
            JobDataRecordSet jobDataRecordSet = new JobDataRecordSet();
            if (templateModel.PrintData == null)
            {
                LoggingService.Error("TFORMer PrintData can not be null");
                return null;
            }
            try
            {
                LoggingService.DebugFormat("Print data:{0}", DynamicJson.SerializeObject(templateModel.PrintData));
                HashSet<string> dataFields = GetDataField(templateModel.TemplateFile);
                Record record;
                foreach (var item in templateModel.PrintData)
                {
                    record = new Record();
                    Dictionary<string, object> dic = DynamicJson.DeserializeObject<Dictionary<string, object>>(DynamicJson.SerializeObject(item));
                    foreach (var child in dic)
                    {
                        if (dataFields.Contains(child.Key.ToUpper()))
                            record.Data.Add(child.Key, child.Value != null ? child.Value.ToString() : "");
                    }
                    jobDataRecordSet.Records.Add(record);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("TFORMer PrintData format error", ex);
                return null;
            }
            return jobDataRecordSet;
        }

        private HashSet<string> GetDataField(string template)
        {
            HashSet<string> dataFields = new HashSet<string>();
            try
            {
                Repository repository = new Repository(template, false, true);
                Project project = repository.GlobalProject;
                FormLayout formlayout = project.FirstFormLayout;
                DataField datafield = project.FirstDataField;
                while (datafield != null)
                {
                    dataFields.Add(datafield.Name.ToUpper());
                    datafield = datafield.Next;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            return dataFields;
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

        public void VerifyingData()
        {
            if (TemplateModel == null)
            {
                throw new WesException("TFORMerLabelPrint Property[PrintTemplate] can not be null");
            }
            else if (TemplateModel.PrintData == null || string.IsNullOrWhiteSpace(TemplateModel.TemplateFileName))
            {
                throw new WesException("TFORMerLabelPrint PrintTemplate Property[PrintData,TemplateFileName] can not be null");
            }
        }
    }
}
