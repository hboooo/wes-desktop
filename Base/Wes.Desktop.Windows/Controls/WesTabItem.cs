using System.Windows;
using System.Windows.Controls;

namespace Wes.Desktop.Windows.Controls
{
    public class WesTabItem : TabItem
    {
        public int FlowID { get; set; }

        public string FlowKey { get; set; }
        
        public int TitleFontSize
        {
            get { return (int)GetValue(TitleFontSizeProperty); }
            set { SetValue(TitleFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleFontSizeProperty =
            DependencyProperty.Register("TitleFontSize", typeof(int), typeof(WesTabItem), new PropertyMetadata(30));


    }
}
