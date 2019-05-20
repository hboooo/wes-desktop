using GalaSoft.MvvmLight.Command;
using System.Windows.Controls;
using Wes.Component.Widgets.Action;
using Wes.Component.Widgets.ViewModel;
using Wes.Core;
using Wes.Customer.Sinbon.Action;

namespace Wes.Customer.Sinbon.ViewModel
{
    public class SinbonPalletizationViewModel : PalletizationViewModel
    {
        protected override PalletizationAction GetAction()
        {
            var shippingPallteAction = ViewModelFactory.CreateActoin<SinbonPalletizationAction>() as SinbonPalletizationAction;
            if (UndoPackageGridLoadingRow == null)
            {
                UndoPackageGridLoadingRow = new RelayCommand<DataGridRowEventArgs>(shippingPallteAction.DataGridLoadingRow);
            }
            return shippingPallteAction;
        }
    }
}
