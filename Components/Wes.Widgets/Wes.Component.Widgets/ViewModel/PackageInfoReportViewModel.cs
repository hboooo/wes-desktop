using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Input;
using Wes.Component.Widgets.Action;
using Wes.Core;
using Wes.Core.Api;
using Wes.Core.ViewModel;
using Wes.Flow;

namespace Wes.Component.Widgets.ViewModel
{
    public class PackageInfoReportViewModel : ScanViewModelBase<WesFlowID, PackageInfoReportAction>, IViewModel
    {
        public ICommand SearchCommand { get; private set; }
        protected override WesFlowID GetFirstScan()
        {
            return WesFlowID.FLOW_ACTION_SCAN_PART_NO;
        }

        protected override PackageInfoReportAction GetAction()
        {
            var packageInfoAction = ViewModelFactory.CreateActoin<PackageInfoReportAction>() as PackageInfoReportAction;
            if (SearchCommand == null)
            {
                SearchCommand = new RelayCommand(packageInfoAction.SearchData);
            }
            return packageInfoAction;
        }

        protected override void InitNextTargets(Dictionary<WesFlowID, dynamic> nextTargets)
        {
            nextTargets.Add(WesFlowID.FLOW_ACTION_SCAN_PART_NO, new { tooltip = "PartNo" });
        }

        public string _searchPid;
        public string SearchPid
        {
            get { return _searchPid; }
            set
            {
                if (_searchPid != value)
                {
                    _searchPid = value;
                    RaisePropertyChanged<string>(nameof(SearchPid));
                }
            }
        }

        private string _searchPN;
        public string SearchPN
        {
            get { return _searchPN; }
            set
            {
                if (_searchPN != value)
                {
                    _searchPN = value;
                    RaisePropertyChanged<string>(nameof(SearchPN));
                }
            }
        }
    }
}
