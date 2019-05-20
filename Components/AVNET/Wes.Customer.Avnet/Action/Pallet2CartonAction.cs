using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities.Exception;
using Wes.Utilities.Extends;
using Wes.Wrapper;

namespace Wes.Customer.Avnet.Action
{
    public class Pallet2CartonAction : ScanActionBase<WesFlowID, Pallet2CartonAction>, IScanAction
    {
        public const long CheckReceivingNoScriptId = 6413590053024436224L;
        public const long ScanPalletIdScriptId = 6415166038958149632L;
        public const long RecevingEndScriptId = 6416104193391595520L;
        public const long DeletePackageIdScriptId = 6417305825848594432L;
        public const long CheckCartonNoScriptId = 6415132002676117504L;
        public const long CheckCanPrintScriptId = 6432845863915098112L;


        [Ability(6415479801422815232, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanReceivingNo(string value)
        {
            value = value.ToUpper();
            base.Vm.SelfInfo.ReceivingNo = value;
            base.Vm.SelfInfo.IsDelete = false;
            base.Vm.SelfInfo.LabelCount = 0;

            dynamic result = RestApi.NewInstance(Method.GET)
                    .AddUriParam(RestUrlType.WmsServer, CheckReceivingNoScriptId, true)
                    .AddQueryParameter("ReceivingNo", value)
                    .Execute().To<object>();

            base.Vm.SelfInfo.EndCustomer = result.EndCustomer;
            base.Vm.SelfInfo.Supplier = result.Supplier;
            base.Vm.SelfInfo.Ctns = result.Ctns;
            base.Vm.SelfInfo.Plts = result.Plts;
            base.Vm.SelfInfo.LCtns = result.LCtns;
            base.Vm.SelfInfo.EndCustomerName = result.EndCustomerName;
            base.Vm.SelfInfo.Target = result.Supplier;

            if ((int)result.Ctns == 0)
            {
                WesModernDialog.ShowWesMessage("已完成");
                base.Vm.SelfInfo.ReceivingInfo = result.ReceivingInfo;
            }
            else
            {
                this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PALLET_ID);
                SetActionValid();
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
            base.Vm.SelfInfo.LabelCount = 0;
            dynamic result = RestApi.NewInstance(Method.GET)
                    .AddUriParam(RestUrlType.WmsServer, ScanPalletIdScriptId, true)
                    .AddQueryParameter("ReceivingNo", base.Vm.SelfInfo.ReceivingNo)
                    .AddQueryParameter("PalletId", value)
                    .Execute().To<object>();

            base.Vm.SelfInfo.IsDelete = false;
            base.Vm.SelfInfo.ReceivingInfo = result.ReceivingInfo;
            //if (result.ReceivingInfo != null && result.ReceivingInfo.Count > 0)
            //{
            //    this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PALLET_ID);
            //    throw new WesException(string.Format("{0} 已轉 {1} 箱 \n請掃描其他PalletId 或刪除本板箱數據后重新掃描該palletId", base.Vm.SelfInfo.PalletId, result.ReceivingInfo.Count));
            //}
            //else
            //{
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_TOTAL_RECEIVE);
            //}
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
                    .AddUriParam(RestUrlType.WmsServer, CheckCartonNoScriptId, true)
                    .AddQueryParameter("ReceivingNo", Vm.SelfInfo.ReceivingNo)
                    .AddQueryParameter("PalletId", Vm.SelfInfo.PalletId)
                    .AddQueryParameter("Total", total)
                    .Execute().To<object>();

            base.Vm.SelfInfo.ReceivingInfo = result.ReceivingInfo;

            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
            //有掃版重打的情況,故此處打印最後累加的部分(查詢時已經按照箱號排序,取最後 total個即可)
            for (int i = result.ReceivingInfo.Count; i > result.ReceivingInfo.Count - total; i--)
            {
                var item = result.ReceivingInfo[i - 1];
                if (string.IsNullOrEmpty(item.PackageID.ToString()))
                {
                    continue;
                }
                templates.Add(PrepareData(item.PackageID.ToString()));
            }
            PrintData(templates);

            if (result.Ctns != null && (int)result.Ctns == 0)
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
                    .AddUriParam(RestUrlType.WmsServer, RecevingEndScriptId, true)
                    .AddQueryParameter("ReceivingNo", base.Vm.SelfInfo.ReceivingNo)
                    .AddQueryParameter("Plts", base.Vm.SelfInfo.Plts)
                    .AddQueryParameter("Ctns", base.Vm.SelfInfo.Ctns)
                    .AddQueryParameter("LCtns", base.Vm.SelfInfo.LCtns)
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

            dynamic result = RestApi.NewInstance(Method.DELETE)
            .AddUriParam(RestUrlType.WmsServer, DeletePackageIdScriptId, true)
            .AddJsonBody("ReceivingNo", base.Vm.SelfInfo.ReceivingNo)
            .AddJsonBody("PackageId", id)
            .Execute();


            result = result.To<object>();

            if (result)
            {
                foreach (var item in Vm.SelfInfo.ReceivingInfo)
                {
                    if (id.Equals(item.PackageID.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        base.Vm.SelfInfo.ReceivingInfo.Remove(item);
                        break;
                    }
                }
            }
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
            pm.PrintDataValues.Add("InvoiceNo", base.Vm.SelfInfo.EndCustomerName);
            pm.TemplateFileName = "PackageId.btw";
            base.Vm.SelfInfo.LabelCount++;
            return pm;
        }

        public void PrintData(List<PrintTemplateModel> templates)
        {
            //InvoiceNo
            LabelPrintBase lpb = new LabelPrintBase(templates, false);
            var res = lpb.Print();
            SetActionValid();
        }
    }
}
