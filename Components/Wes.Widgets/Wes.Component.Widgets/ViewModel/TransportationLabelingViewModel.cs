using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Wes.Component.Widgets.Action;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Desktop.Windows.Common;
using Wes.Flow;

namespace Wes.Component.Widgets.ViewModel
{
    public class TransportationLabelingViewModel : ScanViewModelBase<WesFlowID, TransportationLabelingAction>, IViewModel
    {
        public ICommand RePrintCommand { get; private set; }

        protected override TransportationLabelingAction GetAction()
        {
            var transportationLabelingAction = ViewModelFactory.CreateActoin<TransportationLabelingAction>() as TransportationLabelingAction;
            RePrintCommand = new RelayCommand<object>(transportationLabelingAction.RePrint);
            return transportationLabelingAction;
        }

        protected override WesFlowID ScanInterceptor(string scanVal, WesFlowID cst)
        {
            return cst;
        }

        protected override bool BeforeScanInvoke(string scanVal)
        {
            return true;
        }

        protected override void TeamSupport()
        {
            var teamSupport = new WesTeamSupport(WesFlowID.FLOW_OUT_TRANSPORTATION_LABELING);
            teamSupport.ShowDialog();
        }

        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_TRUCK_NO;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_TRUCK_NO, "");
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PALLET_OR_CARTON, "");
        }
    }
}
