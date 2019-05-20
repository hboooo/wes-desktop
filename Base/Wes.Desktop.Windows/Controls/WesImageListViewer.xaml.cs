using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections;

namespace Wes.Desktop.Windows.Controls
{
    /// <summary>
    /// WesImageListViewer.xaml 的交互逻辑
    /// </summary>
    public partial class WesImageListViewer : UserControl
    {
        public IEnumerable ImageSources
        {
            get { return (IEnumerable)GetValue(ImageSourcesProperty); }
            set { SetValue(ImageSourcesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSources.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourcesProperty =
            DependencyProperty.Register("ImageSources", typeof(IEnumerable), typeof(WesImageListViewer), new PropertyMetadata(default(IEnumerable), (obj, e) =>
            {
                WesImageListViewer imageView = obj as WesImageListViewer;
                if (imageView.ImageSources == null)
                    imageView.InitTextVisibility = Visibility.Visible;
                else
                    imageView.InitTextVisibility = Visibility.Collapsed;
            }));


        public Orientation ImageOrientation
        {
            get { return (Orientation)GetValue(ImageOrientationProperty); }
            set { SetValue(ImageOrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageOrientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageOrientationProperty =
            DependencyProperty.Register("ImageOrientation", typeof(Orientation), typeof(WesImageListViewer), new PropertyMetadata(Orientation.Vertical));


        public Visibility InitTextVisibility
        {
            get { return (Visibility)GetValue(InitTextVisibilityProperty); }
            set { SetValue(InitTextVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InitTextVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitTextVisibilityProperty =
            DependencyProperty.Register("InitTextVisibility", typeof(Visibility), typeof(WesImageListViewer), new PropertyMetadata(Visibility.Visible));
        
        public WesImageListViewer()
        {
            InitializeComponent();
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var img = sender as Image;
                string strUrl = img.Source.ToString();
                Process.Start(new ProcessStartInfo(strUrl));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
