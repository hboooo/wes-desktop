using System.Windows;
using System.Windows.Controls;

namespace Wes.Desktop.Windows.Controls
{
    /// <summary>
    /// NavButton.xaml 的交互逻辑
    /// </summary>
    public partial class NavButton : UserControl
    {


        public int FlowID
        {
            get { return (int)GetValue(FlowIDProperty); }
            set { SetValue(FlowIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowIDProperty =
            DependencyProperty.Register("FlowID", typeof(int), typeof(NavButton), new PropertyMetadata(0));


        public string FlowKey
        {
            get { return (string)GetValue(FlowKeyProperty); }
            set { SetValue(FlowKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowKeyProperty =
            DependencyProperty.Register("FlowKey", typeof(string), typeof(NavButton), new PropertyMetadata(null));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NavButton), new PropertyMetadata("Undefined"));


        public string IconData
        {
            get { return (string)GetValue(IconDataProperty); }
            set { SetValue(IconDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register("IconData", typeof(string), typeof(NavButton), new PropertyMetadata(""));
        
        public NavButton()
        {
            InitializeComponent();
        }
        
    }
}
