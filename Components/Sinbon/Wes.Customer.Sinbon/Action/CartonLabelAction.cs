using System;
using System.Collections.Generic;
using System.IO;
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

namespace Wes.Customer.Sinbon.Action
{
    public class CartonLabelAction : ScanActionBase<WesFlowID, CartonLabelAction>, IScanAction
    {
        public const string CARTONLABEL = "cartonlableling/";

        public const string GETLABELINFO = CARTONLABEL + "get-label-info";
        public const string GETCARTONLABELINFO = CARTONLABEL + "get-carton-label-info";
        public const string PRINTCARTONLOG = CARTONLABEL + "print-carton-label";
        public const string UPDATEINFO = CARTONLABEL + "update-info";
        public const string OPERATIONLOG = CARTONLABEL + "op-log";
        public const long CHECKDIMENSIONSCRIPTID = 6420095813136949248L;

        [Ability(6418283844952133632, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanLoadingNo(string value)
        {
            if (!StringExtensions.IsSxt(value))
            {
                throw new WesException("LoadingNo 不合法");
            }
            base.Vm.SelfInfo.LoadingNo = value;
            dynamic result = RestApi.NewInstance(Method.GET)
                    .AddUri(GETLABELINFO)
                    .AddQueryParameter("loadingNo", value)
                    .Execute().To<object>();
            
            base.Vm.SelfInfo.ShippingDate = result.shippingDate;
            base.Vm.SelfInfo.LabelInfo = result.labelInfo;

            base.Vm.SelfInfo.labelRequirement = result.labelRequirement;
            BindImage(result.labelRequirement);
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            base.Vm.SelfInfo.Target = result.consignee;
            base.Vm.SelfInfo.Consignee = result.consignee;
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
                    .AddUri(GETCARTONLABELINFO)

                    .AddQueryParameter("loadingNo", base.Vm.SelfInfo.LoadingNo)
                    .AddQueryParameter("packageId", value)
                    .Execute().To<object>();

            Vm.SelfInfo.LabelCount = 0;

            Vm.SelfInfo.GW = result.gw;
            Vm.SelfInfo.MarkInfos = result.marks;
            Vm.SelfInfo.CartonInfos = result.cartons;
            Vm.SelfInfo.ScanStatus = result.scanStatus;
            Vm.SelfInfo.CartonSize = result.cartonSize;
            Vm.SelfInfo.SIZE = result.cartonSize;
            Vm.SelfInfo.NW = result.nw;

            //如果是重打,查看是否有权限
            base.Vm.SelfInfo.IsPrint = false;
            if ((int)Vm.SelfInfo.ScanStatus == 3)
            {
                base.Vm.SelfInfo.IsPrint = MasterAuthorService.Authorization(VerificationType.Print);
                if (!Vm.SelfInfo.IsPrint) return;
            }

            double _gw = 0;
            if (result.gw == null || !double.TryParse(result.gw.ToString(), out _gw) || _gw <= 0)
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_GW);
            }
            else
            {
                base.Vm.SelfInfo.GW = _gw;
                PrintLabel();
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            }
        }

        [Ability(6418359164048773120, AbilityType.ScanAction)]
        public virtual void ScanFlowActionEntryGw(string value)
        {
            double gw = 0;
            if (!double.TryParse(value, out gw) || gw <= 0)
            {
                throw new WesException("重量不合法");
            }
            Vm.SelfInfo.GW = gw.ToString("0.00");
            if (Vm.SelfInfo.NW != null && Vm.SelfInfo.NW.ToString() == "0")
                Vm.SelfInfo.NW = (gw - 0.5).ToString("0.00");
            this.Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_DIM);
        }

