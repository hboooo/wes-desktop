using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Wes.Customer.Avnet.Action
{
    public class CartonLabelAction : ScanActionBase<WesFlowID, CartonLabelAction>, IScanAction
    {
        public const long PRINT_CARTON_LABEL_SCRIPT_ID = 6418364750001872896L;
        public const long GET_LABEL_INFO_SCRIPT_ID = 6418379793280012288L;
        public const long UPDATE_INFO_SCRIPT_ID = 6419880397160587264L;
        public const long INSERT_OP_LOG_SCRIPT_ID = 6418404698088280064L;
        public const long CHECK_DIMENSION_SCRIPT_ID = 6420095813136949248L;
        public const long CARTON_LABEL_LOG_SCRIPT_ID = 6438667209886666752L;

        private const double MAX_GW = 99.99;
        private const double MIN_GW = 0.01;

        [Ability(6418283844952133632, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanTruckNo(string value)
        {
            if (!StringExtensions.IsTxt(value))
            {
                throw new WesException("Truck Order 不合法");
            }

            base.Vm.SelfInfo.TruckOrder = value;
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, GET_LABEL_INFO_SCRIPT_ID, true)
                .AddQueryParameter("TruckOrder", value)
                .Execute().To<object>();
            base.Vm.SelfInfo.ShippingDate = result.ShippingDate;
            base.Vm.SelfInfo.Field1 = result.Field1;
            base.Vm.SelfInfo.EndCustomer = result.EndCustomer;
            base.Vm.SelfInfo.Consignee = result.Consignee;
            base.Vm.SelfInfo.LabelInfo = result.LabelInfo;
            base.Vm.SelfInfo.labelRequirement = result.labelRequirement;
            BindImage(result.labelRequirement);
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            base.Vm.SelfInfo.Target = result.Consignee;
            SetActionValid();
        }

        [Ability(6418356205290266624, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPackageId(string value)
        {
            if (!StringExtensions.IsPackageID(value))
            {
                throw new WesException("PackageId 不合法");
            }

            base.Vm.SelfInfo.PackageId = value;
            base.Vm.SelfInfo.CartonNo = value;
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, PRINT_CARTON_LABEL_SCRIPT_ID, true)
                .AddQueryParameter("TruckOrder", base.Vm.SelfInfo.TruckOrder)
                .AddQueryParameter("PackageId", value)
                .AddQueryParameter("Consignee", base.Vm.SelfInfo.Consignee)
                .Execute().To<object>();
            Vm.SelfInfo.CartonSn = result.CartonSn;
            Vm.SelfInfo.PartNo = result.PartNo;
            Vm.SelfInfo.Ctns = result.Ctns;
            Vm.SelfInfo.minQty = result.minQty;
            Vm.SelfInfo.SumQty = result.SumQty;
            Vm.SelfInfo.Field2 = result.Field2;
            Vm.SelfInfo.Field3 = result.Field3;
            Vm.SelfInfo.CNO = result.CartonSn.ToString().PadLeft(5, '0') + "/" + result.Ctns.ToString().PadLeft(5, '0');
            Vm.SelfInfo.CartonInfo = result.CartonInfo;
            Vm.SelfInfo.HasGW = true;
            Vm.SelfInfo.ScanStatus = result.ScanStatus;
            Vm.SelfInfo.LabelCount = 0;
            Vm.SelfInfo.CartonSize = result.CartonSize;
            Vm.SelfInfo.GW = result.GW;
            Vm.SelfInfo.IsPrint = false;
            //如果已打过箱标,判断是否有权限重新打印
            if ((int) Vm.SelfInfo.ScanStatus == 3)
            {
                base.Vm.SelfInfo.IsPrint = MasterAuthorService.Authorization(VerificationType.Print);
                if (!Vm.SelfInfo.IsPrint) return;
            }

            double _gw = 0;
            if (result.GW == null || !double.TryParse(result.GW.ToString(), out _gw) || _gw <= 0)
            {
                Vm.SelfInfo.HasGW = false;
                Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_GW);
            }
            else
            {
                dynamic dimension = RestApi.NewInstance(Method.GET)
                    .AddUriParam(RestUrlType.WmsServer, CHECK_DIMENSION_SCRIPT_ID, null)
                    .AddQueryParameter("dimension", result.CartonSize)
                    .Execute().To<object>();
                Vm.SelfInfo.GW = _gw;
                Vm.SelfInfo.CartonSize = dimension.dimension;
                Vm.SelfInfo.Dimension = dimension.dimension;
                if (value.IsNotOriginalPackage())
                {
                    Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_DIM);
                }
                else
                {
                    PrintLabel();
                    Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
                }
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

            if (!double.TryParse(value, out gw) || gw < MIN_GW)
            {
                throw new WesException("GW數值必须是在0.01—99.99之間(不超過100,小數位不可超过2位)");
            }

            if (!double.TryParse(value, out gw) || gw > MAX_GW)
            {
                throw new WesException("GW數值必须是在0.01—99.99之間(不超過100,小數位不可超过2位)");
            }


            Vm.SelfInfo.GW = gw;
            Vm.Next(WesFlowID.FLOW_ACTION_ENTRY_DIM);
        }

        [Ability(6418359187801120768, AbilityType.ScanAction)]
        public virtual void ScanFlowActionEntryDim(string value)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, CHECK_DIMENSION_SCRIPT_ID)
                .AddQueryParameter("dimension", value)
                .Execute().To<object>();
            Vm.SelfInfo.Dimension = result.dimension;
            Vm.SelfInfo.CartonSize = result.dimension;
            PrintLabel();
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        private void BindImage(dynamic imageList)
        {
            List<object> datas = new List<object>();
            foreach (var item in imageList)
            {
                datas.Add(new
                {
                    ImageUri = item.PhotoUrl.ToString(),
                    FileName = item.FileName.ToString(),
                    Desc = Path.GetFileNameWithoutExtension(item.FileName.ToString()),
                    Remark = item.Remarks == null ? "" : item.Remarks.ToString()
                });
            }

            this.Vm.SelfInfo.imageViewList = datas;
        }

        private void PrintLabel()
        {
            string consignee = (string) Vm.SelfInfo.Consignee;
            Dictionary<string, string> checkStrList = new Dictionary<string, string>();
            List<string> winstronCnees = new List<string>();
            winstronCnees.Add("C03887");
            winstronCnees.Add("C03895");
            winstronCnees.Add("C03896");
            winstronCnees.Add("C03897");
            winstronCnees.Add("C03898");
            winstronCnees.Add("C03899");
            winstronCnees.Add("C03900");
            winstronCnees.Add("C03901");
            winstronCnees.Add("C03902");
            winstronCnees.Add("C03903");
            winstronCnees.Add("C03904");
            winstronCnees.Add("C03905");
            winstronCnees.Add("C03993");
            winstronCnees.Add("C03994");
            winstronCnees.Add("C03906");
            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
            foreach (var item in Vm.SelfInfo.LabelInfo)
            {
                string labelName = item.LabelName.ToString();
                string fileName = labelName + ".btw";

                List<dynamic> cartons =
                    DynamicJson.DeserializeObject<List<dynamic>>(DynamicJson.SerializeObject(Vm.SelfInfo.CartonInfo));
                //尾数盘
                if ("AVNET_C03575_LASTCARTON".Equals(labelName, StringComparison.OrdinalIgnoreCase))
                {
                    //如果需要打尾数盘,  判断当前箱的Qty总数小于最小包规则 ,打尾数盘标
                    if ((double) Vm.SelfInfo.SumQty >= (double) Vm.SelfInfo.minQty)
                    {
                        continue;
                    }
                    else
                    {
                        Dictionary<string, object> dic = DynamicUtil.AppendDictionary(Vm.SelfInfo, null);
                        templates.Add(PrepareData(dic, fileName));
                        var carton = cartons.Where(o => !String.IsNullOrEmpty(o.LoadingNo.ToString())).FirstOrDefault();
                        string loadingNo = carton == null ? "" : carton.LoadingNo;
                        dic.Add("LoadingNo", loadingNo);
                        //insertOpLog("PrintCartonLabel", fileName, dic);
                    }
                }
                //箱标  
                else if ("CartonLabel".Equals(item.LabelType.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if ("C00339".Equals(consignee, StringComparison.OrdinalIgnoreCase))
                    {
                        //AVNET出给自己的 一箱一料,直接汇总
                        var cartonByPartNO = (from ct in cartons
                            group ct by ct.PartNO
                            into g
                            select new {PartNO = g.Key, count = g.Count(), qty = g.Sum(c => c.Qty)}).ToList();
                        int i = 1;
                        foreach (var cbp in cartonByPartNO)
                        {
                            dynamic carton = cartons.Where(c => c.PartNO == cbp.PartNO).First();
                            carton.CartonSN = carton.ShipRemark;
                            Dictionary<string, object> dic = DynamicUtil.AppendDictionary(carton, Vm.SelfInfo);
                            dic["Qty"] = cbp.qty;

                            templates.Add(PrepareData(dic, fileName));
                            insertOpLog("PrintCartonLabel", labelName, dic);
                            i++;
                        }
                    }
                    else
                    {
                        // winstron
                        if (winstronCnees.Contains(consignee.ToUpper()))
                        {
                            //group by CPN 查找Qty  箱标by  CPN 出
                            var cartonByCpn = (from ct in cartons
                                group ct by ct.CPN
                                into g
                                select new {CPN = g.Key, count = g.Count(), qty = g.Sum(c => c.Qty)}).ToList();

                            foreach (var cbc in cartonByCpn)
                            {
                                dynamic carton = cartons.Where(c => c.CPN == cbc.CPN).First();
                                carton.CartonSN = carton.Shipremark;
                                Dictionary<string, object> dic = DynamicUtil.AppendDictionary(carton, Vm.SelfInfo);
                                dic["Qty"] = cbc.qty;
                                //添加需要check的Qty号码
                                checkStrList.Add("Qty", dic["Qty"].ToString());
                                templates.Add(PrepareData(dic, fileName));
                                insertOpLog("PrintCartonLabel", labelName, dic);
                            }
                        }
                        else
                        {
                            //group by DC 查找Qty  箱标by  DateCode 出
                            var cartonByDC = (from ct in cartons
                                    group ct by ct.DateCode
                                    into g
                                    select new {DateCode = g.Key, count = g.Count(), qty = g.Sum(c => c.Qty)}
                                ).ToList();
                            int i = 1;
                            foreach (var cbd in cartonByDC)
                            {
                                dynamic carton = cartons.Where(c => c.DateCode == cbd.DateCode).First();
                                carton.CartonSN = carton.Shipremark;
                                Dictionary<string, object> dic = DynamicUtil.AppendDictionary(carton, Vm.SelfInfo);
                                dic["Qty"] = cbd.qty;
                                //添加需要check的Field4+DateCode号码
                                checkStrList.Add("PNDC" + i.ToString(),
                                    dic["Field4"].ToString() + dic["DateCode"].ToString());
                                templates.Add(PrepareData(dic, fileName));
                                insertOpLog("PrintCartonLabel", labelName, dic);
                                i++;
                            }
                        }
                    }
                }
                //唛头
                else if ("MarkLabel".Equals(item.LabelType.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    //By NInvoiceNo  (DO) 出 唛头
                    var cartonByInv = (from ct in cartons
                            group ct by ct.NInvoiceNo
                            into g
                            select new {NInvoiceNo = g.Key, count = g.Count(), qty = g.Sum(c => c.Qty)}
                        ).ToList();
                    bool printed = false;
                    int i = 1;
                    foreach (var cbi in cartonByInv)
                    {
                        dynamic carton = cartons.Where(c => c.NInvoiceNo == cbi.NInvoiceNo).First();
                        Dictionary<string, object> dic = DynamicUtil.AppendDictionary(carton, Vm.SelfInfo);
                        dic["Qty"] = cbi.qty;
                        double gw = Double.Parse(dic["GW"].ToString());
                        gw = Math.Round(gw, 2);
                        dic["GW"] = gw.ToString("0.00");
                        //如果当前箱出两张唛头 第一张显示实际GW 以后都显示 0
                        if (printed)
                        {
                            dic["GW"] = "0.00";
                        }

                        checkStrList.Add("PKG ID" + i.ToString(), "(3S)" + dic["NInvoiceNo"].ToString());
                        templates.Add(PrepareData(dic, fileName));
                        insertOpLog("MaiTouLabel", labelName, dic);
                        printed = true;
                        i++;
                    }
                }
            }

            Print(templates, checkStrList);
            if (Vm.SelfInfo.ScanStatus != 3)
            {
                UpdateInfo();
                base.Vm.SelfInfo.NeedAddPlus = true;
                SetActionValid();
            }
        }

        private PrintTemplateModel PrepareData(Dictionary<string, object> dic, string fileName)
        {
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.TemplateFileName = fileName;
            pm.PrintDataValues = dic;
            Vm.SelfInfo.LabelCount++;
            return pm;
        }

        private void Print(List<PrintTemplateModel> templates, Dictionary<string, string> checkStrList)
        {
            LabelPrintBase lpb = new LabelPrintBase(templates);
            if (checkStrList != null && checkStrList.Count > 2) //大于2时检查箱标
            {
                lpb.PrintParam.RiseCheckLabel = Wes.Print.Model.PrintCheckType.PrintComplated;
                lpb.PrintParam.LabelCheckDatas = checkStrList;
            }

            var res = lpb.Print();
        }

        private void UpdateInfo()
        {
            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, UPDATE_INFO_SCRIPT_ID, true)
                .AddJsonBody("TruckOrder", Vm.SelfInfo.TruckOrder)
                .AddJsonBody("PackageId", Vm.SelfInfo.PackageId)
                .AddJsonBody("EndCustomer", Vm.SelfInfo.EndCustomer.ToString())
                .AddJsonBody("PartNo", Vm.SelfInfo.PartNo.ToString())
                .AddJsonBody("Qty", Vm.SelfInfo.SumQty.ToString())
                .AddJsonBody("GW", Vm.SelfInfo.GW.ToString())
                .AddJsonBody("CartonSize", Vm.SelfInfo.CartonSize.ToString())
                .AddJsonBody("HasGW", Vm.SelfInfo.HasGW)
                .AddJsonBody("UserName", WesDesktop.Instance.User.Code)
                .Execute().To<object>();
        }

        private void insertOpLog(string actionType, string labelName, Dictionary<string, object> dic)
        {
            RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, CARTON_LABEL_LOG_SCRIPT_ID, true)
                .AddJsonBody("LoadingNo", dic["LoadingNo"].ToString())
                .AddJsonBody("CartonNo", Vm.SelfInfo.PackageId.ToString())
                .AddJsonBody("PrintType", actionType.ToString())
                .AddJsonBody("Title", dic["CartonSn"].ToString())
                .AddJsonBody("Cpn", dic["CPN"].ToString())
                .AddJsonBody("PartNo", dic["PartNo"].ToString())
                .AddJsonBody("LotNo", dic["LotNos"].ToString())
                .AddJsonBody("Qty", dic["Qty"].ToString())
                .AddJsonBody("ReferenceNo", labelName.Length > 50 ? labelName.Substring(0, 50) : labelName)
                .AddJsonBody("DateCode", dic["DateCode"].ToString())
                .AddJsonBody("Po", dic.ContainsKey("PO") ? dic["PO"] : "")
                .AddJsonBody("CargoType", "")
                .AddJsonBody("Cno", dic["CNO"].ToString())
                .AddJsonBody("ShippingDate", dic["ShippingDate"].ToString())
                .AddJsonBody("OrignCountry", dic["OriginCountry"].ToString())
                .AddJsonBody("ReceivingNo", "")
                .AddJsonBody("ShipperName", "AvNet")
                .AddJsonBody("OriginName", dic["OriginCountry"].ToString())
                .AddJsonBody("EdpKeyRule", dic["Field4"].ToString())
                .AddJsonBody("InvoiceNo", dic.ContainsKey("NInvoiceNo") ? dic["NInvoiceNo"] : "")
                .AddJsonBody("PrintCount", 0)
                .Execute().To<object>();
            dynamic result = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, INSERT_OP_LOG_SCRIPT_ID, true)
                .AddJsonBody("logType", "shipping")
                .AddJsonBody("operationNo", Vm.SelfInfo.TruckOrder)
                .AddJsonBody("operationNo2", Vm.SelfInfo.PackageId)
                .AddJsonBody("operationNo3", labelName)
                .AddJsonBody("opType", "CartonLabelPrint")
                .AddJsonBody("opValue", Vm.SelfInfo.PartNo)
                .AddJsonBody("opPerson", WesDesktop.Instance.User.Code)
                .AddJsonBody("user", WesDesktop.Instance.User.Code)
                .AddJsonBody("remarks", "貼箱標")
                .Execute().To<object>();
        }
    }
}