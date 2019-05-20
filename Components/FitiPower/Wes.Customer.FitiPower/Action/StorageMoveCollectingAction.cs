using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.Utils;
using Wes.Core.Base;
using Wes.Core.ViewModel;
using Wes.Customer.FitiPower.API;
using Wes.Desktop.Windows;
using Wes.Flow;
using Wes.Utilities.Languages;
using Wes.Wrapper;

namespace Wes.Customer.FitiPower.Action
{
    public class StorageMoveCollectingAction : SimpleScanAction<WesFlowID, StorageMoveCollectingAction>, IScanAction
    {
        public virtual void ScanReceivingNo(string value)
        {
            this.RefreshList();
            base.Vm.Next(WesFlowID.FLOW_ACTION_SCAN_PACKAGE_ID);
        }

        public virtual void ScanPackageId(string value)
        {
            this.Move(value);
        }

        public virtual void DeletePackage(dynamic rowData)
        {
            var result = WesModernDialog.ShowWesMessage($"確認要刪除箱{rowData.packageID}嗎?", "WES_Message".GetLanguage(),
                System.Windows.MessageBoxButton.OKCancel);
            if (result == System.Windows.MessageBoxResult.OK)
            {
                RestApi.NewInstance(Method.DELETE)
                    .AddUri("/move-storage")
                    .AddParams("rid", rowData.rowNo)
                    .Execute();
                RefreshList();
            }
        }

        protected virtual void Move(string value)
        {
            List<dynamic> labels = RestApi.NewInstance(Method.POST)
                .AddUri("/move-storage")
                .AddParams(base.Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            PrintUtil.PrivatePrint(this.Vm.SelfInfo, labels, false);
            RefreshList();
        }

        protected virtual void RefreshList()
        {
            List<dynamic> list = RestApi.NewInstance(Method.GET)
                .AddUri("/move-storage")
                .AddParams(base.Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
            base.Vm.SelfInfo.list = list;
        }
        //托盘操作完成的确认操作项
        public virtual void PalletEnd(string scanVal)
        {
            RestApi.NewInstance(Method.PUT)
                .AddUri("/move-storage")
                .AddParams(base.Vm.GetSelfInfo())
                .Execute()
                .To<List<object>>();
        }
    }
}