        [Ability(6418359187801120768, AbilityType.ScanAction)]
        public virtual void ScanFlowActionEntryDim(string value)
        {

            dynamic result = RestApi.NewInstance(Method.GET)
                   .AddUriParam(RestUrlType.WmsServer, CHECKDIMENSIONSCRIPTID, null)
                   .AddQueryParameter("dimension", value)
                   .Execute().To<object>();

            base.Vm.SelfInfo.CartonSize = result.dimension;
            base.Vm.SelfInfo.SIZE = result.dimension;
            double cbm = result.length * result.width * result.height * 0.0000353 / 35.315;
            base.Vm.SelfInfo.CBM = cbm.ToString("0.000");
            PrintLabel();

            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        private void BindImage(dynamic imageList)
        {
            List<object> datas = new List<object>();
            foreach (var item in imageList)
            {
                datas.Add(new
                {
                    ImageUri = item.photoUrl.ToString(),
                    FileName = item.fileName.ToString(),
                    Desc = Path.GetFileNameWithoutExtension(item.fileName.ToString()),
                    Remark = item.Remarks == null ? "" : item.rmarks.ToString()
                });
            }
            this.Vm.SelfInfo.imageViewList = datas;
        }


        public void PrintLabel()
        {
            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
            foreach (var item in Vm.SelfInfo.LabelInfo)
            {
                string labelName = item.labelName.ToString();
                string fileName = labelName + ".btw";
                if ("CartonLabel".Equals(item.labelType.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    //箱标  
                    foreach (var carton in Vm.SelfInfo.CartonInfos)
                    {
                        Dictionary<string, object> dic = DynamicUtil.AppendDictionary(carton, Vm.SelfInfo);

                        if (dic.ContainsKey("SIZE"))
                        {
                            dic["SIZE"] = ConvertSize(dic["SIZE"]);
                        }

                        Vm.SelfInfo.GW = dic["GW"];
                        Vm.SelfInfo.CartonSize = dic["CartonSize"];

                        if (string.Compare(base.Vm.SelfInfo.Target.ToString(), "C03853", true) == 0)
                        {
                            //以BM41P-14DP/2-0.35V開始的這一類料號出SB_T323-3_Ctn2.btw標
                            string partNo = dic.ContainsKey("PartNo") ? dic["PartNo"].ToString() : "";
                            if (partNo.StartsWith("BM41P-14DP/2-0.35V"))
                            {
                                if (string.Compare("SB_T323-3_Ctn2.btw", fileName, true) == 0)
                                {
                                    templates.Add(PrepareData(dic, fileName));
                                    insertOpLog("PrintCartonLabel", labelName, dic);
                                }
                            }
                            else
                            {
                                if (string.Compare("SB_T323-3_Ctn1.btw", fileName, true) == 0)
                                {
                                    templates.Add(PrepareData(dic, fileName));
                                    insertOpLog("PrintCartonLabel", labelName, dic);
                                }
                            }
                        }
                        else if (string.Compare(base.Vm.SelfInfo.Target.ToString(), "C03724", true) == 0)
                        {
                            //以BM41P-14DP/2-0.35V開始的這一類料號出SB_T323-3_Ctn2.btw標
                            string partNo = dic.ContainsKey("PartNo") ? dic["PartNo"].ToString() : "";
                            if (partNo.Equals("BM25U-4S/2-V(51)", StringComparison.OrdinalIgnoreCase))
                            {
                                if (string.Compare("SB_HZVIVOMP_Ctn.btw", fileName, true) == 0)
                                {
                                    templates.Add(PrepareData(dic, fileName));
                                    insertOpLog("PrintCartonLabel", labelName, dic);
                                }
                            }
                            else
                            {
                                if (string.Compare("SB_HZVIVO_Ctn.btw", fileName, true) == 0)
                                {
                                    templates.Add(PrepareData(dic, fileName));
                                    insertOpLog("PrintCartonLabel", labelName, dic);
                                }
                            }
                        }
                        else
                        {
                            templates.Add(PrepareData(dic, fileName));
                            insertOpLog("PrintCartonLabel", labelName, dic);
                        }

                    }
                }
                else if ("MarkLabel".Equals(item.labelType.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    //箱标  
                    foreach (var mark in Vm.SelfInfo.MarkInfos)
                    {
                        Dictionary<string, object> dic = DynamicUtil.AppendDictionary(mark, Vm.SelfInfo);
                        if (dic.ContainsKey("SIZE"))
                        {
                            dic["SIZE"] = ConvertSize(dic["SIZE"]);
                        }
                        Vm.SelfInfo.GW = dic["GW"];
                        Vm.SelfInfo.CartonSize = dic["CartonSize"];

                        templates.Add(PrepareData(dic, fileName));
                        insertOpLog("PrintMarkLabel", labelName, dic);
                    }
                }
            }
            Print(templates);

            if (Vm.SelfInfo.ScanStatus != 3)
            {
                Score();
                UpdateInfo();
            }
        }

        private String ConvertSize(object size)
        {
            if (size != null && !String.IsNullOrEmpty(size.ToString()))
            {
                dynamic result = RestApi.NewInstance(Method.GET)
                           .AddUriParam(RestUrlType.WmsServer, CHECKDIMENSIONSCRIPTID, null)
                           .AddQueryParameter("dimension", size)
                           .Execute().To<object>();

                return string.Format("{0}*{1}*{2}",
                    Convert.ToDouble(result.length) / 100,
                    Convert.ToDouble(result.width) / 100,
                    Convert.ToDouble(result.height) / 100);
            }
            return null;
        }

        [AbilityAble(6420101420141256704, AbilityType.ScanAction, true, KPIActionType.LSCartonLabeling | KPIActionType.LSCartonLabelingPlus, "Consignee", false)]
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
                RestApi.NewInstance(Method.POST)
                 .AddUri(UPDATEINFO)
                 .AddJsonBody("loadingNo", Vm.SelfInfo.LoadingNo)
                 .AddJsonBody("packageId", Vm.SelfInfo.PackageId)
                 .AddJsonBody("partNo", carton.PartNo.ToString())
                 .AddJsonBody("qty", carton.Qty.ToString())
                 .AddJsonBody("gw", Vm.SelfInfo.GW.ToString())
                 .AddJsonBody("cartonSize", Vm.SelfInfo.CartonSize.ToString())
                 .Execute();
            }
        }

        private void insertOpLog(string actionType, string labelName, Dictionary<string, object> dic)
        {
            string dateCode = dic.ContainsKey("ShippingDate") ? dic["ShippingDate"].ToString() : "";
            if (dateCode.Length > 10)
            {
                dateCode = dateCode.Substring(0, 10);
            }
            RestApi.NewInstance(Method.POST)
                 .AddUri(PRINTCARTONLOG)
                 .AddJsonBody("loadingNo", Vm.SelfInfo.LoadingNo)
                 .AddJsonBody("cartonNo", Vm.SelfInfo.PackageId)
                 .AddJsonBody("printType", actionType)
                 .AddJsonBody("title", dic.ContainsKey("CartonSn") ? dic["CartonSn"].ToString() : "")
                 .AddJsonBody("cpn", dic.ContainsKey("CPN") ? dic["CPN"].ToString() : "")
                 .AddJsonBody("partNo", dic.ContainsKey("PartNO") ? dic["PartNO"].ToString() : "")
                 .AddJsonBody("lotNo", dic.ContainsKey("LotNO") && dic["LotNO"] != null ? dic["LotNO"].ToString() : "")
                 .AddJsonBody("qty", dic.ContainsKey("Qty") ? dic["Qty"].ToString() : "")
                 .AddJsonBody("referenceNo", labelName.Length > 50 ? labelName.Substring(0, 50) : labelName)
                 .AddJsonBody("dateCode", dateCode)
                 .AddJsonBody("po", dic.ContainsKey("PO") && dic["PO"] != null ? dic["PO"].ToString() : "")
                 .AddJsonBody("cargoType", "")
                 .AddJsonBody("cno", "")
                 .AddJsonBody("shippingDate", dateCode)
                 .AddJsonBody("orignCountry", "CHINA")
                 .AddJsonBody("receivingNo", "")
                 .AddJsonBody("shipperName", "Sinbon")
                 .AddJsonBody("originName", "CHINA")
                 .AddJsonBody("edpKeyRule", "")
                 .AddJsonBody("invoiceNo", dic.ContainsKey("NInvoiceNo") ? dic["NInvoiceNo"].ToString() : "")
                 .AddJsonBody("printCount", 1)
                 .Execute();

            RestApi.NewInstance(Method.POST)
               .AddUri(OPERATIONLOG)
               .AddJsonBody("operationNo", Vm.SelfInfo.LoadingNo)
               .AddJsonBody("operationNo2", Vm.SelfInfo.PackageId)
               .AddJsonBody("operationNo3", labelName)
               .AddJsonBody("opType", "CartonLabelPrint")
               .AddJsonBody("opValue", dic.ContainsKey("PartNO") ? dic["PartNO"] : "")
               .AddJsonBody("opPerson", WesDesktop.Instance.User.Code)
               .AddJsonBody("remarks", "貼箱標")
               .Execute();
        }
    }
}
