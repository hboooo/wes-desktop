using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Wes.Component.Widgets.APIAddr;
using Wes.Core.Attribute;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows;
using Wes.Desktop.Windows.Controls;
using Wes.Flow;
using Wes.Print;
using Wes.Utilities;
using Wes.Utilities.Exception;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.Sinbon.Action
{
    public class LabelingAction : ScanActionBase<WesFlowID, LabelingAction>, IScanAction
    {
        #region 需要输入COO的consignee

        private HashSet<string> _cooConsignee = new HashSet<string>()
        {
            "C03723",
            "C03724",
            "C03726",
            "C03744",
            "C03746",
            "C03747",
            "C03749",
            "C03765",
            "C03768",
            "C03814",
            "C03787",
            "C03778",
            "C03738",
            "C03731",
            "C03732",
            "C03733",
            "C03851"

        };

        #endregion

        [Ability(6421990780402929664, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanLoadingNo(string scanVal)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "reel-labeling/reel-loadingNo")
                .AddQueryParameter("sxt", scanVal)
                .Execute()
                .To<Object>();
            
            base.Vm.SelfInfo.sxt = scanVal;
            base.Vm.SelfInfo.consignee = (string)result.consignee;
            LoadLaeblImages(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        private void LoadLaeblImages(string loadingNo)
        {
            try
            {
                dynamic result = RestApi.NewInstance(Method.GET)
                    .AddUri(RestUrlType.WmsServer, "reel-labeling/label-images")
                    .AddQueryParameter("loadingNo", loadingNo)
                    .Execute()
                    .To<Object>();
                BindImage(result);
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
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

        [Ability(6421990763017543680, AbilityType.ScanAction)]
        public virtual void ScanFlowActionScanPackageId(String scanVal)
        {
            this.LoadingCarton(scanVal);
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PN_OR_QRCODE);
        }

        public virtual void ScanFlowActionScanPnOrQrcode(string scanVal)
        {
            base.Vm.SelfInfo.keyIn = 1;
            if (string.Compare(base.Vm.SelfInfo.shipper.ToString(), "C03690", true) == 0 && scanVal.Length >= 30
            ) //supplier=C03690  QRCode以空格分割
            {
                ScanC03690Qrcode(scanVal);
                base.Vm.SelfInfo.keyIn = 0;
            }
            else if (string.Compare(base.Vm.SelfInfo.shipper.ToString(), "C02973", true) == 0 && scanVal.Length >= 30
            ) //supplier=C02973  QRCode以";"分割
            {
                ScanC02973Qrcode(scanVal);
                base.Vm.SelfInfo.keyIn = 0;
            }
            else if (string.Compare(base.Vm.SelfInfo.shipper.ToString(), "C02975", true) == 0 && scanVal.Length >= 30
            ) //supplier=C02975  QRCode以"[)>"开始
            {
                ScanC02975QrCode(scanVal);
                base.Vm.SelfInfo.keyIn = 0;
            }
            //CL Code 规则 3碼-4碼-1碼"-"2碼(共13碼，但QR CODE掃描為3碼-4碼-1碼"空格"2碼，請將"空格"自動轉換成"-") 
            //採CL CODE轉換PN，CL CODE = 580 - 2408 - 0 60(13碼，先將空格轉換為"-")
            else if (string.Compare(base.Vm.SelfInfo.shipper.ToString(), "C03690", true) == 0)
            {
                dynamic result = RestApi.NewInstance(Method.GET)
                    .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.Other_DISPATCH)
                    .AddBranch("CL_CODE")
                    .AddQueryParameter("clCode", scanVal)
                    .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                    .AddQueryParameter("shipper", base.Vm.SelfInfo.shipper)
                    .Execute()
                    .To<object>();
                base.Vm.SelfInfo.clCode = result.clCode;
                base.Vm.SelfInfo.pn = result.pn;
                base.Vm.SelfInfo.minQty = result.minQty;
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
            }
            //PN
            else
            {
                dynamic result = RestApi.NewInstance(Method.GET)
                    .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.Other_DISPATCH)
                    .AddBranch("PN")
                    .AddQueryParameter("pn", scanVal)
                    .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                    .AddQueryParameter("shipper", base.Vm.SelfInfo.shipper)
                    .Execute()
                    .To<object>();
                base.Vm.SelfInfo.pn = result.pn;
                base.Vm.SelfInfo.basPartNoRules = result.basePartNoRules;
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO);
            }
        }

        public virtual void ScanC03690Qrcode(string scanVal)
        {
            //解析QrCode
            dynamic qrCode = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.QR_CODE_DISPATCH)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .AddJsonBody("qrCode", scanVal)
                .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                .Execute()
                .To<object>();
            this.Vm.SelfInfo.clCode = qrCode.clCode;
            this.Vm.SelfInfo.pn = qrCode.pn;
            this.Vm.SelfInfo.lot = qrCode.lot;
            this.Vm.SelfInfo.qty = qrCode.qty;
            this.Vm.SelfInfo.dt = qrCode.dt;
            this.Vm.SelfInfo.dc = qrCode.dc;

            QrCodeNext(scanVal);
        }

        public virtual void ScanC02973Qrcode(string scanVal)
        {
            dynamic qrCode = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.QR_CODE_DISPATCH)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .AddJsonBody("qrCode", scanVal)
                .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                .Execute()
                .To<object>();
            this.Vm.SelfInfo.pn = qrCode.pn;
            this.Vm.SelfInfo.dc = qrCode.dc;
            this.Vm.SelfInfo.qty = qrCode.qty;

            QrCodeNext(scanVal);
        }

        public virtual void ScanC02975QrCode(string scanVal)
        {
            dynamic qrCode = RestApi.NewInstance(Method.POST)
                .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.QR_CODE_DISPATCH)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .AddJsonBody("qrCode", scanVal)
                .AddJsonBody("shipper", base.Vm.SelfInfo.shipper)
                .Execute()
                .To<object>();
            this.Vm.SelfInfo.qty = qrCode.qty;
            this.Vm.SelfInfo.pn = qrCode.pn;
            this.Vm.SelfInfo.lot = qrCode.lot;
            this.Vm.SelfInfo.dc = qrCode.dc;
            this.Vm.SelfInfo.dt = qrCode.dt;
            this.Vm.SelfInfo.isScanQrCode = true;
            this.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_DC_NO);

        }

        /// <summary>
        /// 是否需要输入COO
        /// 2018-10-10 COO不用錄入
        /// </summary>
        public virtual void QrCodeNext(string scanVal)
        {
            if (_cooConsignee.Contains(base.Vm.SelfInfo.consignee.ToString().ToUpper()))
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_COO);
            }
            else
            {
                this.Save();
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PN_OR_QRCODE);
            }
        }

        public virtual void ScanFlowActionScanCoo(string coo)
        {
            this.Vm.SelfInfo.coo = coo;
            this.Save();
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PN_OR_QRCODE);
        }

        public virtual void ScanFlowActionScanLotNo(string scanVal)
        {
            base.Vm.SelfInfo.useIntelligent = false;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.Other_DISPATCH)
                .AddBranch("LOT")
                .AddQueryParameter("lot", scanVal)
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.lot = result.lot;

            if (string.Compare(base.Vm.SelfInfo.shipper.ToString(), "C03690", true) == 0)
            {
                base.Vm.SelfInfo.dc = result.dc;

                dynamic lot = RestApi.NewInstance(Method.GET)
                    .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.DATE_CODE_DISPATCH)
                    .AddQueryParameter("dc", base.Vm.SelfInfo.dc)
                    .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                    .Execute()
                    .To<object>();

                base.Vm.SelfInfo.dc = lot.dc;
                base.Vm.SelfInfo.dt = lot.dt;
                base.Vm.SelfInfo.originDc = lot.originDc;
            }

            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_QTY);
        }

        public virtual void ScanFlowActionScanQty(string scanVal)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.Other_DISPATCH)
                .AddBranch("QTY")
                .AddQueryParameter("qty", scanVal)
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .AddQueryParameter("pn", base.Vm.SelfInfo.pn)
                .Execute()
                .To<object>();
            base.Vm.SelfInfo.qty = result.qty;
            QrCodeNext(scanVal);
        }

        public virtual void ScanFlowActionScanDcNo(string scanVal)
        {

            base.Vm.SelfInfo.useIntelligent = false;

            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUriParam(RestUrlType.WmsServer, (long)ScriptSid.DATE_CODE_DISPATCH)
                .AddQueryParameter("dc", scanVal)
                .AddQueryParameter("pid", base.Vm.SelfInfo.pid)
                .Execute()
                .To<object>();

            base.Vm.SelfInfo.dc = result.dc;
            base.Vm.SelfInfo.dt = result.dt;
            base.Vm.SelfInfo.originDc = result.originDc;

            if (this.Vm.SelfInfo.isScanQrCode == true)
            {
                QrCodeNext(scanVal);
            }
            else
            {
                base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_LOT_NO);
            }

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
                    .AddUri(RestUrlType.WmsServer, "reel-labeling/reel-delete")
                    .AddJsonBody("rid", (string)model.rid)
                    .AddJsonBody("sxt", (string)base.Vm.SelfInfo.sxt)
                    .AddJsonBody("pid", (string)base.Vm.SelfInfo.pid)
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
                    .AddUri(RestUrlType.WmsServer, "reel-labeling/reel-delete")
                    .AddJsonBody("rid", (string)model.rid)
                    .AddJsonBody("sxt", (string)base.Vm.SelfInfo.sxt)
                    .AddJsonBody("pid", (string)base.Vm.SelfInfo.pid)
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
                .AddUriParam(RestUrlType.WmsServer, 6420563238571745280, true)
                .AddJsonBody("rid", rid)
                .Execute()
                .To<object>();

            if (base.Vm.SelfInfo.isLabeling)
            {
                Print(label, true);
            }
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

        [Ability(6422073996916826112, AbilityType.ScanAction)]
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
                .AddUri(RestUrlType.WmsServer, "reel-labeling/reel-carton-end")
                .AddJsonBody("sxt", base.Vm.SelfInfo.sxt)
                .AddJsonBody("pid", base.Vm.SelfInfo.pid)
                .Execute();

            return true;
        }

        public virtual bool Save()
        {
            bool isSuccess = false;
            if (int.TryParse(base.Vm.SelfInfo.printCount, out int count))
            {
                if (count > 25)
                {
                    throw new WesException("批量打印最大數量25");
                }

                for (int i = 0; i < count; i++)
                {
                    if ((bool)base.Vm.SelfInfo.isLabeling)
                    {
                        isSuccess = SaveLabeing();
                    }
                    else
                    {
                        isSuccess = SaveLabeingForCheck();
                    }
                }
            }
            else
            {
                throw new WesException("PrintCount數據格式錯誤");
            }

            base.Vm.SelfInfo.printCount = "1";

            return isSuccess;
        }

        [AbilityAble(true, KPIActionType.LSChecking, "consignee")]
        public virtual bool SaveLabeingForCheck()
        {
            dynamic label = RestApi.NewInstance(Method.POST)
                .AddUri(RestUrlType.WmsServer, "reel-labeling/reel-save")
                .AddJsonBody(this.Vm.SelfInfo)
                .Execute()
                .To<object>();

            this.LoadingCarton(this.Vm.SelfInfo.pid);

            return true;
        }

        [AbilityAble(true, KPIActionType.LSLabeling, "consignee")]
        public virtual bool SaveLabeing()
        {
            dynamic label = RestApi.NewInstance(Method.POST)
                .AddUri(RestUrlType.WmsServer, "reel-labeling/reel-save")
                .AddJsonBody(this.Vm.SelfInfo)
                .Execute()
                .To<object>();

            this.LoadingCarton(this.Vm.SelfInfo.pid);

            Print(label, true);

            return true;
        }

        private void Print(dynamic label, bool isLock)
        {
            String labelName = (label.labelName + "");
            String[] labelNames = null;
            if (labelName.Contains(","))
            {
                labelNames = labelName.Split(',');
            }
            else
            {
                labelNames = new string[] { labelName };
            }

            List<PrintTemplateModel> templates = new List<PrintTemplateModel>();
            //添加對KPI統計的支持
            Vm.SelfInfo.labelCount = 0;

            foreach (string name in labelNames)
            {
                PrintTemplateModel ptm = new PrintTemplateModel();
                ptm.PrintData = label.context;
                ptm.TemplateFileName = name + ".btw";
                templates.Add(ptm);

                //對KPI數量進行統計
                Vm.SelfInfo.labelCount++;
            }

            LabelPrintBase labelPrint = new LabelPrintBase(templates);
            ErrorCode errorCode = labelPrint.Print();
            Debug.WriteLine($"打印返回CODE: {errorCode}");
        }

        private void PrivateBoxBnd(String scanValue)
        {
            String boxType = scanValue.ToUpper();
            List<dynamic> labels = RestApi.NewInstance(Method.PUT)
                .AddUri(RestUrlType.WmsServer, "reel-labeling/reel-box-end")
                .AddJsonBody("sxt", this.Vm.SelfInfo.sxt)
                .AddJsonBody("pid", this.Vm.SelfInfo.pid)
                .AddJsonBody("pn", this.Vm.SelfInfo.pn)
                .AddJsonBody("lot", this.Vm.SelfInfo.lot)
                .AddJsonBody("dc", this.Vm.SelfInfo.dc)
                .AddJsonBody("dt", this.Vm.SelfInfo.dt)
                .AddJsonBody("clCode", this.Vm.SelfInfo.clCode)
                .AddJsonBody("coo", this.Vm.SelfInfo.coo)
                .AddJsonBody("boxType", boxType)
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
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PN_OR_QRCODE);
        }

        private void LoadingCarton(String pid)
        {
            dynamic result = RestApi.NewInstance(Method.GET)
                .AddUri(RestUrlType.WmsServer, "reel-labeling")
                .AddQueryParameter("sxt", base.Vm.SelfInfo.sxt)
                .AddQueryParameter("pid", pid)
                .Execute()
                .To<object>();

            this.Vm.SelfInfo.isScanQrCode = false;

            base.Vm.SelfInfo.pid = (string)pid;
            base.Vm.SelfInfo.carton = (string)pid;
            base.Vm.SelfInfo.overQty = (int)result.overQty;
            base.Vm.SelfInfo.isLabeling = (bool)result.isLabeling;
            base.Vm.SelfInfo.cartonList = result.cartonList;
            base.Vm.SelfInfo.panList = result.panList;
            base.Vm.SelfInfo.isMasterReprint = false;
            base.Vm.SelfInfo.isMasterDelete = false;

            dynamic shipperObj = RestApi.NewInstance(Method.GET)
                .AddCommonUri(RestUrlType.WmsServer, "shipping/top1-detail2-by-pid")
                .AddQueryParameter("pid", pid)
                .Execute()
                .To<object>();
            if (shipperObj != null)
            {
                base.Vm.SelfInfo.shipper = shipperObj.shipper;
            }


            if (!(bool)result.isLabeling)
            {
                base.Vm.SelfInfo.OnlyCheckMessage = "(Checking)";
            }
            else
            {
                base.Vm.SelfInfo.OnlyCheckMessage = "(Labeling)";
            }

            BindingAutoComplateData();
        }

        //绑定PN/CLCode自动完成
        private void BindingAutoComplateData()
        {
            RestApi.NewInstance(Method.GET)
                .AddUri("/reel-labeling/part-end")
                .AddQueryParameter("sxt", (string)base.Vm.SelfInfo.sxt)
                .ExecuteAsync((res, exp, restApi) =>
                {
                    if (restApi != null)
                    {
                        dynamic listCode = restApi.To<object>();
                        ObservableCollection<BarCodeScanModel> souce = new ObservableCollection<BarCodeScanModel>();
                        for (int i = 0; i < listCode.Count; i++)
                        {
                            souce.Add(new BarCodeScanModel()
                            {
                                Name = listCode[i].ToString(),
                                Type = null,
                                Code = null
                            });
                        }

                        Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            base.Vm.SelfInfo.useIntelligent = true;
                            base.Vm.SelfInfo.intelligentItems = souce;
                        }));
                    }
                });
        }
    }
}