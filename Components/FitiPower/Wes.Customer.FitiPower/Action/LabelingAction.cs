using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.FitiPower.API;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.FitiPower.Action
{
    public class LabelingAction : ScanActionBase<WesFlowID, LabelingAction>, IScanAction
    {
        public virtual void ScanFlowActionScanLoadingNo(string scanVal)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.LABELING_LOADING_NO, false)
                .AddQueryParameter("sxt", scanVal)
                .Execute()
                .To<Object>();

            base.Vm.SelfInfo.sxt = scanVal;
            base.Vm.SelfInfo.consignee = (string) result.consignee;
            base.Vm.SelfInfo.relabelList = result.relabelList;

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            BindImage(base.Vm, "REEL", "BOX");
        }

        public virtual void ScanFlowActionScanPackageId(String scanVal)
        {
            this.LoadingCarton(scanVal);

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        public virtual void FlowStart(string scanVal)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .SetUrl(RestUrlType.SidServer)
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.waitingSid = (long) result;
        }

        public virtual void FlowEnd(string scanVal)
        {
            base.Vm.SelfInfo.reelingCommand = scanVal;

            List<dynamic> labels = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.LABELING_REEL_END, true)
                .AddJsonBody(base.Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            int i = 0;
            foreach (var label in labels)
            {
                if ((bool)label.isLabeling)
                {
                    if (i == labels.Count - 1)
                    {
                        Print(label, true);
                    }
                    else
                    {
                        Print(label, false);
                    }
                }

                i++;
            }

            this.LoadingCarton(this.Vm.SelfInfo.pid);
            base.Vm.SelfInfo.waitingSid = null;
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        public virtual void ScanFlowActionScanMpn(string scanVal)
        {
            if (scanVal.Contains("_&_"))
            {
                this.ScanFlowActionScanQrcode(scanVal);
            }
            else
            {
                //resolver mpn
                string mpn = RestApi.NewInstance(Method.GET)
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DECODE_DISPATCH)
                    .AddBranch("MPN")
                    .AddQueryParameter(base.Vm.GetSelfInfo())
                    .AddQueryParameter("mpn", scanVal)
                    .Execute()
                    .To<string>();

                //check mpn
                dynamic rules = RestApi.NewInstance(Method.GET)
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.PART_RULES)
                    .AddQueryParameter(base.Vm.GetSelfInfo())
                    .AddQueryParameter("mpn", mpn)
                    .Execute()
                    .To<object>();

                base.Vm.SelfInfo.mpn = rules.mpn;
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
            }
        }

        public virtual void ScanFlowActionScanQty(string scanVal)
        {
            //decode qty
            dynamic qty = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DECODE_DISPATCH)
                .AddBranch("QTY")
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("mpn", base.Vm.SelfInfo.mpn)
                .AddQueryParameter("qty", scanVal)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.qty = (int) qty;

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO);
        }

        public virtual void ScanFlowActionScanDcNo(string scanVal)
        {
            //decode dc
            dynamic dc = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DECODE_DISPATCH)
                .AddBranch("DC")
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("mpn", base.Vm.SelfInfo.mpn)
                .AddQueryParameter("qty", base.Vm.SelfInfo.qty)
                .AddQueryParameter("dc", scanVal)
                .Execute()
                .To<string>();

            //resolver dc
            dynamic dt = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DATE_CODE_DISPATCH)
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("dc", dc)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.dc = (string) dc;
            base.Vm.SelfInfo.dt = (string) dt;

            this.SaveLabeing(scanVal);

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        public virtual void ScanFlowActionScanLotNo(string scanVal)
        {
            //decode lot
            dynamic lot = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.DECODE_DISPATCH)
                .AddBranch("LOT")
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("mpn", base.Vm.SelfInfo.mpn)
                .AddQueryParameter("qty", base.Vm.SelfInfo.qty)
                .AddQueryParameter("dc", base.Vm.SelfInfo.dc)
                .AddQueryParameter("lot", scanVal)
                .Execute()
                .To<string>();

            base.Vm.SelfInfo.lot = (string) lot;

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY);
        }

        public virtual void ScanFlowActionScanQrcode(string scanVal)
        {
            //解析QrCode
            dynamic qrCode = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.QR_CODE_DISPATCH)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .AddJsonBody("qrCode", scanVal)
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.cpo = qrCode.cpo;
            base.Vm.SelfInfo.cpn = qrCode.cpn;
            base.Vm.SelfInfo.mpn = qrCode.mpn;

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
        }

        [AbilityAble(true, KPIActionType.LSLabeling, "consignee")]
        public virtual bool BoxEnd(string scanVal)
        {
            PrivateBoxBnd(scanVal);

            if (base.Vm.SelfInfo.overQty == 0)
            {
                this.CartonEnd(scanVal);
            }

            return true;
        }

        public virtual void NoBox(string scanVal)
        {
            PrivateBoxBnd(scanVal);

            if (base.Vm.SelfInfo.overQty == 0)
            {
                this.CartonEnd(scanVal);
            }
        }

        [AbilityAble(true, KPIActionType.LSLabelingPlus, "consignee")]
        public virtual bool CartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.LABELING_CARTON_END, true)
                .AddJsonBody("sxt", base.Vm.SelfInfo.sxt)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .Execute();

            return true;
        }

        [AbilityAble(true, KPIActionType.LSLabeling, "consignee")]
        public virtual bool SaveLabeing(string scanVal)
        {
            dynamic label = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.LABELING_SAVE, true)
                .AddJsonBody(this.Vm.SelfInfo)
                .Execute()
                .To<object>();

            this.LoadingCarton(this.Vm.SelfInfo.pid);

            if ((bool) label.isLabeling)
            {
                Print(label, true);
            }

            return true;
        }

        public virtual void DeleteBoxLabel(dynamic model)
        {
            if (!base.Vm.SelfInfo.isMasterDelete)
                base.Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!base.Vm.SelfInfo.isMasterDelete) return;

            var result = WesModernDialog.ShowWesMessage("[盒]盒标多行会被删除, 您确定要删除吗?", "WES_Message".GetLanguage(),
                System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                RestApi.NewInstance(Method.DELETE)
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.LABELING_DELETE, true)
                    .AddJsonBody("rid", (string) model.rid)
                    .AddJsonBody("sxt", (string) base.Vm.SelfInfo.sxt)
                    .AddJsonBody("pid", (string) base.Vm.SelfInfo.pid)
                    .AddJsonBody("isDeletePlate", false)
                    .Execute();

                this.LoadingCarton(this.Vm.SelfInfo.pid);
            }
        }

        public virtual void DeletePlateLabel(dynamic model)
        {
            if (!base.Vm.SelfInfo.isMasterDelete)
                base.Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!base.Vm.SelfInfo.isMasterDelete) return;

            var result = WesModernDialog.ShowWesMessage("[盘]您确定要删除盘标吗?", "WES_Message".GetLanguage(),
                System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                RestApi.NewInstance(Method.DELETE)
                    .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.LABELING_DELETE, true)
                    .AddJsonBody("rid", (string) model.rid)
                    .AddJsonBody("sxt", (string) base.Vm.SelfInfo.sxt)
                    .AddJsonBody("pid", (string) base.Vm.SelfInfo.pid)
                    .AddJsonBody("isDeletePlate", true)
                    .Execute();

                this.LoadingCarton(this.Vm.SelfInfo.pid);
            }
        }

        public virtual void Reprint(long rid)
        {
            if (!base.Vm.SelfInfo.isMasterReprint)
                base.Vm.SelfInfo.isMasterReprint = MasterAuthorService.Authorization(VerificationType.Print);
            if (!base.Vm.SelfInfo.isMasterReprint) return;

            dynamic label = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.LABELING_REPRINT, true)
                .AddJsonBody("rid", rid)
                .Execute()
                .To<object>();

            if (base.Vm.SelfInfo.isLabeling)
            {
                Print(label, true);
            }
        }

        private void Print(dynamic label, bool isLock)
        {
            PrintTemplateModel ptm = new PrintTemplateModel();
            ptm.TemplateFileName = label.labelName + ".btw";
            ptm.PrintData = label.context;
            Vm.SelfInfo.labelCount = 0;
            for (int i = 0; i < (int) label.printNum; i++)
            {
                LabelPrintBase labelPrint = new LabelPrintBase(ptm);
                labelPrint.Print();
                Vm.SelfInfo.labelCount++;
            }
        }

        private void PrivateBoxBnd(String scanValue)
        {
            String boxType = scanValue.ToUpper();
            List<dynamic> labels = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.LABELING_BOX_END, true)
                .AddJsonBody("sxt", this.Vm.SelfInfo.sxt)
                .AddJsonBody("pid", this.Vm.SelfInfo.pid)
                .AddJsonBody("boxType", boxType)
                .Execute()
                .To<List<object>>();
            int i = 0;
            foreach (var label in labels)
            {
                if ((bool) label.isLabeling)
                {
                    if (i == labels.Count - 1)
                    {
                        Print(label, true);
                    }
                    else
                    {
                        Print(label, false);
                    }
                }

                i++;
            }

            this.LoadingCarton(this.Vm.SelfInfo.pid);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        private void LoadingCarton(String pid)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long) ScriptSid.LABELING_PACKAGE, true)
                .AddQueryParameter("sxt", base.Vm.SelfInfo.sxt)
                .AddQueryParameter("pid", pid)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.pid = (string) pid;
            base.Vm.SelfInfo.carton = (string) pid;
            base.Vm.SelfInfo.overQty = (int) result.overQty;
            base.Vm.SelfInfo.isLabeling = (bool) result.isLabeling;
            base.Vm.SelfInfo.cartonList = result.cartonList;
            base.Vm.SelfInfo.panList = result.panList;
            base.Vm.SelfInfo.isMasterReprint = false;
            base.Vm.SelfInfo.isMasterDelete = false;

            if (!(bool) result.isLabeling)
            {
                base.Vm.SelfInfo.OnlyCheckMessage = "(Checking)";
            }
            else
            {
                base.Vm.SelfInfo.OnlyCheckMessage = "(Labeling)";
            }
        }

        public static void BindImage(dynamic vm, params string[] labelImageType)
        {
            vm.SelfInfo.labelImageType = labelImageType.ToList<string>();

            var result = RestApi.NewInstance(Method.POST)
                .SetUrl(RestUrlType.WmsServer, "/common/load-image")
                .AddJsonBody(vm.GetSelfInfo())
                .Execute()
                .To<dynamic>();
            var viewData = new List<dynamic>();
            foreach (var item in result)
            {
                viewData.Add(new
                {
                    ImageUri = item.photoUrl.ToString(),
                    FileName = item.fileName.ToString(),
                    Desc = Path.GetFileNameWithoutExtension(item.fileName.ToString()),
                    Remark = item.remarks == null ? "" : item.remarks.ToString()
                });
            }

            vm.SelfInfo.imageViewList = viewData;
        }
    }
}