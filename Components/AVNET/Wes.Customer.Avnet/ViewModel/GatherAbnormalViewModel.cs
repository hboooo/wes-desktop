using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Customer.Avnet.Action;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Utilities;
using Wes.Utilities.Excel;

namespace Wes.Customer.Avnet.ViewModel
{
    public class GatherAbnormalViewModel : ScanViewModelBase<WesFlowID, GatherAbnormalAction>, IViewModel
    {
        public ICommand PartNoDetailsCommand { get; private set; }

        public ICommand BtnSearchCommand { get; private set; }

        public GatherAbnormalViewModel()
        {
            this.SelfInfo.rcv = "";
            this.SelfInfo.doneList = new List<object>();
        }

        protected override GatherAbnormalAction GetAction()
        {
            var gatherAbnormalAction = ViewModelFactory.CreateActoin<GatherAbnormalAction>() as GatherAbnormalAction;
            if (gatherAbnormalAction != null)
            {
                PartNoDetailsCommand = new RelayCommand<dynamic>(gatherAbnormalAction.GetGetherPortNoDetails);
                BtnSearchCommand = new RelayCommand(gatherAbnormalAction.SearchClickCommand);
            }
            return gatherAbnormalAction;
        }

        protected override void TeamSupport()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);   //取得当前桌面路径
            DataSet ds = DynamicToDataSet();
            if (ds != null)
            {
                string detailsHeader = ds.Tables[0].TableName;
                string file = System.IO.Path.Combine(dir, string.Format("Details_{0}.xls", this.SelfInfo.rcv.ToString()));
                bool res = NPOIHelper.DataSetToExcel(ds, detailsHeader, file);
                if (res) WesModernDialog.ShowWesMessage(string.Format("导出成功,文件:{0}", file));
            }
            else
            {
                WesModernDialog.ShowWesMessage(string.Format("没有任何数据，请先查询数据"));
            }
        }

        private DataSet DynamicToDataSet()
        {
            DataSet dataSet = new DataSet();
            DataTable dt = new DataTable();
            dt.Columns.Add("PartNo");
            dt.Columns.Add("Quantity(PCS)");
            dt.Columns.Add("Done(PCS)");
            dt.Columns.Add("Differ(PCS)");
            dt.Columns.Add("IsEnabled");
            try
            {
                if (this.SelfInfo.doneList != null && this.SelfInfo.doneList.Count > 0)
                {
                    foreach (var item in this.SelfInfo.doneList)
                    {
                        DataRow dataRow = dt.NewRow();
                        dataRow[0] = item.partNo;
                        dataRow[1] = item.expectQty;
                        dataRow[2] = item.actualQty;
                        dataRow[3] = item.differQty;
                        dataRow[4] = "";
                        dt.Rows.Add(dataRow);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }
            dataSet.Tables.Add(dt);
            return dataSet;
        }

        protected override WesFlowID ScanInterceptor(string scanVal, WesFlowID cst)
        {
            return cst;
        }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_RECEIVING_NO, new { tooltip = "Receiving No" });
        }
    }
}
