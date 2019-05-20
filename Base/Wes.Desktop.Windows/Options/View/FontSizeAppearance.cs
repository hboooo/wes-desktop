using FirstFloor.ModernUI.Presentation;
using System.Collections.Generic;
using System.Windows;

namespace Wes.Desktop.Windows.Options.View
{
    /// <summary>
    /// 字体大小设置
    /// </summary>
    public class FontSizeAppearance
    {
        /// <summary>
        /// 字体大小设置范围
        /// </summary>
        private static readonly HashSet<double> _fontSizeList = new HashSet<double>()
        {
            10,11,12,14,16,18,20,24,26,28,30
        };

        /// <summary>
        /// 字体大小, 默认12
        /// </summary>
        private static double _fontSize = 12;

        public static HashSet<double> FontSizeList
        {
            get
            {
                return _fontSizeList;
            }
        }

        public static double FontSize
        {
            get
            {
                return _fontSize;
            }
            private set
            {
                _fontSize = value;
            }
        }

        /// <summary>
        /// 设置字体大小
        /// </summary>
        /// <param name="size"></param>
        public static void SetFontSize(double size)
        {
            Application.Current.Resources[AppearanceManager.KeyDefaultFontSize] = size;
            Application.Current.Resources[AppearanceManager.KeyFixedFontSize] = size;
            FontSize = size;
        }
        
    }
}
