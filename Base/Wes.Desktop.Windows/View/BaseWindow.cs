using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wes.Desktop.Windows.View;

namespace Wes.Desktop.Windows
{
    public class BaseWindow : ModernWindow
    {
        #region 依赖属性
        public Visibility MaskVisibility
        {
            get { return (Visibility)GetValue(MaskVisibilityProperty); }
            set { SetValue(MaskVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaskVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaskVisibilityProperty =
            DependencyProperty.Register("MaskVisibility", typeof(Visibility), typeof(BaseWindow), new PropertyMetadata(Visibility.Collapsed));


        public string MaskIntensiveContent
        {
            get { return (string)GetValue(MaskIntensiveContentProperty); }
            set { SetValue(MaskIntensiveContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaskIntensiveContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaskIntensiveContentProperty =
            DependencyProperty.Register("MaskIntensiveContent", typeof(string), typeof(BaseWindow), new PropertyMetadata("請取標！"));

        public string MaskContent
        {
            get { return (string)GetValue(MaskContentProperty); }
            set { SetValue(MaskContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaskContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaskContentProperty =
            DependencyProperty.Register("MaskContent", typeof(string), typeof(BaseWindow), new PropertyMetadata("再掃描"));


        public string PrintLabelCount
        {
            get { return (string)GetValue(PrintLabelCountProperty); }
            set { SetValue(PrintLabelCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrintLabelCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrintLabelCountProperty =
            DependencyProperty.Register("PrintLabelCount", typeof(string), typeof(BaseWindow), new PropertyMetadata("0"));


        public BitmapImage LabelImageSource
        {
            get { return (BitmapImage)GetValue(LabelImageSourceProperty); }
            set { SetValue(LabelImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelImageSourceProperty =
            DependencyProperty.Register("LabelImageSource", typeof(BitmapImage), typeof(BaseWindow), new PropertyMetadata(null));
        #endregion

        public BaseWindow()
        {
            this.Style = (Style)Application.Current.Resources["wesBaseWindow"];
            this.CommandBindings.Add(new CommandBinding(BaseWindowCommand.Menu, MenuCommand));
            this.CommandBindings.Add(new CommandBinding(BaseWindowCommand.AddIn, AddInCommand));
            this.CommandBindings.Add(new CommandBinding(BaseWindowCommand.Robot, RobotCommand));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape && this.MaskVisibility != Visibility.Visible)
            {
                this.Close();
            }
        }

        protected virtual void MenuCommand(object sender, ExecutedRoutedEventArgs e)
        {

        }

        protected virtual void AddInCommand(object sender, ExecutedRoutedEventArgs e)
        {

        }

        protected virtual void RobotCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var robotWindow = WindowHelper.GetOpenedWindow<RobotWindow>();
            if (robotWindow == null)
            {
                RobotWindow setting = new RobotWindow();
                setting.Show();
            }
            else
            {
                robotWindow.WindowState = System.Windows.WindowState.Normal;
                robotWindow.Activate();
            }
        }
    }
}
