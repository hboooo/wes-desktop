using System.Collections.Generic;
using Wes.Print;
using Wes.Server.Listener;
using Wes.Wrapper;
using Wes.Utilities;

namespace Wes.Server.Controller
{
    /// <summary>
    /// 处理http请求
    /// 目前支持GET
    /// HttpRequest 设置模块名称
    /// RequestUrl 设置接口名称
    /// http接口为 http://host:port/HttpRequest/RequestUrl
    /// 例如 http://127.0.0.1:12339/print/air-label?id=10&name=123123
    /// </summary>
    [HttpRequest("print")]
    public class PrintController : BaseController, IKernelRequest
    {
        [RequestUrl(Url = "air-label", Description = "打印空运标")]
        public void AirLabel(Dictionary<string, string> query)
        {
            List<string> paras = new List<string>() { "orderGuid", "txt", "palletId", "labelCount", "ctns", "looseCtns", "qty", "location", "userCode", "version" };
            if (!IsParameterValid(query, paras)) return;

            dynamic labelList = RestApi.NewInstance(Method.POST)
                        .AddCommonUri(RestUrlType.WmsServer, "air-shipping/get-label-info")
                        .AddJsonBody(query)
                        .Execute()
                        .To<object>();
            if (labelList != null && labelList.Count > 0)
            {
                List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
                int labelCount = labelList.Count;
                for (int i = 0; i < labelCount; i++)
                {
                    var item = labelList[i];
                    PrintTemplateModel barTenderModel = new PrintTemplateModel();
                    barTenderModel.PrintData = item;
                    barTenderModel.TemplateFileName = "AirLabel-Template.btw";
                    templates.Add(barTenderModel);
                }

                LabelPrintBase labelPrint = new LabelPrintBase(templates, false);
                var res = labelPrint.Print();
                ShowPrintResult(res);
            }
        }


        [RequestUrl(Url = "pallet-label", Description = "打印板头纸")]
        public void PalletLabel(Dictionary<string, string> query)
        {
            List<string> paras = new List<string>() { "rxt", "userCode" };
            if (!IsParameterValid(query, paras)) return;

            dynamic labelList = RestApi.NewInstance(Method.POST)
                        .AddCommonUri(RestUrlType.WmsServer, "wes-server/get-pallet-label")
                        .AddJsonBody(query)
                        .Execute()
                        .To<object>();
            if (labelList != null && labelList.Count > 0)
            {
                List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
                int labelCount = labelList.Count;
                for (int i = 0; i < labelCount; i++)
                {
                    var item = labelList[i];
                    PrintTemplateModel barTenderModel = new PrintTemplateModel();
                    barTenderModel.PrintData = item;
                    barTenderModel.TemplateFileName = "RXTLabel-Template.btw";
                    templates.Add(barTenderModel);
                }
                
                LabelPrintBase labelPrint = new LabelPrintBase(templates, false);
                var res = labelPrint.Print();
                ShowPrintResult(res);
            }
        }
        
        [RequestUrl(Url ="qrcode",Description = "打印出貨QrCode")]
        public void PrintQrCode(Dictionary<string, string> query)
        {
            //台口號-platformNum，車牌-carNum，單號-oddNUm
            List<string> paras = new List<string>() { "platformNum", "carNum" , "oddNUm" };
            if (!IsParameterValid(query, paras)) return;

            string platformNum = query["platformNum"];
            string carNum = query["carNum"];
            string oddNum = query["oddNUm"];
            string qrCode = platformNum + "@" + carNum + "@" + oddNum;

            PrintTemplateModel printTemplateModel = new PrintTemplateModel();
            var testData = new Dictionary<string,string>();
            testData.Add("qrCode", qrCode);
            testData.Add("oddNum", oddNum);
            printTemplateModel.PrintData = testData;

            printTemplateModel.TemplateFileName = "qrCode_entablature.btw";

            LabelPrintBase labelPrintBase = new LabelPrintBase(new List<PrintTemplateModel>() { printTemplateModel},false);
            var res = labelPrintBase.Print();
            ShowPrintResult(res);
        }
    }
}
