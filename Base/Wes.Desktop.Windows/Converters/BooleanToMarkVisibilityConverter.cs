using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Wes.Desktop.Windows.Converters
{
    public class BooleanToMarkVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibily = Visibility.Collapsed;
            bool isEnable = (bool)value;
            if (isEnable)
                visibily = Visibility.Collapsed;
            else
                visibily = Visibility.Visible;
            return visibily;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
