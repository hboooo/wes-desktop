using System.Collections.Generic;
using Wes.Print;

namespace Wes.Desktop.Windows.Printer
{
    public interface IPrintModalWindow
    {
        /// <summary>
        /// 打印机列表
        /// </summary>
        List<string> Printers { get; set; }
        /// <summary>
        /// 模态窗体设置
        /// </summary>
        ModalItemValue ItemValue { get; set; }

        List<PrintTemplateModel> Templates { get; set; }

        List<PrintTemplateModel> PrintedTemplates { get; set; }

        void ShowModalWindow();

        void UpdateLabelCount();

        void UpdateLabelImage(string name);
    }
}
