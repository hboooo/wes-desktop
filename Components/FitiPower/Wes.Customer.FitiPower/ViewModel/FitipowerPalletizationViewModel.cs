using GalaSoft.MvvmLight.Command;
using System.Windows.Controls;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;
using Wes.Customer.FitiPower.Action;

namespace Wes.Customer.FitiPower.ViewModel
{
    public class FitipowerPalletizationViewModel : PalletizationViewModel
    {
        protected override PalletizationAction GetAction()
        {
            var shippingPallteAction = ViewModelFactory.CreateActoin<FitipowerPalletizationAction>() as FitipowerPalletizationAction;
            if (UndoPackageGridLoadingRow == null)
            {
                UndoPackageGridLoadingRow = new RelayCommand<DataGridRowEventArgs>(shippingPallteAction.DataGridLoadingRow);
            }
            return shippingPallteAction;
        }
    }
}
