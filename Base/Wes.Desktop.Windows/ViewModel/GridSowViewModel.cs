using GalaSoft.MvvmLight;
using System.Windows.Media;

namespace Wes.Desktop.Windows.ViewModel
{
    public class GridSowViewModel : ViewModelBase
    {
        private int _num;

        public int Num
        {
            get { return _num; }
            set
            {
                _num = value;
                RaisePropertyChanged(() => Num);
            }
        }

        private string _qtyDesc;

        public string QtyDesc
        {
            get { return _qtyDesc; }
            set
            {
                _qtyDesc = value;
                RaisePropertyChanged(() => QtyDesc);
            }
        }

        private string _newCartonNo;

        public string NCartonNo
        {
            get { return _newCartonNo; }
            set
            {
                _newCartonNo = value;
                RaisePropertyChanged(() => NCartonNo);
            }
        }

        private double _numFontSize;

        public double NumFontSize
        {
            get { return _numFontSize; }
            set
            {

                _numFontSize = value;
                RaisePropertyChanged(() => NumFontSize);
            }
        }

        private Brush _numForeground;
        public Brush NumForeground
        {
            get { return _numForeground; }
            set
            {

                _numForeground = value;
                RaisePropertyChanged(() => NumForeground);
            }
        }

        private Brush _nCartonForeground;
        public Brush NCartonForeground
        {
            get { return _nCartonForeground; }
            set
            {

                _nCartonForeground = value;
                RaisePropertyChanged(() => NCartonForeground);
            }
        }

        private Brush _background;

        public Brush Background
        {
            get { return _background; }
            set
            {

                _background = value;
                RaisePropertyChanged(() => Background);
            }
        }
    }
}
