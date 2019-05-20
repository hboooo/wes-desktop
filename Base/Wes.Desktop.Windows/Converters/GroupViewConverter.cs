using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Wes.Desktop.Windows.Converters
{
    public class GroupViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reels = value as IEnumerable<object>;
            if (reels == null)
                return "0";

            double sum = 0;
            double sumQty = 0;
            foreach (var u in reels)
            {
                if (((PackageInfoModel)u).Qty < 0) continue;
                sum += ((PackageInfoModel)u).Reels;
                sumQty += ((PackageInfoModel)u).Qty;
            }

            return sum + "                " + sumQty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PackageInfoModel
    {
        public PackageInfoModel()
        {
            PartNo = string.Empty;
            Reels = 0;
            Qty = 0;
            Type = string.Empty;
            LoadingNo = string.Empty;
            NCartonNo = string.Empty;
            LotNo = string.Empty;
            BatchNo = string.Empty;
            DateCode = string.Empty;
            TruckOrder = string.Empty;
        }
        public string PartNo { get; set; }
        public int Reels { get; set; }
        public int Qty { get; set; }
        public string LoadingNo { get; set; }
        public string NCartonNo { get; set; }
        public string Type { get; set; }
        public string LotNo { get; set; }
        public string DateCode { get; set; }
        public string BatchNo { get; set; }
        public string TruckOrder { get; set; }
    }
}
