using System.Collections.Generic;
using Wes.Print;
using Wes.Server.Listener;

namespace Wes.Server.Controller
{
    [HttpRequest("system")]
    public class SystemController : BaseController, IKernelRequest
    {
        [RequestUrl(Url = "shutdown", Description = "关闭服务")]
        public void Shutdown(Dictionary<string, string> query)
        {
            PopupService.GetMainWindow((win) =>
            {
                if (win != null) win.CloseServer();
            });
        }

        [RequestUrl(Url = "printtest", Description = "打印测试")]
        public void PrintTest(Dictionary<string, string> query)
        {
            PrintTemplateModel barTenderModel = new PrintTemplateModel();
            var testData = new Dictionary<string, string>();
            testData.Add("InvoiceNo", "这是一个WES测试页");
            barTenderModel.PrintData = testData;
            barTenderModel.TemplateFileName = "PackageId.btw";

            LabelPrintBase labelPrint = new LabelPrintBase(new List<PrintTemplateModel>() { barTenderModel }, false);
            var res = labelPrint.Print();
            ShowPrintResult(res);
        }
    }
}
