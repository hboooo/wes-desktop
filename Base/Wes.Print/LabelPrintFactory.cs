using System;
using Wes.Utilities;

namespace Wes.Print
{
    public class LabelPrintFactory
    {
        public static ILabelPrint CreateLabelPrint(PrintTemplateModel templateModel, PrintParam param)
        {
            ILabelPrint labelPrint = null;
            try
            {
                switch (templateModel.Mode)
                {
                    case PrintMode.General:
                        break;
                    case PrintMode.BarTender:
                        labelPrint = new BarTenderLabelPrint();
                        break;
                    case PrintMode.TFORMer:
                        labelPrint = new TFORMerLabelPrint();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error("初始化打印機實例失敗", ex);
            }
            if (labelPrint != null)
            {
                labelPrint.TemplateModel = templateModel;
                labelPrint.Param = param;
            }

            return labelPrint;
        }
    }
}
