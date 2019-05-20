using FirstFloor.ModernUI.Presentation;
using System.Windows;
using Wes.Utilities;

namespace Wes.Desktop.Windows
{
    public class BaseWindowViewModel : NotifyPropertyChanged
    {
        private string _userID;
        private string _endCustomerName = WesApp.GeneralAddIn;
        private string _endCustomerDetails;

        public BaseWindowViewModel()
        {
        }

        public string UserID
        {
            get { return _userID; }
            set
            {
                if (_userID != value)
                {
                    _userID = value;
                    OnPropertyChanged(nameof(UserID));
                }
            }
        }

        public string EndCustomerName
        {
            get { return _endCustomerName; }
            set
            {
                if (_endCustomerName != value)
                {
                    _endCustomerName = value;
                    OnPropertyChanged(nameof(EndCustomerName));
                }
            }
        }

        public string EndCustomerDetails
        {
            get { return _endCustomerDetails; }
            set
            {
                if (_endCustomerDetails != value)
                {
                    _endCustomerDetails = value;
                    OnPropertyChanged(nameof(EndCustomerDetails));
                }
            }
        }
    }
}