using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Wes.Desktop.Windows;
using Wes.Utilities;
using Wes.Utilities.Languages;
using System.Linq;
using System.Collections.ObjectModel;
using Wes.Wrapper;
using Wes.Addins.Addin;
using Wes.Desktop.Windows.Updater;
using System.Threading.Tasks;

namespace Wes.Desktop.Windows.View
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : BaseWindow
    {
        public string ProductName
        {
            get { return (string)GetValue(ProductNameProperty); }
            set { SetValue(ProductNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProductName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProductNameProperty =
            DependencyProperty.Register("ProductName", typeof(string), typeof(AboutWindow), new PropertyMetadata(null));


        public string Company
        {
            get { return (string)GetValue(CompanyProperty); }
            set { SetValue(CompanyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Company.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompanyProperty =
            DependencyProperty.Register("Company", typeof(string), typeof(AboutWindow), new PropertyMetadata(null));


        public string Copyright
        {
            get { return (string)GetValue(CopyrightProperty); }
            set { SetValue(CopyrightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Copyright.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CopyrightProperty =
            DependencyProperty.Register("Copyright", typeof(string), typeof(AboutWindow), new PropertyMetadata(null));


        public string WesVersion
        {
            get { return (string)GetValue(WesVersionProperty); }
            set { SetValue(WesVersionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WesVersion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WesVersionProperty =
            DependencyProperty.Register("WesVersion", typeof(string), typeof(AboutWindow), new PropertyMetadata(null));


        public ObservableCollection<string> AddinItems
        {
            get { return (ObservableCollection<string>)GetValue(AddinItemsProperty); }
            set { SetValue(AddinItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddinItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddinItemsProperty =
            DependencyProperty.Register("AddinItems", typeof(ObservableCollection<string>), typeof(AboutWindow), new PropertyMetadata(null));


        public string LatestVersion
        {
            get { return (string)GetValue(LatestVersionProperty); }
            set { SetValue(LatestVersionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatestVersion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatestVersionProperty =
            DependencyProperty.Register("LatestVersion", typeof(string), typeof(AboutWindow), new PropertyMetadata(null));

        private WesUpdater updater = new WesUpdater();
        private bool _isNeedUpdate = false;

        public AboutWindow()
        {
            InitializeComponent();
            this.AddinItems = new ObservableCollection<string>();
            this.Loaded += AboutWindow_Loaded;
        }

        private void AboutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GetMainVersionInfo();
            GetAddinsVersionInfo();
            Task.Factory.StartNew(() =>
            {
                var latest = updater.GetLatestVersion();
                var current = DesktopVersion.GetVersionNo();
                if (string.Compare(latest, current) != 0 && !string.IsNullOrEmpty(latest))
                {
                    WesApp.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        LatestVersion = $"最新版本:{latest}";
                    }));
                    _isNeedUpdate = true;
                }
            });
        }

        private void GetMainVersionInfo()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyProductAttribute titleAttribute = (AssemblyProductAttribute)attributes[0];
                    if (titleAttribute.Product != "")
                    {
                        this.ProductName = titleAttribute.Product;
                    }
                }
                attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyCompanyAttribute titleAttribute = (AssemblyCompanyAttribute)attributes[0];
                    if (titleAttribute.Company != "")
                    {
                        this.Company = titleAttribute.Company;
                    }
                }
                attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyCopyrightAttribute titleAttribute = (AssemblyCopyrightAttribute)attributes[0];
                    if (titleAttribute.Copyright != "")
                    {
                        this.Copyright = titleAttribute.Copyright;
                    }
                }
                this.WesVersion = "Version".GetLanguage() + "  " + DesktopVersion.GetFullVersoinNo(assembly) + "\r\n发布时间" + "  " + WesDesktop.Instance.ReleaseDate;
            }
        }

        /// <summary>
        /// 获取插件信息
        /// </summary>
        private void GetAddinsVersionInfo()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var conf = ConfigurationMapping.Instance.GlobalConf;
            dynamic addIns = conf.addin;
            foreach (var item in addIns.mapping)
            {
                string name = item.name.ToString();
                string version = DesktopVersion.GetAddinFullVersionNo(name);
                if (string.IsNullOrEmpty(version))
                    version = "未加載";
                else
                    version = "Version".GetLanguage() + "  " + version;
                this.AddinItems.Add(string.Format("{0}- {1}", name + (item.desc == null ? "" : $" [{item.desc.ToString()}]"), version));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_isNeedUpdate)
            {
                var res = WesModernDialog.ShowMessage("在升级前要关闭工作站，确认要升级吗？", "WES升级提示", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    updater.WesUpdate();
                    WesModernDialog.ShowInfoMessage("当前已是最新版本");
                }
            }
            else
            {
                WesModernDialog.ShowInfoMessage("当前已是最新版本");
            }
        }
    }
}
