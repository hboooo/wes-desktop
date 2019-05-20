using System;
using System.Globalization;
using System.Windows.Data;

namespace Wes.Desktop.Windows.Converters
{
    class ThirdValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
           Type targetType,
           object parameter,
           CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                throw new ArgumentException("value error");
            }

            double totalHeight = (double)values[0];
            double height = (double)values[1];
            return (object)((totalHeight - height) / 2.6);
        }

        public object[] ConvertBack(object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
