using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Wrapper;

namespace Wes.Component.Widgets.Action
{
    public class Pallet2CartonAction : ScanActionBase<WesFlowID, Pallet2CartonAction>, IScanAction
    {
        public const string PALLET2CARTON = "pallet2carton/";

        public const string CHECKRECEIVINGNO = PALLET2CARTON + "check-receiving-no";
        public const string CHECKPALLETID = PALLET2CARTON + "check-pallet-id";
        public const string CONVERT = PALLET2CARTON + "convert";
        public const string DELETECARTON = PALLET2CARTON + "delete-carton";
        public const string RCVEND = PALLET2CARTON + "rcv-end";


        [Ability(6415479801422815232, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanReceivingNo(string value)
        {
            value = value.ToUpper();
            base.Vm.SelfInfo.ReceivingNo = value;
            base.Vm.SelfInfo.IsDelete = false;

            dynamic result = RestApi.NewInstance(Method.GET)
                    .AddFlowUri(CHECKRECEIVINGNO)
                    .AddQueryParameter("receivingNo", value)
                    .Execute().To<object>();

            base.Vm.SelfInfo.Ctns = result.ctns;
            base.Vm.SelfInfo.Plts = result.plts;
            base.Vm.SelfInfo.LCtns = result.lCtns;
            base.Vm.SelfInfo.JobNo = result.jobNo;
            base.Vm.SelfInfo.Supplier = result.supplier;
            base.Vm.SelfInfo.Target = result.supplier;

            if ((int)result.ctns == 0)
            {
                WesModernDialog.ShowWesMessage("已完成");
                base.Vm.SelfInfo.ReceivingInfo = result.receivingInfo;
            }
            else
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PALLET_ID);
            }
        }


        [Ability(6415778447792017408, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPalletId(string value)
        {
            value = value.ToUpper();
            var reg = new Regex(@"^[x|X]\d{10}[p|P]\d{3}$");
            if (!reg.IsMatch(value))
            {
                throw new WesException("PalletId输入有误!");
            }

            base.Vm.SelfInfo.PalletId = value;
            dynamic result = RestApi.NewInstance(Method.GET)
                    .AddFlowUri(CHECKPALLETID)
                    .AddQueryParameter("receivingNo", base.Vm.SelfInfo.ReceivingNo)
                    .AddQueryParameter("palletId", value)
                    .Execute().To<object>();

            base.Vm.SelfInfo.IsDelete = false;
            base.Vm.SelfInfo.ReceivingInfo = result;
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_TOTAL_RECEIVE);
        }

        [Ability(6415780069146370048, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanTotalReceive(string value)
        {
            Regex reg = new Regex(@"^[1-9]\d*$");
            if (!reg.IsMatch(value))
            {
                throw new WesException("輸入箱數不正確");
            }
            int total = int.Parse(value);

            base.Vm.SelfInfo.TotalOfCarton = total;
            dynamic result = RestApi.NewInstance(Method.GET)
                    .AddFlowUri(CONVERT)
                    .AddQueryParameter("receivingNo", Vm.SelfInfo.ReceivingNo)
                    .AddQueryParameter("palletId", Vm.SelfInfo.PalletId)
                    .AddQueryParameter("total", total)
                    .AddQueryParameter("jobNo", Vm.SelfInfo.JobNo)
                    .Execute().To<object>();

            base.Vm.SelfInfo.ReceivingInfo = result.receivingInfo;

            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
            //有掃版重打的情況,故此處打印最後累加的部分(查詢時已經按照箱號排序,取最後 total個即可)
            for (int i = result.receivingInfo.Count; i > result.receivingInfo.Count - total; i--)
            {
                var item = result.receivingInfo[i - 1];
                if (string.IsNullOrEmpty(item.packageID.ToString()))
                {
                    continue;
                }
                templates.Add(PrepareData(item.packageID.ToString()));
            }

            PrintData(templates);

            if (result.ctns != null && (int)result.ctns == 0)
            {
                WesModernDialog.ShowWesMessage("完成");
                Vm.ResetUiStatus();
            }
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PALLET_ID);
        }

        /// <summary>
        /// RvcEnd
        /// </summary>
        /// <param name="value"></param>
        public virtual void RcvEnd(string value)
        {
            base.Vm.SelfInfo.TotalOfCarton = value;
            dynamic result = RestApi.NewInstance(Method.GET)
                    .AddFlowUri(RCVEND)
                    .AddQueryParameter("receivingNo", base.Vm.SelfInfo.ReceivingNo)
                    .AddQueryParameter("plts", base.Vm.SelfInfo.Plts)
                    .AddQueryParameter("ctns", base.Vm.SelfInfo.Ctns)
                    .AddQueryParameter("lctns", base.Vm.SelfInfo.LCtns)
                    .Execute().To<object>();
            if (result == 0)
            {
                WesModernDialog.ShowWesMessage("ReceivingEnd 完成");
                Vm.ResetUiStatus();
            }
        }


        [Ability(6432917738397634560, AbilityType.ScanAction)]
        public virtual void DeleteData(string id)
        {
            if (!base.Vm.SelfInfo.IsDelete)
            {
                base.Vm.SelfInfo.IsDelete = MasterAuthorService.Authorization(VerificationType.Delete);
            }

            if (!Vm.SelfInfo.IsDelete) return;

            dynamic result = RestApi.NewInstance(Method.GET)
            .AddFlowUri(DELETECARTON)
            .AddQueryParameter("receivingNo", base.Vm.SelfInfo.ReceivingNo)
            .AddQueryParameter("packageId", id)
            .Execute();


            result = result.To<object>();

            if (result)
            {
                foreach (var item in Vm.SelfInfo.ReceivingInfo)
                {
                    if (id.Equals(item.packageID.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        base.Vm.SelfInfo.ReceivingInfo.Remove(item);
                        base.Vm.SelfInfo.ReceivingInfo = DynamicJson.DeserializeObject<List<object>>(DynamicJson.SerializeObject(base.Vm.SelfInfo.ReceivingInfo));
                        break;
                    }
                }
            }

            Score();
        }
        [AbilityAble(6420101420141256704, AbilityType.ScanAction, true, KPIActionType.LSReceivingLabelling | KPIActionType.LSReceivingLabellingPlus, "Supplier", true)]
        public virtual bool Score()
        {
            return true;
        }

        public void ManualPrint(string id)
        {
            if (MasterAuthorService.Authorization())
            {
                List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
                templates.Add(PrepareData(id));
                LabelPrintBase lpb = new LabelPrintBase(templates, false);
                var res = lpb.Print();
            }
        }

        private PrintTemplateModel PrepareData(string id)
        {
            PrintTemplateModel pm = new PrintTemplateModel();
            pm.PrintDataValues = new Dictionary<string, object>();
            pm.PrintDataValues.Add("SplitPkgID", id);
            pm.SortKey = "SplitPkgID";
            pm.PrintDataValues.Add("InvoiceNo", WesDesktop.Instance.AddIn.EndCustomerName);
            pm.TemplateFileName = "PackageId.btw";
            return pm;
        }

        [AbilityAble(6443652930846334976, AbilityType.ScanAction, true, KPIActionType.LSReceivingLabelling | KPIActionType.LSReceivingLabellingPlus, "Supplier", false)]

        public virtual bool PrintData(List<PrintTemplateModel> templates)
        {
            Vm.SelfInfo.LabelCount = templates.Count;
            templates.Sort();
            LabelPrintBase lpb = new LabelPrintBase(templates, false);
            var res = lpb.Print();
            return true;
        }
    }
}
