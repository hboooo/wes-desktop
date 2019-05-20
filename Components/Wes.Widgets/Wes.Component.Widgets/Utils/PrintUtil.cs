using System.Collections.Generic;
using System.Diagnostics;
using Wes.Print;

namespace Wes.Component.Widgets.Utils
{
    public class PrintUtil
    {
        public static void PrivatePrint(dynamic vm, List<dynamic> labels)
        {
            PrivatePrint(vm, labels, true);
        }

        public static void PrivatePrint(dynamic vm, List<dynamic> labels, bool isEnabledLabelCheck)
        {
            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
            foreach (var label in labels)
            {
                for (int i = 0; i < (int) label.printCount; i++)
                {
                    PrintTemplateModel ptm = new PrintTemplateModel();
                    ptm.TemplateFileName = label.labelName + ".btw";
                    ptm.PrintData = label.context;
                    templates.Add(ptm);
                }
            }

            var labelPrint = new LabelPrintBase(templates);
            ErrorCode error = labelPrint.Print();
            Debug.WriteLine($"打印返回CODE: {error}");
        }
    }
}