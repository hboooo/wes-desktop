using FirstFloor.ModernUI.Presentation;

namespace Wes.Desktop.Windows.Controls
{
    public class BarCodeScanModel : NotifyPropertyChanged
    {
        private string _name;
        private string _type;
        private string _code;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public string Code
        {
            get { return _code; }
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged(nameof(Code));
                }
            }
        }
    }
}
