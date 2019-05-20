using System;
using System.Collections.Generic;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.FitiPower.Action
{
    public class CartonLabelAction : ScanActionBase<WesFlowID, CartonLabelAction>, IScanAction
    {
        public const long INSERTOPLOGSCRIPTID = 6418404698088280064L;
        public const long CHECKDIMENSIONSCRIPTID = 6420095813136949248L;

        public const long PRINTCARTONLABELSCRIPTID = 6442622138338779136L;
        public const long GETLABELINFOSCRIPTID = 6442622111205822464L;
        public const long UPDATEINFOSCRIPTID = 6442622176607608832;
        public const long CARTONLABLELOGSCRIPTID = 6442622161445195776;


        [Ability(6418283844952133632, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanLoadingNo(string value)
        {
            if (!StringExtensions.IsSxt(value))
            {
                throw new WesException("LoadingNo 不合法");
            }

            base.Vm.SelfInfo.LoadingNo = value;
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, GETLABELINFOSCRIPTID, true)
                .AddQueryParameter("LoadingNo", value)
                .Execute().To<object>();

            base.Vm.SelfInfo.ShippingDate = result.ShippingDate;

            base.Vm.SelfInfo.EndCustomer = result.EndCustomer;
            base.Vm.SelfInfo.Consignee = result.Consignee;
            base.Vm.SelfInfo.consignee = result.Consignee;

            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            base.Vm.SelfInfo.Target = result.Consignee;

            LabelingAction.BindImage(this.Vm, "CARTON");
        }

        [Ability(6418356205290266624, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPackageId(string value)
        {
            if (!StringExtensions.IsPackageID(value))
            {
                throw new WesException("PackageId 不合法");
            }

            base.Vm.SelfInfo.PackageId = value;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, PRINTCARTONLABELSCRIPTID, true)
                .AddQueryParameter("LoadingNo", base.Vm.SelfInfo.LoadingNo)
                .AddQueryParameter("PackageId", value)
                .AddQueryParameter("Consignee", Vm.SelfInfo.Consignee)
                .AddQueryParameter("ShippingDate", base.Vm.SelfInfo.ShippingDate)
                .Execute().To<object>();
            Vm.SelfInfo.LabelCount = 0;
            Vm.SelfInfo.GW = result.GW.ToString("0.00");
            Vm.SelfInfo.CartonSize = result.CartonSize;
            Vm.SelfInfo.MarkInfos = result.MarkInfos;
            Vm.SelfInfo.CartonInfos = result.CartonInfos;
            Vm.SelfInfo.ScanStatus = result.ScanStatus;
            Vm.SelfInfo.Relabel = result.relabel;


            base.Vm.SelfInfo.LabelInfo = result.LabelInfo;
            base.Vm.SelfInfo.labelRequirement = result.labelRequirement;


            base.Vm.SelfInfo.IsPrint = false;
            if ((int) Vm.SelfInfo.ScanStatus == 3)
            {
                base.Vm.SelfInfo.IsPrint = MasterAuthorService.Authorization(VerificationType.Print);
                if (!Vm.SelfInfo.IsPrint) return;
            }

            PrintLabel();
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }


        private void BindImage(dynamic imageList)
        {
        }


        public void PrintLabel()
        {
            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();

            foreach (var item in Vm.SelfInfo.LabelInfo)
            {
                string labelName = item.LabelName.ToString();
                string fileName = labelName + ".btw";
                //箱标  
                if ("CartonLabel".Equals(item.LabelType.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var carton in Vm.SelfInfo.CartonInfos)
                    {
                        Dictionary<string, object> dic = DynamicUtil.AppendDictionary(carton, null);

                        double cw = (double) carton.CW;
                        cw = cw > 0.01 ? cw : 0.01;
                        dic["CW"] = cw.ToString("0.00");

                        double gw = (double) carton.GW;
                        gw = gw > 0.01 ? gw : 0.01;
                        dic["GW"] = gw.ToString("0.00");

                        templates.Add(PrepareData(dic, fileName));
                        insertOpLog("PrintCartonLabel", labelName, dic);
                    }
                }
                //唛头
                else if ("MarkLabel".Equals(item.LabelType.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var mark in Vm.SelfInfo.MarkInfos)
                    {
                        Dictionary<string, object> dic = DynamicUtil.AppendDictionary(mark, null);

                        double cw = (double) mark.CW;
                        cw = cw > 0.01 ? cw : 0.01;
                        dic["CW"] = cw.ToString("0.00");

                        double gw = (double) mark.GW;
                        gw = gw > 0.01 ? gw : 0.01;
                        dic["GW"] = gw.ToString("0.00");

                        templates.Add(PrepareData(dic, fileName));
                        insertOpLog("PrintCartonLabel", labelName, dic);
                    }
                }
            }

            Print(templates);

            if (Vm.SelfInfo.ScanStatus != 3)
            {
                UpdateInfo();
                Score();
            }
        }

        [AbilityAble(6420101420141256704, AbilityType.ScanAction, true,
            KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "Consignee", false)]
        public virtual bool Score()
        {
            return true;
        }

        private PrintTemplateModel PrepareData(Dictionary<string, object> dic, string fileName)
        {
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.TemplateFileName = fileName;
            pm.PrintDataValues = dic;
            Vm.SelfInfo.LabelCount++;
            return pm;
        }

        private void Print(List<PrintTemplateModel> templates)
        {
            LabelPrintBase lpb = new LabelPrintBase(templates);
            var res = lpb.Print();
        }

        private void UpdateInfo()
        {
            foreach (var carton in Vm.SelfInfo.CartonInfos)
            {
                dynamic result = RestApi.NewInstance(Method.POST)
                    .AddUriParam(RestUrlType.WmsServer, UPDATEINFOSCRIPTID, true)
                    .AddJsonBody("LoadingNo", Vm.SelfInfo.LoadingNo)
                    .AddJsonBody("PackageId", Vm.SelfInfo.PackageId)
                    .AddJsonBody("GW", Vm.SelfInfo.GW)
                    .AddJsonBody("CartonSize", Vm.SelfInfo.CartonSize)
                    .Execute().To<object>();
            }
        }

        private void insertOpLog(string actionType, string labelName, Dictionary<string, object> dic)
        {
            RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, CARTONLABLELOGSCRIPTID, true)
                .AddJsonBody("LoadingNo", Vm.SelfInfo.LoadingNo)
                .AddJsonBody("CartonNo", Vm.SelfInfo.PackageId)
                .AddJsonBody("PrintType", actionType)
                .AddJsonBody("Title",
                    dic.ContainsKey("CartonSn") && dic["CartonSn"] != null ? dic["CartonSn"].ToString() : "")
                .AddJsonBody("Cpn", dic.ContainsKey("CPN") && dic["CPN"] != null ? dic["CPN"].ToString() : "")
                .AddJsonBody("PartNo",
                    dic.ContainsKey("PartNO") && dic["PartNO"] != null ? dic["PartNO"].ToString() : "")
                .AddJsonBody("LotNo", dic.ContainsKey("LotNO") && dic["LotNO"] != null ? dic["LotNO"].ToString() : "")
                .AddJsonBody("Qty", dic.ContainsKey("Qty") && dic["Qty"] != null ? dic["Qty"].ToString() : "")
                .AddJsonBody("ReferenceNo", labelName.Length > 50 ? labelName.Substring(0, 50) : labelName)
                .AddJsonBody("DateCode",
                    dic.ContainsKey("DateCode") && dic["DateCode"] != null ? dic["DateCode"].ToString() : "")
                .AddJsonBody("Po", dic.ContainsKey("PO") && dic["PO"] != null ? dic["PO"].ToString() : "")
                .AddJsonBody("CargoType", "")
                .AddJsonBody("Cno", "")
                .AddJsonBody("ShippingDate",
                    dic.ContainsKey("ShippingDate") && dic["ShippingDate"] != null
                        ? dic["ShippingDate"].ToString()
                        : "")
                .AddJsonBody("OrignCountry", "CHINA")
                .AddJsonBody("ReceivingNo", "")
                .AddJsonBody("ShipperName", "Fitipower")
                .AddJsonBody("OriginName", "CHINA")
                .AddJsonBody("EdpKeyRule", "")
                .AddJsonBody("InvoiceNo",
                    dic.ContainsKey("NInvoiceNo") && dic["NInvoiceNo"] != null ? dic["NInvoiceNo"].ToString() : "")
                .AddJsonBody("PrintCount", 1)
                .Execute();

            RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, INSERTOPLOGSCRIPTID, true)
                .AddJsonBody("logType", "shipping")
                .AddJsonBody("operationNo", Vm.SelfInfo.LoadingNo)
                .AddJsonBody("operationNo2", Vm.SelfInfo.PackageId)
                .AddJsonBody("operationNo3", labelName)
                .AddJsonBody("opType", "CartonLabelPrint")
                .AddJsonBody("opValue", dic.ContainsKey("PartNO") ? dic["PartNO"] : "")
                .AddJsonBody("opPerson", WesDesktop.Instance.User.Code)
                .AddJsonBody("user", WesDesktop.Instance.User.Code)
                .AddJsonBody("remarks", "貼箱標")
                .Execute();
        }
    }
}