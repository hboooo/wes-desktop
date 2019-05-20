using System.Collections.Generic;
using Wes.Desktop.Windows.Printer;
using Wes.Print;
using Wes.Utilities;

namespace Wes.Desktop.Windows.View
{
    /// <summary>
    /// 打印模态窗口
    /// </summary>
    public class PrintModalWindowProvider : IPrintWindowProvider
    {
        public void Initialize()
        {
            //初始化打印机引擎
            WesPrint.Engine.LabelChecked -= Engine_LabelChecked;
            WesPrint.Engine.LabelChecked += Engine_LabelChecked;
            WesPrint.Engine.BeforePrint -= Engine_BeforePrint;
            WesPrint.Engine.BeforePrint += Engine_BeforePrint;
            WesPrint.Engine.BeforePrePrint -= Engine_BeforePrePrint;
            WesPrint.Engine.BeforePrePrint += Engine_BeforePrePrint;
            WesPrint.Engine.Printed -= Engine_Printed;
            WesPrint.Engine.Printed += Engine_Printed;
            LoggingService.Debug("Initialize printer complated...");
        }

        private void Engine_BeforePrint(object sender, PrintEventArgs e)
        {
            if (_printModalWindow != null)
            {
                _printModalWindow.UpdateLabelImage(e.Template.TemplateFileName);
            }
        }

        private IPrintModalWindow _printModalWindow = null;

        private void Engine_Printed(object sender, PrintEventArgs e)
        {
            if (_printModalWindow != null)
            {
                if (_printModalWindow.PrintedTemplates == null)
                    _printModalWindow.PrintedTemplates = new List<PrintTemplateModel>();
                _printModalWindow.PrintedTemplates.Add(e.Template);

                _printModalWindow.UpdateLabelCount();
            }
        }

        private void Engine_BeforePrePrint(object sender, PrintEventArgs e)
        {
            ShowModalWindow(e);
        }

        private void Engine_LabelChecked(object sender, Print.LabelEventArgs e)
        {
            if (e.LabelValues != null && e.LabelValues.Count > 0)
            {
                ActiveCheckLabelWindow checkLabelWindow = new ActiveCheckLabelWindow();
                checkLabelWindow.AddItem(e.LabelValues);
                checkLabelWindow.ShowDialog();
            }
        }

        public void ShowModalWindow(PrintEventArgs printEventArgs)
        {
            _printModalWindow = GetInstance(printEventArgs);
            _printModalWindow?.ShowModalWindow();
        }

        private IPrintModalWindow GetInstance(PrintEventArgs printEventArgs)
        {
            IPrintModalWindow modalWindow = null;
            if (printEventArgs.Param.IsMaskedLayerDisplay && SerialPortProvider.IsSerialPortConfig())
            {
                List<string> printers = new List<string>();
                foreach (var item in printEventArgs.Templates)
                {
                    if (!string.IsNullOrEmpty(item.Printer) && !printers.Contains(item.Printer))
                    {
                        printers.Add(item.Printer);
                    }
                }
                modalWindow = new MutiPrintModalWindow();
                modalWindow.Templates = printEventArgs.Templates;
                modalWindow.Printers = printers;
                modalWindow.ItemValue = new ModalItemValue(
                    printEventArgs.Param.Message,
                    printEventArgs.Param.Timeout,
                    printEventArgs.Param.DialogCallback);
            }
            return modalWindow;
        }

    }
}
