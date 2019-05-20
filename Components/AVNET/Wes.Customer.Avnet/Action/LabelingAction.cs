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
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Avnet.Action
{
    public class LabelingAction : ScanActionBase<WesFlowID, LabelingAction>, IScanAction
    {
        [Ability(6421990780402929664, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanTruckNo(string scanVal)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, 6420891050830729216, false)
                .AddQueryParameter("txt", scanVal)
                .Execute()
                .To<object>();

            Vm.SelfInfo.txt = scanVal;
            Vm.SelfInfo.Target = (string) result.consignee;
            Vm.SelfInfo.consignee = (string) result.consignee;
            BindImage(Vm, "REEL", "BOX");
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
            SetActionValid();
        }

        [Ability(6421990763017543680, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPackageId(String scanVal)
        {
            LoadingCarton(scanVal);

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        [Ability(6421990744264806400, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanMpn(String scanVal)
        {
            Vm.SelfInfo.spn = scanVal;
            var result = RestApi.NewInstance(Method.POST)
                .SetWmsCustomerUri("/resolver/spn")
                .AddParams(Vm.GetSelfInfo())
                .Execute()
                .To<dynamic>();
            Vm.SelfInfo.mpn = result.spn;
            Vm.SelfInfo.pn = result.partNo;

            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QRCODE);
        }

        [Ability(6421990725533048832, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanQrcode(string scanVal)
        {
            Vm.SelfInfo.qrCode = scanVal;
            var qcResult = RestApi.NewInstance(Method.POST)
                .SetWmsCustomerUri("/resolver/qc")
                .AddParams(Vm.GetSelfInfo())
                .Execute()
                .To<dynamic>();
            Vm.SelfInfo.lot = qcResult.lot;
            Vm.SelfInfo.opn = qcResult.opn;
            Vm.SelfInfo.dc = qcResult.dc;
            Vm.SelfInfo.qty = qcResult.qty;
            Vm.SelfInfo.dt = qcResult.dt;
            SaveLabeling(scanVal);
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        public virtual void DeleteBoxLabel(dynamic model)
        {
            if (!Vm.SelfInfo.isMasterDelete)
                Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!Vm.SelfInfo.isMasterDelete) return;

            var result = WesModernDialog.ShowWesMessage("[盒]盒标多行会被删除, 您确定要删除吗?", "WES_Message".GetLanguage(),
                System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                RestApi.NewInstance(Method.DELETE)
                    .AddUriParam(RestUrlType.WmsServer, 6419817329495904256, true)
                    .AddJsonBody("rid", (string) model.rid)
                    .AddJsonBody("txt", (string) Vm.SelfInfo.txt)
                    .AddJsonBody("pid", (string) Vm.SelfInfo.pid)
                    .AddJsonBody("sxt", (string) model.sxt)
                    .AddJsonBody("isDeletePlate", false)
                    .Execute();

                this.LoadingCarton(Vm.SelfInfo.pid);
            }
        }

        public virtual void DeletePlateLabel(dynamic model)
        {
            if (!Vm.SelfInfo.isMasterDelete)
                Vm.SelfInfo.isMasterDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            if (!Vm.SelfInfo.isMasterDelete) return;

            var result = WesModernDialog.ShowWesMessage("[盘]您确定要删除盘标吗?", "WES_Message".GetLanguage(),
                System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                RestApi.NewInstance(Method.DELETE)
                    .AddUriParam(RestUrlType.WmsServer, 6419817329495904256, true)
                    .AddJsonBody("rid", (string) model.rid)
                    .AddJsonBody("txt", (string) Vm.SelfInfo.txt)
                    .AddJsonBody("sxt", (string) model.sxt)
                    .AddJsonBody("pid", (string) Vm.SelfInfo.pid)
                    .AddJsonBody("isDeletePlate", true)
                    .Execute();

                this.LoadingCarton(Vm.SelfInfo.pid);
            }
        }

        public virtual void Reprint(long rid)
        {
            if (!Vm.SelfInfo.isMasterReprint)
                Vm.SelfInfo.isMasterReprint = MasterAuthorService.Authorization(VerificationType.Print);
            if (!Vm.SelfInfo.isMasterReprint) return;

            dynamic label = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, 6420563238571745280, true)
                .AddJsonBody("rid", rid)
                .Execute()
                .To<object>();

            if (Vm.SelfInfo.isLabeling)
            {
                Print(label, true);
            }
        }

        [Ability(6422073996916826112, AbilityType.ScanAction)]
        public virtual void BoxEnd(string scanVal)
        {
            PrivateBoxBnd(scanVal);

            if (Vm.SelfInfo.overQty == 0)
            {
                this.CartonEnd(scanVal);
            }

            Vm.SelfInfo.integralOperationNo = Vm.SelfInfo.txt;
            Vm.SelfInfo.integralTotal = 1;
            Vm.SelfInfo.integralIsPlus = false;
            SetActionValid();
        }

        [Ability(6422073996916826112, AbilityType.ScanAction)]
        public virtual void NoBox(string scanVal)
        {
            PrivateBoxBnd(scanVal);

            if (Vm.SelfInfo.overQty == 0)
            {
                CartonEnd(scanVal);
            }
        }

        [Ability(6437992870661070848, AbilityType.ScanAction)]
        public virtual void CartonEnd(string scanVal)
        {
            RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, 6437991668514820096, true)
                .AddJsonBody("txt", Vm.SelfInfo.txt)
                .AddJsonBody("pid", Vm.SelfInfo.pid)
                .Execute();

            Vm.SelfInfo.integralOperationNo = Vm.SelfInfo.txt;
            Vm.SelfInfo.integralTotal = 1;
            Vm.SelfInfo.integralIsPlus = true;
            SetActionValid();
        }

        [Ability(6433231853867503616, AbilityType.ScanAction)]
        public virtual void SaveLabeling(string scanVal)
        {
            dynamic label = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, 6417675992126464000, true)
                .AddJsonBody(Vm.SelfInfo)
                .Execute()
                .To<object>();

            if ((bool) label.isLabeling)
            {
                Print(label, true);
            }

            Vm.SelfInfo.integralOperationNo = (String) Vm.SelfInfo.txt;
            Vm.SelfInfo.integralTotal = 1;
            Vm.SelfInfo.integralIsPlus = false;
            SetActionValid();

            this.LoadingCarton(Vm.SelfInfo.pid);
        }

        private void Print(dynamic label, bool isLock)
        {
            if (label.context == null)
                throw new WesException("获取标签数据失败，请稍后再试");

            PrintTemplateModel ptm = new PrintTemplateModel();
            ptm.TemplateFileName = label.labelName + ".btw";
            ptm.PrintData = label.context;
            LabelPrintBase labelPrint = new LabelPrintBase(ptm);
            labelPrint.Print();
        }

        private void PrivateBoxBnd(String scanValue)
        {
            String boxType = scanValue.ToUpper();
            List<dynamic> labels = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, 6420486579894099968, true)
                .AddJsonBody("txt", Vm.SelfInfo.txt)
                .AddJsonBody("pid", Vm.SelfInfo.pid)
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
            Vm.Next(WesFlowID.FLOW_ACTION_SCAN_MPN);
        }

        private void LoadingCarton(String pid)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, 6417182548757057536, true)
                .AddQueryParameter("txt", base.Vm.SelfInfo.txt)
                .AddQueryParameter("pid", pid)
                .Execute()
                .To<object>();

            Vm.SelfInfo.pid = (string) pid;
            Vm.SelfInfo.carton = (string) pid;
            Vm.SelfInfo.overQty = (int) result.overQty;
            Vm.SelfInfo.totalQty = (int) result.totalQty;
            Vm.SelfInfo.isLabeling = (bool) result.isLabeling;
            Vm.SelfInfo.cartonList = result.cartonList;
            Vm.SelfInfo.panList = result.panList;
            Vm.SelfInfo.isMasterReprint = false;
            Vm.SelfInfo.isMasterDelete = false;

            if (!(bool) result.isLabeling)
            {
                Vm.SelfInfo.OnlyCheckMessage = "(Checking)";
            }
            else
            {
                Vm.SelfInfo.OnlyCheckMessage = "(Labeling)";
            }
        }

        public static void BindImage(dynamic vm, params string[] labelImageType)
        {
            vm.SelfInfo.labelImageType = labelImageType.ToList();

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